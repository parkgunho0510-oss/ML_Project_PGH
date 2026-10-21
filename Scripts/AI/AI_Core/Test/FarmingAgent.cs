using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.AI;
using Wooduduk.AI_Core;
using Wooduduk.AI_Core.Training;
using Wooduduk.Combat;
using Wooduduk.Core;
using Wooduduk.Player;

namespace Wooduduk.AI.EnemyAI.Training
{
    public class FarmingAgent : Agent
    {
        [Header("Settings")]
        [SerializeField] private float _turnSpeed = 720f; // 회전 속도를 높여 즉각적으로 방향 전환
        private IPlayerStats _stats;

        private MLAgentInputProvider _inputProvider;
        private PlayerHealth _playerHealth;
        private WoodBag _woodBag;
        private AttackLogic _attackLogic;
        private NavMeshAgent _navMeshAgent;
        private BaseAIController _baseController;

        private int _lastWoodCount = 0;
        private float _timeSinceLastWood = 0f;
        private bool _isCached = false;

        private Vector2 _previousMoveInput = Vector2.zero;
        private Transform _previousTarget = null;

        private Vector3 _previousPosition;
        private float _minDistanceToTarget = float.MaxValue;

        public override void Initialize()
        {
            EnsureCached();
        }

        private void EnsureCached()
        {
            if (_isCached) return;

            _inputProvider = GetComponentInParent<MLAgentInputProvider>();
            _playerHealth = GetComponentInParent<PlayerHealth>();
            _woodBag = GetComponentInParent<WoodBag>();
            _attackLogic = GetComponentInParent<AttackLogic>();
            _navMeshAgent = GetComponentInParent<NavMeshAgent>();
            _stats = GetComponentInParent<IPlayerStats>();
            _baseController = GetComponentInParent<BaseAIController>();

            // ML-Agent가 직접 물리/이동을 제어하므로 NavMeshAgent의 자동 회전을 끕니다.
            if (_navMeshAgent != null)
            {
                _navMeshAgent.updateRotation = false;
            }

            _isCached = true;
        }

        public override void OnEpisodeBegin()
        {
            _lastWoodCount = 0;
            _timeSinceLastWood = 0f;

            if (_playerHealth != null) _playerHealth.ResetHealthToMax();
            if (_woodBag != null) _woodBag.SetDisplayWood(0);

            var arena = GetComponentInParent<MLTrainingArena>();
            if (arena != null) arena.SpawnArena();

            // 에피소드 시작 시 속도 초기화
            if (_navMeshAgent != null && _navMeshAgent.isOnNavMesh)
            {
                _navMeshAgent.velocity = Vector3.zero;
            }
            _inputProvider.SetAction(Vector2.zero, false);

            _previousTarget = null;
            _previousPosition = transform.position;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            EnsureCached();
            if (_woodBag != null) _woodBag.OnWoodChanged += HandleWoodCollected;

            // BT에서 Agent를 켰을 때 잔여 입력값으로 인한 미끄러짐 방지
            _inputProvider.SetAction(Vector2.zero, false);

            _previousTarget = null;
            _previousPosition = transform.position;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (_woodBag != null) _woodBag.OnWoodChanged -= HandleWoodCollected;

            // BT에서 Agent를 끌 때 완벽하게 정지시킵니다.
            if (_navMeshAgent != null && _navMeshAgent.isOnNavMesh)
            {
                _navMeshAgent.velocity = Vector3.zero;
            }
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            // 1. 상태 관측
            sensor.AddObservation(_playerHealth != null ? (float)_playerHealth.CurrentHealth / _playerHealth.MaxHealth : 1f);
            sensor.AddObservation(_woodBag != null ? Mathf.Clamp01((float)_woodBag.CurrentWood / _baseController.TargetWoodCount) : 0f);
            sensor.AddObservation((_attackLogic != null && _attackLogic.IsAttackReady) ? 1f : 0f);

            // 2. 현재 이동 벡터 (정규화)
            if (_navMeshAgent != null && _navMeshAgent.isOnNavMesh)
                sensor.AddObservation(_navMeshAgent.velocity.normalized);
            else
                sensor.AddObservation(Vector3.zero);


            // 현재 목표(나무/출구)에 대한 관측치 제공
            Transform currentTarget = _baseController != null ? _baseController.CurrentTarget : null;
            if (currentTarget != null)
            {
                // 내 위치에서 타겟으로 향하는 방향 벡터 (로컬 또는 월드 방향)
                Vector3 toTarget = (currentTarget.position - transform.position);
                float distance = toTarget.magnitude;

                sensor.AddObservation(toTarget.normalized); // 방향 (Vector3 = 관측치 3개)
                sensor.AddObservation(Mathf.Clamp01(distance / 20f)); // 거리 
            }
            else
            {
                sensor.AddObservation(Vector3.zero); // 타겟 없을 때 예외 처리
                sensor.AddObservation(0f); //
            }

            // RayPerceptionSensor3D 컴포넌트는 자동으로 관측값을 추가하므로 여기에 코드로 작성할 필요가 없습니다.
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            // ContinuousActions[0]: World X축 이동 (-1 ~ 1)
            // ContinuousActions[1]: World Z축 이동 (-1 ~ 1)
            float moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
            float moveZ = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
            bool isAttacking = actions.DiscreteActions[0] == 1;

            Vector2 currentMoveInput = new Vector2(moveX, moveZ);
            _inputProvider.SetAction(new Vector2(moveX, moveZ), isAttacking);

            // 공격 로직 및 패널티 (아무데나 공격하는 꼼수 방지)
            if (isAttacking && _attackLogic != null && _attackLogic.IsAttackReady)
            {
                _attackLogic.Swing();
                AddReward(-0.01f);
            }

            // 시간 지연 패널티 (빠른 클리어 유도)
            if (MaxStep > 0)
            {
                //  매 스텝마다 미세 패널티 (빠른 클리어를 )
                AddReward(-1f / MaxStep);

                // 시간 내에 탈출하지 못하고 MaxStep 도달 감점 부여
                if (StepCount >= MaxStep - 1)
                {
                    AddReward(-10.0f); 
                }
            }
            // 장기간 나무 수집 실패 시 패널티 (가만히 있는 꼼수 방지)
            _timeSinceLastWood += Time.fixedDeltaTime;
            if (_timeSinceLastWood > 15f)
            {
                AddReward(-0.1f);
                _timeSinceLastWood = 0f;
            }

            Transform currentTarget = _baseController != null ? _baseController.CurrentTarget : null;

            if (currentTarget != null)
            {
                float currentDistance = Vector3.Distance(transform.position, currentTarget.position);
                // 타겟이 중간에 바뀌었다면 -> 거리 연산을 초기화하여 점수 복사 방지
                if (currentTarget != _previousTarget)
                {
                    _previousTarget = currentTarget;
                    _minDistanceToTarget = currentDistance;
                }

                // 에이전트가 이전보다 타겟에 더 가까워졌을 때만 보상 지급
                if (currentDistance < _minDistanceToTarget)
                {
                    float distanceClosed = _minDistanceToTarget - currentDistance;

                    // 좁혀진 거리만큼 보상 지급 (가중치 0.05f는 테스트하며 조절)
                    AddReward(distanceClosed * 0.05f);

                    // 최소 거리 갱신 (핵심 포인트)
                    _minDistanceToTarget = currentDistance;
                }
                float inputChange = Vector2.Distance(currentMoveInput, _previousMoveInput);
                if (inputChange > 0.5f)
                {
                    AddReward(-0.005f);
                }
                _previousPosition = transform.position;
            }
            else
            {
                // 타겟을 완전히 잃어버렸을 경우
                _previousTarget = null;
            }
            _previousMoveInput = currentMoveInput;
            // 사망 처리
            if (_playerHealth != null && !_playerHealth.IsAlive)
            {
                AddReward(-1.0f);
                EndEpisode();
            }
        }

        private void FixedUpdate()
        {
            if (_navMeshAgent == null || !_navMeshAgent.isOnNavMesh) return;

            // X, Z 축 기반의 절대 방향 벡터 생성
            Vector3 moveDir = new Vector3(_inputProvider.MoveAxis.x, 0f, _inputProvider.MoveAxis.y);

            if (moveDir.sqrMagnitude > 0.01f)
            {
                // 벡터 정규화를 통해 대각선 이동 속도 뻥튀기 방지
                moveDir.Normalize();

                // 이동 방향으로 캐릭터 즉각 회전
                Quaternion targetRotation = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _turnSpeed * Time.fixedDeltaTime);
                Vector3 targetVelocity = moveDir * _stats.MoveSpeed;
                // NavMeshAgent를 통한 이동 (State에서 가져온 속도 적용)
                _navMeshAgent.Move(moveDir * _stats.MoveSpeed * Time.fixedDeltaTime);

                _navMeshAgent.velocity = targetVelocity;
            }
            else
            {
                // 입력이 없으면 정지
                _navMeshAgent.velocity = Vector3.zero;
            }
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var continuousActionsOut = actionsOut.ContinuousActions;
            // WASD를 절대 방향(X, Z)으로 매핑
            continuousActionsOut[0] = Input.GetAxisRaw("Horizontal");
            continuousActionsOut[1] = Input.GetAxisRaw("Vertical");

            var discreteActionsOut = actionsOut.DiscreteActions;
            discreteActionsOut[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
        }


        private void HandleWoodCollected(int currentWood)
        {
            int gained = currentWood - _lastWoodCount;
            if (gained > 0)
            {
                _timeSinceLastWood = 0f;

                // 이미 할당량을 채운 상태라면 추가 장작을 먹어도 보상 0 (무한 파밍 차단)
                if (_lastWoodCount >= _baseController.TargetWoodCount)
                {
                    _lastWoodCount = currentWood;
                    return;
                }

                // 기본 수집 보상 지급
                AddReward(0.5f * gained);

                // 보너스 1회 지급
                if (currentWood >= _baseController.TargetWoodCount)
                {
                    AddReward(2.0f);
                }

                _lastWoodCount = currentWood;
            }
        }

        public void OnHitSuccessfully()
        {
            if (_woodBag.CurrentWood >= _baseController.TargetWoodCount) return;

            AddReward(0.2f);
        }

        // 트리거 등에서 호출
        public void OnExitReached()
        {
            if (_lastWoodCount < _baseController.TargetWoodCount)
            {
                AddReward(-0.05f);

                return;
            }
            // 할당량까지 채우고 탈출한 거라면 보너스
            if (_lastWoodCount >= _baseController.TargetWoodCount)
            {
                AddReward(5.0f);
            }
            EndEpisode();
        }
    }
}