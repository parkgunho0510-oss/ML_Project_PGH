using UnityEngine;
using UnityEngine.AI;
using Wooduduk.AI.EnemyAI.State;
using Wooduduk.Core;

namespace Wooduduk.AI.EnemyAI
{
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class BaseAIController : MonoBehaviour
    {
        [Header("Ranges & Positions")]
        [SerializeField] protected float _detectRange = 10f;
        [SerializeField] protected float _attackRange = 2f;
        [SerializeField] protected Vector3 _centerPoint = Vector3.zero;

        [Header("Target Layers")]
        [SerializeField] protected LayerMask _playerLayer;
        [SerializeField] protected LayerMask _treeLayer;

        [Header("Farming Settings")]
        [SerializeField] protected LayerMask _exitLayer;
        [SerializeField] protected int _targetWoodCount = 50;

        public LayerMask ExitLayer => _exitLayer;
        public int TargetWoodCount => _targetWoodCount;

        private Animator _animator;
        protected NavMeshAgent _agent;
        private IPlayerStats _stats;

        public LayerMask PlayerLayer => _playerLayer;
        public LayerMask TreeLayer => _treeLayer;
        public IPlayerStats Stats => _stats;
        public bool IsStopped => _agent != null && _agent.isStopped;

        public Transform CurrentTarget { get; set; }
        public Vector3 DefaultCenterPoint => _centerPoint;
        public float AttackRange => _attackRange;
        public float DetectRange => _detectRange;

        // 캐싱
        private static readonly int _moveXHash = Animator.StringToHash("MoveX");
        private static readonly int _moveYHash = Animator.StringToHash("MoveY");
        private static readonly int _velocityHash = Animator.StringToHash("Velocity");

        private bool _hasMoveX;
        private bool _hasMoveY;
        private bool _hasVelocity;

        public AIStateMachine StateMachine { get; protected set; }

        private readonly Collider[] _hitColliders = new Collider[10];

        protected virtual void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _stats = GetComponent<IPlayerStats>();
            _animator = GetComponent<Animator>();
            StateMachine = new AIStateMachine();
            CacheAnimatorFlags();
        }

        protected virtual void Update()
        {
            if (!_agent.isActiveAndEnabled || !_agent.isOnNavMesh) return;

            if (_stats != null)
            {
                _agent.speed = _stats.MoveSpeed;
            }

            if (StateMachine != null && StateMachine.CurrentState != null)
            {
                StateMachine.CurrentState.LogicUpdate();
            }

            if (_animator != null)
            {
                if (_animator != null)
                {
                    Vector3 localVel = transform.InverseTransformDirection(_agent.velocity);
                    float maxSpeed = _agent.speed > 0f ? _agent.speed : 1f;

                    if (_hasMoveX) _animator.SetFloat(_moveXHash, localVel.x / maxSpeed, 0.1f, Time.deltaTime);
                    if (_hasMoveY) _animator.SetFloat(_moveYHash, localVel.z / maxSpeed, 0.1f, Time.deltaTime);
                    if (_hasVelocity) _animator.SetFloat(_velocityHash, _agent.velocity.magnitude / maxSpeed, 0.1f, Time.deltaTime);
                }
            }
        }

        protected virtual void OnEnable()
        {
            // 타겟 초기화
            CurrentTarget = null;

            // NavMeshAgent 초기화
            if (_agent != null)
            {
                // 데이터 초기화
                _agent.enabled = false;
                _agent.enabled = true;

                // 현재 위치로 강제 스냅
                _agent.Warp(transform.position);

                _agent.stoppingDistance = _attackRange * 0.8f;

                // 정지 상태로 리셋
                if (_agent.isOnNavMesh)
                {
                    _agent.ResetPath();
                    _agent.isStopped = true;
                    _agent.velocity = Vector3.zero;
                }
            }
        }

        private void CacheAnimatorFlags()
        {
            if (_animator == null) return;
            foreach (var p in _animator.parameters)
            {
                if (p.nameHash == _moveXHash) _hasMoveX = true;
                else if (p.nameHash == _moveYHash) _hasMoveY = true;
                else if (p.nameHash == _velocityHash) _hasVelocity = true;
            }
        }

        public Transform DetectClosestTarget(LayerMask targetLayer)
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position, _detectRange, _hitColliders, targetLayer);
            if (count == 0) return null;

            Transform closest = null;
            float minSqrDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                if (_hitColliders[i].gameObject == gameObject) continue;

                float sqrDist = (transform.position - _hitColliders[i].transform.position).sqrMagnitude;
                if (sqrDist < minSqrDistance)
                {
                    minSqrDistance = sqrDist;
                    closest = _hitColliders[i].transform;
                }
            }
            return closest;
        }

        public void MoveTo(Vector3 destination)
        {
            if (!_agent.isActiveAndEnabled || !_agent.isOnNavMesh) return;

            _agent.isStopped = false;

            if ((_agent.destination - destination).sqrMagnitude > 1.0f)
            {
                _agent.SetDestination(destination);
            }
        }

        public void StopMoving()
        {
            if (!_agent.isActiveAndEnabled || !_agent.isOnNavMesh) return;

            if (!_agent.isStopped)
            {
                _agent.isStopped = true;
                _agent.velocity = Vector3.zero;
            }
        }

        public bool IsTargetValid()
        {
            if (CurrentTarget == null) return false;

            if (!CurrentTarget.gameObject.activeInHierarchy)
            {
                CurrentTarget = null;
                return false;
            }

            if ((transform.position - CurrentTarget.position).sqrMagnitude > _detectRange * _detectRange)
            {
                CurrentTarget = null;
                return false;
            }

            return true;
        }
    }
}