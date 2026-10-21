using System.Collections.Generic;
using UnityEngine;
using Wooduduk.AI.BT;
using Wooduduk.AI.EnemyAI.AINode;

namespace Wooduduk.AI.EnemyAI.State
{
    public class FleeState : AIState
    {
        private Node _btRoot;
        private float _tickTimer = 0f;
        private const float TICK_RATE = 0.15f; 

        public FleeState(BaseAIController controller, AIStateMachine stateMachine) 
            : base(controller, stateMachine) { }

        public override void Enter()
        {
            if (_btRoot == null)
            {
                _btRoot = new Selector(new List<Node>
                {
                    // 1순위: 플레이어 감지 -> 3초 도망 / 2초 휴식
                    new Sequence(new List<Node> {
                        new DetectTargetNode(_controller, _controller.PlayerLayer),
                        new FleeNode(_controller, 3f, 2f)
                    }),
                    
                    new MoveToCenterNode(_controller)
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
        }
    }
}