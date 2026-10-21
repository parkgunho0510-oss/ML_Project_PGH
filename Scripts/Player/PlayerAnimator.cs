using FishNet.Object;
using UnityEngine;
using Wooduduk.AI.EnemyAI;
using Wooduduk.Core;

namespace Wooduduk.Player
{
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private Animator _animator;

        [Header("Animator Parameter Settings")]
        [SerializeField] private string _velocity = "Velocity";
        [SerializeField] private string _isCharging = "IsCharging";
        [SerializeField] private string _chargeLevel = "ChargeLevel";
        [SerializeField] private string _attackTrigger = "Attack";
        [SerializeField] private string _dieTrigger = "Die";
        [Header("Locomotion")]
        [SerializeField] private string _moveX = "MoveX";
        [SerializeField] private string _moveY = "MoveY";
        [SerializeField] private float _moveDamp = 0.1f;

        private Vector3 _animVeclocity;
        private Vector2 _input;

        // 캐싱
        private int _velocityHash, _isChargingHash, _chargeLevelHash, _attackHash, _dieHash
            , _moveXHash, _moveYHash;

        private bool _isPlayer;

        private void Awake()
        {
            if (_animator == null) _animator = GetComponentInChildren<Animator>();

            _velocityHash = Animator.StringToHash(_velocity);
            _isChargingHash = Animator.StringToHash(_isCharging);
            _chargeLevelHash = Animator.StringToHash(_chargeLevel);
            _attackHash = Animator.StringToHash(_attackTrigger);
            _dieHash = Animator.StringToHash(_dieTrigger);
            _moveXHash = Animator.StringToHash(_moveX);
            _moveYHash = Animator.StringToHash(_moveY);

            _isPlayer = !TryGetComponent<BaseAIController>(out _);
        }
        private void OnEnable()
        {
            if (_isPlayer)
            {
                PlayerEvents.OnMove += HandleMove;
                //PlayerEvents.OnChargeStarted += HandleChargeStarted;
                //PlayerEvents.OnChargeProgress += HandleChargeProgress;
                //PlayerEvents.OnChargeReleased += HandleChargeReleased;
            }
            GameEvents.OnPlayerDied += HandlePlayerDied;
        }
        private void OnDisable()
        {
            if (_isPlayer)
            {
                PlayerEvents.OnMove -= HandleMove;
                //PlayerEvents.OnChargeStarted -= HandleChargeStarted;
                //PlayerEvents.OnChargeProgress -= HandleChargeProgress;
                //PlayerEvents.OnChargeReleased -= HandleChargeReleased;
            }
            GameEvents.OnPlayerDied -= HandlePlayerDied;
        }
       
        // Hash랑 필드 헷갈리면 코접각. 사실 피곤한 걸수도?
        private void HandleMove(Vector2 v)
        {
            _input = v;
            //if (_animator == null) return;

            //Vector3 world = new Vector3(v.x, 0f, v.y);
            //Vector3 local = transform.InverseTransformDirection(world);

            //_animator.SetFloat(_moveXHash, local.x, _moveDamp, Time.deltaTime);
            //_animator.SetFloat(_moveYHash, local.z, _moveDamp, Time.deltaTime);
            //_animator.SetFloat(_velocityHash, v.magnitude);

            //Vector3 world = new Vector3(v.x, 0f, v.y);
            //Vector3 local = transform.InverseTransformDirection(world);
            //_animator.SetFloat(_moveXHash, local.x);
            //_animator.SetFloat(_moveYHash, local.z);

        } 
        private void Update()
        {
            if (!_isPlayer) return;
            if (_animator == null) return;

            Vector3 world = new Vector3(_input.x, 0f, _input.y);
            Vector3 local = transform.InverseTransformDirection(world);

            _animator.SetFloat(_moveXHash, local.x, _moveDamp, Time.deltaTime);
            _animator.SetFloat(_moveYHash, local.z, _moveDamp, Time.deltaTime);
            _animator.SetFloat(_velocityHash, _input.magnitude, _moveDamp, Time.deltaTime);
        }
        private void HandleChargeStarted()
        {
            if (_animator == null) return;
            _animator.SetBool(_isChargingHash, true);
        }
        private void HandleChargeProgress(float level)
        {
            if (_animator == null) return;
            _animator.SetFloat(_chargeLevelHash, level);
        }
        private void HandleChargeReleased()
        {
            if (_animator == null) return;
            _animator.SetBool(_isChargingHash, false);
        }
        private void HandlePlayerDied(GameObject who)
        {
            if (_animator == null) return;
            // 본인만 처리
            if (who != gameObject) return;
            _animator.SetTrigger(_dieHash); // 나중에 네트워크 적용 업데이트 필요
        }
    }
}