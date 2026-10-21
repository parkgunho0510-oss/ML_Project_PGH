using FishNet.Object;
using UnityEngine;
using Wooduduk.Core;

namespace Wooduduk.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerMovement : NetworkBehaviour // 교체
    {
        //[SerializeField] private float _turnSpeed = 720f;

        private Rigidbody _rb;
        private IPlayerStats _stats;
        private Vector2 _input;

        // 타인 동기화용 변수
        private Vector3 _targetPosition;

        // 초기 스폰 텔레포트용 (FixedUpdate에서 물리적으로 적용)
        private bool _pendingTeleport;
        private Vector3 _teleportTarget;
        private bool _initialSynced;
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _stats = GetComponent<IPlayerStats>();
        }
        private void OnEnable()
        {
            PlayerEvents.OnMove += HandleMove;
            
        }
        private void OnDisable()
        {
            PlayerEvents.OnMove -= HandleMove;
        }
        private void FixedUpdate()
        {
            // 스폰 직후 텔레포트 (FixedUpdate에서 해야 물리엔진이 인식)
            if (_pendingTeleport)
            {
                _pendingTeleport = false;
                _rb.linearVelocity = Vector3.zero;
                _rb.position = _teleportTarget;
                transform.position = _teleportTarget;
                Debug.Log($"[PlayerMovement] 텔레포트 실행 → {_teleportTarget}");
                return;
            }

            if (IsOwner)
            {
                // [Owner] 서버 로직과 동일하게 물리 속도 적용
                Vector3 dir = new Vector3(_input.x, 0f, _input.y);
                if (dir.sqrMagnitude > 1f) dir.Normalize();

                _rb.linearVelocity = new Vector3(
                    dir.x * _stats.MoveSpeed,
                    _rb.linearVelocity.y, // 중력 유지
                    dir.z * _stats.MoveSpeed);
            }
            else
            {
                Debug.Log($"[{Owner.ClientId}] POS={_rb.position} TARGET={_targetPosition}");
                // [Others] 서버로부터 받은 위치로 물리적 이동 (MovePosition 사용)
                // 충돌체에 부딪히면 물리 엔진이 알아서 위치를 보정함
                _rb.MovePosition(Vector3.MoveTowards(_rb.position, _targetPosition, _stats.MoveSpeed * Time.fixedDeltaTime));
            }
        }
        private void HandleMove(Vector2 v)
        {
            _input = v;
        }
        // 서버의 MatchSession에서 위치를 갱신할 때 호출
        // 초회 Sync = 스폰 텔레포트 예약 (FixedUpdate에서 물리적으로 적용). 이후는 _targetPosition만 갱신.
        public void SyncPosition(Vector3 position)
        {
            if (!_initialSynced)
            {
                _initialSynced = true;
                _pendingTeleport = true;
                _teleportTarget = position;
                Debug.Log($"[PlayerMovement] 텔레포트 예약 → {position}");
            }
            // Debug.Log($"SYNC POSITION : {position}");
            _targetPosition = position;
        }


    }
}
