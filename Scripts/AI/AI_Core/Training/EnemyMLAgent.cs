using UnityEngine;
using UnityEngine.AI;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Wooduduk.AI_Core;
using Wooduduk.AI_Core.Training;
using Wooduduk.Combat;
using Wooduduk.Player;

namespace Wooduduk.AI.EnemyAI.Training
{
    [RequireComponent(typeof(MLAgentInputProvider))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyMLAgent : Agent
    {
        [Header("Training References")]
        [SerializeField] private MLTrainingArena _arena;
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _turnSpeed = 200f;

        [Header("Stage Settings")]
        [SerializeField] private int _targetWoodCount = 20;
        [Tooltip("현재 맵의 대략적인 최대 길이 (훈련장 50, 인게임 500 등)")]
        [SerializeField] private float _maxMapDistance = 500f;

        private MLAgentInputProvider _inputProvider;
        private NavMeshAgent _navMeshAgent;
        private Animator _animator;
        private AttackLogic _attackLogic;

        private WoodBag _woodBag;
        private WoodStack _woodStack;
        private Vector3 _lastPosition;
        private float _previousDistanceToExit = float.MaxValue;

        private readonly int _moveXHash = Animator.StringToHash("MoveX");
        private readonly int _moveYHash = Animator.StringToHash("MoveY");
        private readonly int _velocityHash = Animator.StringToHash("Velocity");

        public override void Initialize()
        {
            _inputProvider = GetComponent<MLAgentInputProvider>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();
            _attackLogic = GetComponent<AttackLogic>();
            _woodBag = GetComponent<WoodBag>();
            _woodStack = GetComponent<WoodStack>();
            if (_arena == null)
            {
                _arena = GetComponentInParent<MLTrainingArena>();
            }
            _navMeshAgent.updateRotation = false;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (_woodBag != null) _woodBag.OnWoodChanged -= HandleWoodChanged;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (_woodBag != null) _woodBag.OnWoodChanged += HandleWoodChanged;
        }

        public override void OnEpisodeBegin()
        {
            if (_woodBag != null) _woodBag.SetDisplayWood(0);
            if (_woodStack != null) _woodStack.ClearStack();
            if (_arena != null) _arena.SpawnArena();

            if (_navMeshAgent.isOnNavMesh)
            {
                _navMeshAgent.velocity = Vector3.zero;
                _navMeshAgent.ResetPath();
            }

            _lastPosition = transform.position;
            _previousDistanceToExit = float.MaxValue;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            float woodRatio = Mathf.Clamp01((float)_woodBag.CurrentWood / _targetWoodCount);
            sensor.AddObservation(woodRatio);
            sensor.AddObservation(_woodBag.CurrentWood >= _targetWoodCount ? 1.0f : 0.0f);

            GameObject exitObj = _arena.GetClosestExitPoint(transform.position);
            if (exitObj != null)
            {
                Vector3 dirToExit = (exitObj.transform.position - transform.position).normalized;
                // 정규화 관측값은 선형 비율이 필요하므로 Distance 유지
                float distToExit = Vector3.Distance(transform.position, exitObj.transform.position);

                sensor.AddObservation(dirToExit.x);
                sensor.AddObservation(dirToExit.z);
                sensor.AddObservation(Mathf.Clamp01(distToExit / _maxMapDistance));
            }
            else
            {
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
            }
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            float moveX = actions.ContinuousActions[0];
            float moveZ = actions.ContinuousActions[1];
            bool isAttacking = actions.DiscreteActions[0] == 1;

            _inputProvider.SetAction(new Vector2(moveX, moveZ), isAttacking);

            if (isAttacking && _attackLogic != null)
            {
                _attackLogic.Swing();
            }

            AddReward(-1f / MaxStep);

            ApplyPostureAndActionPenalty(isAttacking);

            if (_woodBag.CurrentWood >= _targetWoodCount)
            {
                GameObject exitObj = _arena.GetClosestExitPoint(transform.position);
                if (exitObj != null)
                {
                    float currentDistance = Vector3.Distance(transform.position, exitObj.transform.position);

                    if (_previousDistanceToExit != float.MaxValue)
                    {
                        float distanceDelta = _previousDistanceToExit - currentDistance;
                        AddReward(distanceDelta * 0.2f);
                    }
                    _previousDistanceToExit = currentDistance;
                }
                AddReward(-0.002f);
            }
        }

        private void ApplyPostureAndActionPenalty(bool isAttacking)
        {
            GameObject targetTree = _arena.GetClosestTree(transform.position);

            if (targetTree != null && targetTree.activeSelf)
            {
                Vector3 dirToTree = (targetTree.transform.position - transform.position).normalized;
                float lookDot = Vector3.Dot(transform.forward, dirToTree);

                // [연산 최적화] 매 프레임 실행되는 로직에서 무거운 Sqrt 연산 제거 및 제곱값 비교
                float sqrDistance = (transform.position - targetTree.transform.position).sqrMagnitude;

                if (isAttacking)
                {
                    // 2.5f의 제곱인 6.25f와 비교
                    if (sqrDistance > 6.25f || lookDot < 0.8f)
                    {
                        AddReward(-0.05f);
                    }
                    else
                    {
                        AddReward(-0.0001f);
                    }
                }
                else
                {
                    // 10f의 제곱인 100f와 비교
                    if (sqrDistance < 100f && lookDot > 0.95f)
                    {
                        AddReward(0.001f);
                    }
                }
            }
            else
            {
                if (isAttacking) AddReward(-0.05f);
            }

            if (Mathf.Abs(_inputProvider.MoveAxis.x) > 0.5f || _inputProvider.MoveAxis.y < 0f)
            {
                AddReward(-0.0001f);
            }
        }

        private void FixedUpdate()
        {
            if (!_navMeshAgent.isOnNavMesh) return;

            float turnInput = _inputProvider.MoveAxis.x;
            float moveInput = _inputProvider.MoveAxis.y;

            transform.Rotate(0f, turnInput * _turnSpeed * Time.fixedDeltaTime, 0f);

            Vector3 moveDir = transform.forward * moveInput;
            _navMeshAgent.Move(moveDir * _moveSpeed * Time.fixedDeltaTime);

            if (Mathf.Abs(moveInput) > 0.1f)
            {
                // [연산 최적화] 제자리걸음 판별 시 Sqrt 연산 제거
                float sqrMovedDist = (transform.position - _lastPosition).sqrMagnitude;
                float distanceThreshold = _moveSpeed * 0.1f * Time.fixedDeltaTime;

                if (sqrMovedDist < (distanceThreshold * distanceThreshold))
                {
                    AddReward(-0.0001f);
                }
            }
            _lastPosition = transform.position;

            UpdateAnimator(moveDir);
        }

        private void UpdateAnimator(Vector3 moveDir)
        {
            if (_animator == null) return;

            Vector3 targetVelocity = moveDir * _moveSpeed;
            Vector3 localVel = transform.InverseTransformDirection(targetVelocity);

            float maxSpeed = _moveSpeed > 0f ? _moveSpeed : 1f;

            _animator.SetFloat(_moveXHash, localVel.x / maxSpeed, 0.1f, Time.deltaTime);
            _animator.SetFloat(_moveYHash, localVel.z / maxSpeed, 0.1f, Time.deltaTime);
            _animator.SetFloat(_velocityHash, targetVelocity.magnitude / maxSpeed, 0.1f, Time.deltaTime);
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var continuousActionsOut = actionsOut.ContinuousActions;
            continuousActionsOut[0] = Input.GetAxisRaw("Horizontal");
            continuousActionsOut[1] = Input.GetAxisRaw("Vertical");

            var discreteActionsOut = actionsOut.DiscreteActions;
            discreteActionsOut[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
        }

        #region Reward & Events
        public void OnTreeChoppedSuccessfully()
        {
            if (_woodBag.CurrentWood >= _targetWoodCount) return;

            AddReward(2.0f);
            _arena.RespawnSingleTree();
        }

        public void OnTreeHitSuccessfully()
        {
            if (_woodBag.CurrentWood >= _targetWoodCount) return;

            AddReward(0.2f);
        }

        private void HandleWoodChanged(int currentWood)
        {
            if (currentWood == 0) return;

            if (currentWood <= _targetWoodCount)
            {
                AddReward(2.0f);

                if (currentWood == _targetWoodCount)
                {
                    AddReward(5.0f);
                }
            }
            else
            {
                AddReward(-0.5f);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("ExitPoint"))
            {
                if (_woodBag.CurrentWood >= _targetWoodCount)
                {
                    AddReward(5.0f);
                    EndEpisode();
                }
                else
                {
                    AddReward(-1.0f);
                    EndEpisode();
                }
            }
        }
        #endregion
    }
}