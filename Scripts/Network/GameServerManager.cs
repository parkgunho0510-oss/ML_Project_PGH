using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Transporting;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Wooduduk.Data;
using Wooduduk.Data.DataSO;
using Wooduduk.Data.Runtime;
using FishNet.Object;
using Wooduduk.Network.Match;
using Wooduduk.Data.DataSystem;
using Wooduduk.Map;

namespace Wooduduk.Network
{
    /// <summary>
    /// 서버 진입점.
    /// MatchRegistry, MatchAllocator,
    /// PlayerNetworkManager를 초기화
    /// 서버 Tick을 실행
    /// </summary>
    public class GameServerManager : MonoBehaviour
    {
        [SerializeField] private DataManagerSO _dataManager;
        [SerializeField] private NetworkObject _botPrefab; // 인스펙터에서 드래그
        [SerializeField] private NetworkObject _treePrefab;

        /// <summary>
        /// 플레이어를 적절한 MatchSession에 배정
        /// </summary>
        private MatchAllocator _matchAllocator;

        /// <summary>
        /// 서버에 존재하는 모든 MatchSession 저장소
        /// </summary>
        private MatchRegistry _matchRegistry;

        /// <summary>
        /// NetworkPlayer 관리
        /// </summary>
        private PlayerNetworkManager _playerNetworkManager;

        private NetworkManager _networkManager;

        /// <summary>
        /// clientId → session 빠른 역조회용 캐시
        /// </summary>
        private readonly Dictionary<int, MatchSession> _clientMatchLookup = new();

        private const float TICK_RATE = 0.02f; // 서버 시뮬레이션 TPS / 50 TPS = 0.02초
        private float _tickTimer;

        public static GameServerManager Instance { get; private set; }
        public PlayerNetworkManager PlayerNetworkManager => _playerNetworkManager;

        public MatchRegistry MatchRegistry => _matchRegistry;
        public bool IsServerRunning => _networkManager != null && _networkManager.IsServerStarted;
        public float TickRate => TICK_RATE;

        private void Awake()
        {
            if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
            else { Destroy(gameObject); return; }
        }

        private void Start()
        {
            _networkManager = InstanceFinder.NetworkManager;
            if (_networkManager == null)
            {
                Debug.LogError("FishNet NetworkManager 없음");
                return;
            }

            // 서버 상태 이벤트
            _networkManager.ServerManager.OnServerConnectionState += OnServerState;
            // 클라이언트 입장
            _networkManager.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;
        }

        private void OnDestroy()
        {
            if (_networkManager == null)
                return;
            _networkManager.ServerManager.OnServerConnectionState -= OnServerState;
            _networkManager.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
        }

        /// <summary>
        /// 서버 시작 감지
        /// </summary>
        private void OnServerState(ServerConnectionStateArgs args)
        {
            if (args.ConnectionState != LocalConnectionState.Started)
                return;

            Debug.Log("[SERVER] 서버 시작");

            InitializeServerSystems();
        }

        /// <summary>
        /// 서버 시스템 초기화
        /// </summary>
        private void InitializeServerSystems()
        {
            Debug.Log("[SERVER] 시스템 초기화 시작");

            _matchRegistry = new MatchRegistry();
            _matchAllocator = new MatchAllocator(_matchRegistry);
            _playerNetworkManager = new PlayerNetworkManager();

            InjectZonePhases();

            CreateDefaultMatch();

            Debug.Log("[SERVER] 시스템 초기화 완료");
        }

        /// <summary>
        /// Zone 데이터 주입
        /// 모든 MatchSession이 공유
        /// </summary>
        private void InjectZonePhases()
        {
            List<ZoneData> phases = new();

            foreach (var zone in _dataManager.GetZonePhases())
            {
                phases.Add(zone._data);
            }

            _matchRegistry.SetZonePhases(phases);
        }

        /// <summary>
        /// 서버 기본 Match 생성
        /// </summary>
        private void CreateDefaultMatch()
        {
            MatchConfigData config = _dataManager.GetDefaultMatch()?._data;

            if (config == null)
            {
                Debug.LogError("MatchConfig 없음");
                return;
            }
            _matchRegistry.CreateMatch(config);

            Debug.Log("[MATCH] 기본 Match 생성 완료");
        }

        /// <summary>
        /// FishNet 접속 이벤트
        /// </summary>
        private void OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
        {
            switch (args.ConnectionState)
            {
                case RemoteConnectionState.Started:
                    OnClientConnected(conn);
                    break;

                case RemoteConnectionState.Stopped:
                    OnClientDisconnected(conn);
                    break;
            }
        }

        private void Update()
        {
            if (_networkManager == null) return;
            if (!_networkManager.IsServerStarted) return;

            _tickTimer += Time.deltaTime;

            if (_tickTimer >= TICK_RATE)
            {
                _tickTimer -= TICK_RATE;
                Tick();
            }
        }

        /// <summary>
        /// 서버 시뮬레이션 루프
        /// </summary>
        private void Tick()
        {
            if (_matchRegistry.AllMatches.Count == 0)
                return;

            List<MatchSession> sessions = new();

            foreach (var session in _matchRegistry.AllMatches.Values)
            {
                sessions.Add(session);
            }

            foreach (var session in sessions)
            {
                session.Tick(TICK_RATE);

                SyncPlayers(session);
            }
        }

        /// <summary>
        /// 로그인 완료 후 호출
        /// 플레이어를 Match에 배정
        /// </summary>
        public void RegisterPlayer(int connectionId, string firebaseUid)
        {

            MatchConfigData config = _dataManager.GetDefaultMatch()?._data;

            if (config == null)
            {
                Debug.LogError("MatchConfig 없음");
                return;
            }
            UserData userData = (UserManager.Instance != null && UserManager.Instance.TryGetUser(out var u) && u._userId == firebaseUid)
                ? u : new UserData { _userId = firebaseUid, _equippedWeaponId = "W001", _equippedTraitId = "" };
            Debug.Log($"USER = {userData._userId}");
            Debug.Log($"WEAPON = {userData._equippedWeaponId}");
            Debug.Log($"TRAIT = {userData._equippedTraitId}");

            var character = _dataManager.GetCharacterData("C001");

            var weapon = _dataManager.GetWeaponData(userData._equippedWeaponId);

            PlayerRuntimeData runtime = PlayerFactory.Create(userData, character, weapon, _dataManager);

            PlayerSpawnProvider.AssignOrDefer(runtime);

                
            runtime._position = PlayerSpawnProvider.Next();
            MatchSession session = _matchAllocator.Allocate(connectionId, firebaseUid, runtime, config);

            _clientMatchLookup[connectionId] = session;

            Debug.Log($"[MATCH] {firebaseUid} -> {session.State._matchId}");
        }

        /// <summary>
        /// Runtime → NetworkPlayer 동기화
        /// </summary>
        private void SyncPlayers(MatchSession session)
        {
            foreach (var playerPair in session.Players)
            {
                PlayerRuntimeData runtime = playerPair.Value;

                var networkPlayer = _playerNetworkManager.GetByUserId(runtime._userId);

                if (networkPlayer == null) continue;

                networkPlayer.SetServerPosition(runtime._position);

                networkPlayer.SetServerHp(runtime._currentMaxHp, runtime._currentHp);

                networkPlayer.SetServerWood(runtime._collectedWood);
            }
        }

        /// <summary>
        /// 클라이언트 접속 처리
        /// </summary>
        private void OnClientConnected(NetworkConnection conn)
        {
            Debug.Log($"[CONNECT] {conn.ClientId}");
        }

        /// <summary>
        /// 클라이언트 연결 종료 처리
        /// </summary>
        private void OnClientDisconnected(NetworkConnection conn)
        {
            int clientId = conn.ClientId;

            if (!_clientMatchLookup.TryGetValue(clientId, out MatchSession session)) return;

            if (!session.ClientToUser.TryGetValue(clientId, out string userId)) return;

            session.RemovePlayer(userId, clientId);

            _clientMatchLookup.Remove(clientId);

            Debug.Log($"[DISCONNECT] {userId}");
        }

        /// <summary>
        /// NetworkPlayer → MatchSession 입력 전달
        /// </summary>
        public void OnReceiveInput(int connectionId, Vector2 input)
        {
            if (!_clientMatchLookup.TryGetValue(connectionId, out MatchSession session))
            {
                return;
            }
            session.SetInput(connectionId, input);
        }

        // 탈출 요청 : NetworkPlayer -> 세션 검증 -> 결과 회신
        internal void OnRequestExtract(int connectionId)
        {
            if (!_clientMatchLookup.TryGetValue(connectionId, out MatchSession session)) return;
            var player = session.GetPlayerByClientId(connectionId);


            bool success = player != null && session.TryExtract(player);
            var np = _playerNetworkManager.Get(connectionId);
            np?.NotifyExtractResult(success);            // 결과 회신 
            if (success) np?.DespawnSelf();              // 서버 권위 디스폰
        }
        public void DespawnExtracted(PlayerRuntimeData player)
        {
            if (player == null) return;
            var np = _playerNetworkManager.GetByUserId(player._userId);
            if (np != null)
            {
                np.DespawnForExtract();
            }

        }
        public void OnRequestPickup(int clientId, int amount)
        {
            if (!_clientMatchLookup.TryGetValue(clientId, out MatchSession session)) return;

            PlayerRuntimeData player = session.GetPlayerByClientId(clientId);
            if (player == null || player._isDead || player._isExtracted) return;

            int clamped = Mathf.Clamp(amount, 1, 1); // 일단 요청당 1개 고정
            PlayerSystem.AddWood(player, clamped);
        }



        #region Debug

        public void DebugCreateMatch()
        {
            if (_dataManager == null || _matchRegistry == null)
                return;

            var config = _dataManager.GetDefaultMatch()?._data;

            if (config == null)
                return;

            _matchRegistry.CreateMatch(config);
        }

        public void DebugRemoveAllMatches()
        {
            if (_matchRegistry == null)
                return;

            var keys = new List<string>(_matchRegistry.AllMatches.Keys);

            foreach (var key in keys)
            {
                _matchRegistry.RemoveMatch(key);
            }
        }

        public void DebugEndMatch(string matchId)
        {
            if (_matchRegistry == null)
                return;

            _matchRegistry.RemoveMatch(matchId);
        }

        public void DebugSpawnTree(string matchId)
        {
            var session = _matchRegistry?.GetMatch(matchId);
            if (session == null || _treePrefab == null) return;

            var treeData = _dataManager.GetTreeData("T001");
            if (treeData == null) return;

            // 맵 범위 내 랜덤 위치 계산 (이게 핵심!)
            Vector2 r = Random.insideUnitCircle * session.Config._mapRadius;
            Vector3 pos = new Vector3(r.x, 0, r.y);

            // ID 생성
            string treeId = $"TREE_{System.Guid.NewGuid():N}";

            // MatchSession의 등록/스폰 메서드 호출 (새로 만든 그 메서드!)
            session.RegisterAndSpawnObject(_treePrefab, pos, treeId);

            // 데이터 매핑
            // treeData.Clone()이나 Factory를 통해 ID를 세팅해주어야 함
            var tree = TreeFactory.Create(treeData, pos);
            tree._treeInstanceId = treeId;

            session.AddTree(tree);
        }

        public void DebugSpawnTrees(string matchId, int count)
        {
            var session = _matchRegistry?.GetMatch(matchId);
            if (session == null) return;

            for (int i = 0; i < count; i++)
            {
                DebugSpawnTree(matchId);
            }
        }

        public void DebugRemoveTrees(string matchId, int count)
        {
            var session = _matchRegistry?.GetMatch(matchId);
            if (session == null) return;

            var keys = session.Trees.Keys.Take(count).ToList();
            foreach (var id in keys)
            {
                session.DespawnAndUnregisterObject(id); // 네트워크 삭제
                session.RemoveTree(id);                 // 데이터 삭제
            }
        }

        public void DebugAddBot(string matchId)
        {
            var session = _matchRegistry?.GetMatch(matchId);
            if (session == null || _botPrefab == null) return;

            var character = _dataManager.GetCharacterData("C001");
            var weapon = _dataManager.GetWeaponData("W001");

            if (character == null || weapon == null)
            {
                Debug.LogError("데이터 없음");
                return;
            }

            var botUser = new UserData { _userId = $"BOT_{System.Guid.NewGuid():N}" };
            var bot = PlayerFactory.Create(botUser, character, weapon, _dataManager);
            bot._isBot = true;

            // 1. 데이터 등록 (ID를 bot._userId로 사용)
            session.AddPlayer(bot._userId, bot);

            // 2. 네트워크 객체 스폰
            Vector2 r = Random.insideUnitCircle * session.Config._mapRadius;
            session.RegisterAndSpawnObject(_botPrefab, new Vector3(r.x, 0, r.y), bot._userId);

            Debug.Log($"[DEBUG] 봇 생성 성공: {bot._userId}");
        }

        public void DebugAddBots(string matchId, int count)
        {
            for (int i = 0; i < count; i++)
                DebugAddBot(matchId);
        }

        public void DebugRemoveBot(string matchId)
        {
            var session = _matchRegistry?.GetMatch(matchId);
            if (session == null) return;

            var bots = session.Players.Values.Where(p => p._isBot).ToList();
            if (bots.Count > 0)
            {
                // 이제 RemovePlayer가 내부적으로 Despawn까지 다 처리합니다.
                session.RemovePlayer(bots[0]._userId, -1);
            }
        }

        public void DebugKillAllBots(string matchId)
        {
            var session = _matchRegistry?.GetMatch(matchId);
            if (session == null) return;

            // 봇 ID 리스트를 먼저 추출 (Remove 도중 열거형 수정 방지)
            var botIds = session.Players.Values.Where(p => p._isBot).Select(p => p._userId).ToList();

            foreach (var id in botIds)
            {
                // 중요: RemovePlayer는 내부에서 Despawn을 호출함
                session.RemovePlayer(id, -1);
            }
        }

        #endregion
    }
}