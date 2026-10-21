using Firebase.Auth;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using UnityEngine;
using Wooduduk.Core;
using Wooduduk.Data.DataSO;
using Wooduduk.Data.DataSystem;
using Wooduduk.Data.Runtime;
using Wooduduk.Player;

namespace Wooduduk.Network.NetworkData
{
    [RequireComponent(typeof(PlayerMovement))]
    public class NetworkPlayer : NetworkBehaviour
    {
        [SerializeField] private DataManagerSO _dataManager;
        private PlayerMovement _movement;

        private readonly SyncVar<Vector3> _syncedPosition = new();
        private readonly SyncVar<string> _userId = new();
        private readonly SyncVar<float> _maxHp = new();
        private readonly SyncVar<float> _currentHp = new();
        private readonly SyncVar<int> _collectedWood = new();

        private WoodBag _woodBag;
        private Vector2 _lastSentInput;
        public string UserId => _userId.Value;

        private void Awake()
        {
            _movement = GetComponent<PlayerMovement>();
            _woodBag = GetComponent<WoodBag>();
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            // SyncVar Hook 등록
            _syncedPosition.OnChange += OnSyncPosition;
            _collectedWood.OnChange += OnSyncWood;
        }

        public override void OnStopNetwork()
        {
            base.OnStopNetwork();
            // 메모리 누수 방지를 위한 Hook 해제
            _syncedPosition.OnChange -= OnSyncPosition;
            _collectedWood.OnChange -= OnSyncWood;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (IsOwner) InitializeClient();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            Debug.Log(
        $"Spawned Player " +
        $"OwnerId={Owner.ClientId} " +
        $"ObjectId={NetworkObject.ObjectId}");
        }

        public override void OnStopServer()
        {
            base.OnStopServer();

            if (GameServerManager.Instance == null) return;

            GameServerManager.Instance.PlayerNetworkManager.Unregister(Owner.ClientId, _userId.Value);

            Debug.Log($"NetworkPlayer 제거 : {Owner.ClientId}");
        }

        private void InitializeClient()
        {
            FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
            if (user != null) LoginServerRpc(user.UserId);
            ApplyLoadoutToStats();
        }

        private void Update()
        {
            if (!IsOwner)
                return;

            Vector2 input = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical"));

            // 입력이 바뀔 때만 전송 (멈춤=0도 한 번 전송) → 트래픽 절감
            if (input != _lastSentInput)
            {
                _lastSentInput = input;
                SendInputServerRpc(input);
            }
        }
        private void ApplyLoadoutToStats()
        {
            var stats = GetComponent<PlayerStats>();
            if (stats == null || _dataManager == null) return;
            if (UserManager.Instance == null || !UserManager.Instance.TryGetUser(out var ud)) return;

            var charactor = _dataManager.GetCharacterData("C001");
            var weaponId = string.IsNullOrEmpty(ud._equippedWeaponId) ? "W001" : ud._equippedWeaponId;
            var weapon = _dataManager.GetWeaponData(weaponId);

            var runtime = PlayerFactory.Create(ud, charactor, weapon, _dataManager);
            stats.Initialize(new PlayerRuntimeBaseStats(runtime));
        }
        private void OnSyncWood(int oldValue, int newValue, bool asServer)
        {
            if (asServer) return;
            _woodBag?.SetDisplayWood(newValue); // 클라 표시 갱신
        }
        public void RequestPickup(int amount)
        {
            if (!IsOwner) return;
            RequestPickupServerRpc(amount);

        }

        // 서버 위치 수신 (클라이언트용)
        private void OnSyncPosition(Vector3 oldValue, Vector3 newValue, bool asServer)
        {
            //Debug.Log($"SYNC RECEIVED [{Owner.ClientId}] server={asServer} pos={newValue}");
            if (asServer || _movement == null) return;
            _movement.SyncPosition(newValue);
        }

        // 서버 전용: MatchSession에서 매 틱 호출
        public void SetServerPosition(Vector3 position)
        {
            //Debug.Log($"SERVER SEND [{Owner.ClientId}] : {position}");
            _syncedPosition.Value = position;
        }

        // 서버가 계산한 HP를 SyncVar로 → 모든 클라에 자동 동기화
        public void SetServerHp(float maxHp, float currentHp)
        {
            _maxHp.Value = maxHp;
            _currentHp.Value = currentHp;
        }

        [ServerRpc]
        private void LoginServerRpc(string uid)
        {
            Debug.Log($"REGISTER ClientId={Owner.ClientId} UID={uid}");
            _userId.Value = uid;

            GameServerManager.Instance.RegisterPlayer(Owner.ClientId, uid);

            GameServerManager.Instance.PlayerNetworkManager.Register(Owner.ClientId, uid, this);

            Debug.Log($"플레이어 등록 완료 : {uid}");
        }

        [ServerRpc]
        private void SendInputServerRpc(Vector2 input)
        {
            GameServerManager.Instance.OnReceiveInput(Owner.ClientId, input);
        }

        // EscapeZone에서 호출
        public void RequestExtract()
        {
            if (!IsOwner) return;
            RequestExtractServerRpc();
        }

        [ServerRpc]
        private void RequestExtractServerRpc()
        {
            GameServerManager.Instance.OnRequestExtract(Owner.ClientId);
        }

        // 서버에서 호출 / 판정 결과 회신(Owner)
        public void NotifyExtractResult(bool success)
        {
            TargetExtractResult(Owner, success);
        }

        [TargetRpc]
        private void TargetExtractResult(NetworkConnection connection, bool success)
        {
            if (!success)
            {
                Debug.Log("미개방/사망/중복");
                return;
            }
            GameEvents.RaisePlayerExtracted(gameObject);
        }

        public void DespawnSelf()
        {
            Debug.Log($"[EXT] DespawnSelf 진입 / IsServerInitialized={IsServerInitialized}");
            if (!IsServerInitialized) return;
            ServerManager.Despawn(NetworkObject);
            Debug.Log("[EXT] Despawn 호출 완료");
        }
        public void DespawnForExtract()
        {
            if (TryGetComponent<PlayerHealth>(out var hp))
            {
                hp.DespawnOnExtract();
            }
        }
        private void RequestPickupServerRpc(int amount)
        {
            GameServerManager.Instance.OnRequestPickup(Owner.ClientId, amount);
        }
        public void SetServerWood(int wood)
        {
            _collectedWood.Value = wood;
        }
        
    }
}