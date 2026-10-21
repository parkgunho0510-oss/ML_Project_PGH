using System.Collections.Generic;
using UnityEngine;
using Wooduduk.AI.BT;
using Wooduduk.AI.EnemyAI.AINode;

namespace Wooduduk.AI.EnemyAI.State
{
    public class EnemyMoveState : AIState
    {
        private Node _btRoot;
        private float _tickTimer = 0f;
        private const float TICK_RATE = 0.15f;

        public EnemyMoveState(BaseAIController controller, AIStateMachine stateMachine)
            : base(controller, stateMachine) { }

        public override void Enter()
        {
            if (_btRoot == null)
            {
                _btRoot = new Selector(new List<Node>
                {
                    // 1순위 (전투): 주변 시야 내에 다른 플레이어가 있는가?
                   new Sequence(new List<Node> {
                        new CheckCombatTargetNode(_controller),
                        new ExecuteCombatNode(_controller)
                    }),

                    // 2순위 (파밍): 적은 없지만 처리할 목표(나무 or 출구)가 있는가?
                    new Sequence(new List<Node> {
                        new CheckFarmingTargetNode(_controller),
                        new ExecuteFarmingNode(_controller)
                    }),
                    new Sequence(new List<Node> {
                        new DisableAllBrainsNode(_controller), // ML-Agent 스크립트 비활성화
                        new MoveToCenterNode(_controller)      // NavMeshAgent.SetDestination으로 직접 이동
                    }),
                });
            }
        }

        public override void LogicUpdate()
        {
            _tickTimer += Time.deltaTime;
            if (_tickTimer >= TICK_RATE)
            {
                _btRoot.Evaluate();
                _tickTimer = 0f;
            }
        }

        public override void Exit()
        {
            _controller.StopMoving();
            // 안전장치: 상태를 빠져나갈 때 모든 ML 브레인을 물리적으로 오프
            var enemyController = _controller as EnemyAIController;
            if (enemyController != null) enemyController.DisableAllBrains();
        }
    }
}