using FishNet;
using FishNet.Object;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wooduduk.Data;
using Wooduduk.Data.DataSystem;
using Wooduduk.Data.Runtime;
using Wooduduk.Network.NetworkData;

namespace Wooduduk.Network.Match
{
    /// <summary>
    /// 한 판(레이드)의 전체 실시간 상태.
    /// Players(사람+봇), Trees(고정나무+정령), FireZone, MatchRuntimeData
    /// </summary>
    public class MatchSession
    {
        private readonly Dictionary<string, NetworkObject> _networkObjects = new();
        /// <summary>
        /// 매치 전체 상태(시간/탈출개방 등)
        /// </summary>
        public MatchState State { get; private set; }

        /// <summary>
        /// 불 경계(산불) 실시간 상태
        /// </summary>
        public FireZoneRuntimeData FireZone { get; private set; }

        /// <summary>
        /// 플레이어 목록 (사람 + 봇, 봇은 _isBot=true)
        /// Key : playerId (사람=userId, 봇=생성 인스턴스ID)
        /// </summary>
        public Dictionary<string, PlayerRuntimeData> Players { get; } = new();

        /// <summary>
        /// 나무 목록 (고정 나무 + 정령, 정령은 _currentMoveSpeed>0)
        /// Key : _treeInstanceId
        /// </summary>
        public Dictionary<string, TreeRuntimeData> Trees { get; } = new();

        /// <summary>
        /// 결과값 전달
        /// </summary>
        public List<MatchResultData> Results { get; private set; } = new();

        /// <summary>
        /// NGO ClientId -> Firebase UserId
        /// </summary>
        public Dictionary<int, string> ClientToUser = new();

        /// <summary>
        /// UserId → ClientId 역매핑 (디버깅/전송용)
        /// </summary>
        public Dictionary<string, int> UserToClient = new();

        /// <summary>
        /// 이 세션의 룰 세팅 (가장 중요)
        /// </summary>
        public MatchConfigData Config { get; private set; }

        /// <summary>
        /// 입력 버퍼 (클라이언트 → 서버 틱 처리용)
        /// </summary>
        public Dictionary<string, Vector2> PendingInputs { get; } = new();

        // 존 축소 페이즈 (StartTime 오름차순) + 맵 반경
        private readonly List<ZoneData> _zonePhases;
        private readonly float _mapRadius;

        // 매치 정보 전달
        public MatchSession(string matchId, MatchConfigData config, List<ZoneData> zonePhases)
        {
            State = new MatchState { _matchId = matchId };

            Config = config;
            _zonePhases = zonePhases;
            _mapRadius = config._mapRadius;

            FireZone = new FireZoneRuntimeData
            {
                _center = Vector3.zero,        // 맵 중심 (필요 시 config/맵에서 받아오기)
                _currentRadius = _mapRadius    // 시작은 풀 반경
            };
        }

        public void SetInput(int clientId, Vector2 input)
        {
            if (!ClientToUser.TryGetValue(clientId, out var userId))
                return;

            PendingInputs[userId] = input;
        }

        public void Tick(float dt)
        {
            foreach (var kv in Players)
            {
                string userId = kv.Key;
                var player = kv.Value;

                if (player._isDead) continue;

                PendingInputs.TryGetValue(userId, out var input);

                ApplyMovement(player, input, dt);
            }

            PendingInputs.Clear();

            State._elapsedTime += dt;
            if (!State._extractionOpened && State._elapsedTime >= Config._extractionOpenTime)
            {
                State._extractionOpened = true;
                Debug.Log("탈출구 개방");
            }

            // 존 축소 + 경계 밖 데미지
            UpdateZone(dt);

            // 시간 초과(20분) = 전역 산불
            if (State._elapsedTime > Config._sessionTime)
            {
                ApplyTimeOverDamage(dt);
            }
        }

        // 경과 시간으로 현재 반경/데미지 계산 → FireZone 갱신 + 경계 밖 플레이어 데미지
        private void UpdateZone(float dt)
        {
            if (_zonePhases == null || _zonePhases.Count == 0) return;

            float ratio = 1f;
            float outDamage = 0f;
            float prevTarget = 1f;
            float t = State._elapsedTime;

            foreach (var phase in _zonePhases) // StartTime 오름차순 전제
            {
                if (t < phase._startTime) break; // 아직 시작 안 된 페이즈

                outDamage = phase._outDamage;

                if (phase._shrinkDuration > 0f && t < phase._startTime + phase._shrinkDuration)
                {
                    float k = (t - phase._startTime) / phase._shrinkDuration;
                    ratio = Mathf.Lerp(prevTarget, phase._targetRatio, k); // 줄어드는 중
                }
                else
                {
                    ratio = phase._targetRatio; // 다 줄어든 상태
                }

                prevTarget = phase._targetRatio;
            }

            float radius = _mapRadius * ratio;
            FireZone._currentRadius = radius;

            if (outDamage <= 0f) return;

            float radiusSqr = radius * radius;
            foreach (var player in Players.Values)
            {
                if (player._isDead) continue;

                Vector3 d = player._position - FireZone._center;
                float distSqr = d.x * d.x + d.z * d.z; // 수평(XZ) 거리

                if (distSqr > radiusSqr) // 산불 밖
                {
                    player._currentHp -= outDamage * dt;
                    if (player._currentHp <= 0f)
                    {
                        player._currentHp = 0f;
                        player._isDead = true;
                    }
                }
            }
        }

        // 매치 내 이동
        private void ApplyMovement(PlayerRuntimeData player, Vector2 input, float dt)
        {
            // 네트워크 객체 찾기
            var netObj = GetNetworkObject(player._userId);

            // 물리 오브젝트(Rigidbody)가 존재한다면 물리 엔진을 통해 이동
            if (netObj != null && netObj.TryGetComponent<Rigidbody>(out var rb))
            {
                Vector3 dir = new Vector3(input.x, 0, input.y);
                if (dir.sqrMagnitude > 1f) dir.Normalize();

                // 서버의 물리 엔진이 충돌을 계산하게 함
                rb.linearVelocity = new Vector3(
                    dir.x * player._currentMoveSpeed,
                    rb.linearVelocity.y, // 중력/수직 속도 유지
                    dir.z * player._currentMoveSpeed);

                // 물리 엔진이 계산한 최종 위치를 RuntimeData에 반영
                // 이렇게 해야 서버는 캐릭터가 나무/벽에 막혔는지 정확히 알 수 있음
                player._position = rb.position;

                // 클라이언트에 동기화 (NetworkPlayer의 SyncVar/Rpc 호출)
                if (netObj.TryGetComponent<NetworkPlayer>(out var netPlayer))
                {
                    netPlayer.SetServerPosition(player._position);
                }
            }
            else
            {
                // 물리 오브젝트가 없을 경우(혹은 예외) 기존 방식 사용
                Vector3 dir = new Vector3(input.x, 0, input.y);
                if (dir.sqrMagnitude > 1f) dir.Normalize();
                player._position += dir * player._currentMoveSpeed * dt;
            }
        }

        // 시간 초과시 데미지
        private void ApplyTimeOverDamage(float dt)
        {
            foreach (var player in Players.Values)
            {
                if (player._isDead || player._isExtracted) continue;

                player._currentHp -= Config._timeOverDamage * dt;

                if (player._currentHp <= 0)
                {
                    player._currentHp = 0;
                    player._isDead = true;
                }
            }
        }

        // 탈출시 결과값 전달
        public void AddResult(MatchResultData result)
        {
            Results.Add(result);
        }

        // 탈출 판정 (서버). 플레이어 = GetPlayerByClientId 경유, 봇 = AI가 직접 호출 가능
        public bool TryExtract(PlayerRuntimeData player)
        {
            if (player == null) return false;
            if (!State._extractionOpened) return false;
            if (player._isDead || player._isExtracted ) return false;
            // TODO 맵 탈출구 소켓 연동
            // 출구 위치 등록 API 추가 후 플레이어 포지션과 출구 거리 검증 삽입
            // 정산 연계
            PlayerSystem.Extract(player); 
            return true;
        }

        // Firebase 등록된 id 찾기
        public PlayerRuntimeData GetPlayerByClientId(int clientId)
        {
            if (!ClientToUser.TryGetValue(clientId, out string userId))
                return null;

            if (!Players.TryGetValue(userId, out PlayerRuntimeData player))
                return null;

            return player;
        }

        /// <summary>
        /// 플레이어/봇 추가
        /// </summary>
        public void AddPlayer(string playerId, PlayerRuntimeData player)
        {
            Players[playerId] = player;
        }

        /// <summary>
        /// 플레이어/봇 제거
        /// </summary>
        public void RemovePlayer(string playerId, int clientId)
        {
            DespawnAndUnregisterObject(playerId);
            Players.Remove(playerId);

            // 봇은 clientId가 없을 수 있으므로 체크 추가
            if (clientId != -1)
            {
                ClientToUser.Remove(clientId);
                UserToClient.Remove(playerId);
            }

            PendingInputs.Remove(playerId);
        }

        /// <summary>
        /// 나무 추가
        /// </summary>
        public void AddTree(TreeRuntimeData tree)
        {
            Trees[tree._treeInstanceId] = tree;
        }

        /// <summary>
        /// 나무 제거 (파괴/정령 처치 시)
        /// </summary>
        public void RemoveTree(string treeInstanceId)
        {
            // 나무도 네트워크 객체이므로 반드시 Despawn해야 함
            DespawnAndUnregisterObject(treeInstanceId);
            Trees.Remove(treeInstanceId);
        }

        // 객체 스폰 및 등록
        public void RegisterAndSpawnObject(NetworkObject prefab, Vector3 position, string id)
        {
            NetworkObject netObj = Object.Instantiate(prefab, position, Quaternion.identity);
            InstanceFinder.ServerManager.Spawn(netObj);
            _networkObjects[id] = netObj;
        }

        // 객체 Despawn 및 제거
        public void DespawnAndUnregisterObject(string id)
        {
            if (_networkObjects.TryGetValue(id, out NetworkObject netObj))
            {
                if (netObj != null && netObj.IsSpawned)
                    netObj.Despawn();
                _networkObjects.Remove(id);
            }
        }

        // 매치 종료 시 모든 객체 정리
        public void Cleanup()
        {
            // 모든 객체(플레이어, 봇, 나무)를 순회하며 제거
            var playerIds = Players.Keys.ToList();
            foreach (var id in playerIds)
            {
                // clientId가 없으면 -1 처리
                RemovePlayer(id, -1);
            }

            var treeIds = Trees.Keys.ToList();
            foreach (var id in treeIds)
            {
                RemoveTree(id); // 이것도 Despawn을 포함해야 함
            }
        }

        // 특정 ID의 객체 반환
        public NetworkObject GetNetworkObject(string id) => _networkObjects.TryGetValue(id, out var netObj) ? netObj : null;
    }
}
