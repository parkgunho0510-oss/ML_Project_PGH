using UnityEngine;
using Wooduduk.AI.EnemyAI.State;

namespace Wooduduk.AI.EnemyAI
{
    public class EnemyAIController : BaseAIController
    {
        public enum TrainingBrainType { Farming, Combat }

        [Header("Two-Brain References")]
        [SerializeField] private GameObject _farmingBrainObject;
        [SerializeField] private GameObject _combatBrainObject;

        [Header("Training Settings")]
        [SerializeField] private bool _isTrainingMode = false; // 인스펙터에서 학습 시 true로 체크
        [SerializeField] private TrainingBrainType _trainingBrainTarget = TrainingBrainType.Farming; 
        public EnemyMoveState MoveState { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            // 플레이어 카피용 AI의 이동/공격 상태 등록
            MoveState = new EnemyMoveState(this, StateMachine);


            if (_isTrainingMode)
            {
                SetupTrainingBrain();
            }
            // 초기 상태에서는 두 브레인을 모두 꺼둡니다.
            else
            {
                if (_farmingBrainObject != null) _farmingBrainObject.SetActive(false);
                if (_combatBrainObject != null) _combatBrainObject.SetActive(false);
            }
            if (_farmingBrainObject != null) _farmingBrainObject.SetActive(false);
            if (_combatBrainObject != null) _combatBrainObject.SetActive(false);
        }

        protected override void OnEnable()
        {
            base.OnEnable(); // Agent 초기화 실행

            if (MoveState != null)
            {
                StateMachine.Initialize(MoveState);

                if (!_isTrainingMode)
                {
                    SwitchToFarmingBrain();
                }
                else
                {
                    SetupTrainingBrain();
                }
            }


        }

        protected override void Update()
        {
            base.Update();
        }

        // --- Two-Brain 스위칭 로직 (BT 노드에서 호출됨) ---

        private void SetupTrainingBrain()
        {
            if (_farmingBrainObject != null)
                _farmingBrainObject.SetActive(_trainingBrainTarget == TrainingBrainType.Farming);

            if (_combatBrainObject != null)
                _combatBrainObject.SetActive(_trainingBrainTarget == TrainingBrainType.Combat);

            if (_agent != null && _agent.isOnNavMesh)
            {
                _agent.ResetPath();
                _agent.isStopped = false;
                _agent.updateRotation = false;
            }
        }

        public void SwitchToFarmingBrain()
        {
            if (_isTrainingMode) return;

            if (_combatBrainObject != null && _combatBrainObject.activeSelf)
                _combatBrainObject.SetActive(false);

            if (_farmingBrainObject != null && !_farmingBrainObject.activeSelf)
            {
                if (_agent != null && _agent.isOnNavMesh)
                {
                    _agent.ResetPath();
                    _agent.isStopped = false;       // ML Agent가 이동할 수 있도록 정지 해제
                    _agent.updateRotation = false;  // 회전은 FarmingAgent에서 직접 제어
                }

                _farmingBrainObject.SetActive(true);
            }
        }

        public void SwitchToCombatBrain()
        {
            if (_isTrainingMode) return;

            if (_farmingBrainObject != null && _farmingBrainObject.activeSelf)
                _farmingBrainObject.SetActive(false);

            if (_combatBrainObject != null && !_combatBrainObject.activeSelf)
            {
                if (_agent != null && _agent.isOnNavMesh)
                {
                    _agent.ResetPath();
                    _agent.isStopped = false;       // ML Agent가 이동할 수 있도록 정지 해제
                    _agent.updateRotation = false;  // 회전은 FarmingAgent에서 직접 제어
                }
                _combatBrainObject.SetActive(true);
            }
        }

        public void DisableAllBrains()
        {
            if (_isTrainingMode) return;

            if (_farmingBrainObject != null && _farmingBrainObject.activeSelf)
                _farmingBrainObject.SetActive(false);

            if (_combatBrainObject != null && _combatBrainObject.activeSelf)
                _combatBrainObject.SetActive(false);
        }
    }
}