using UnityEngine;
using Wooduduk.AI.BT;

namespace Wooduduk.AI.EnemyAI.AINode
{
    public class CheckCombatTargetNode : Node
    {
        private BaseAIController _controller;

        public CheckCombatTargetNode(BaseAIController controller)
        {
            _controller = controller;
        }

        public override NodeState Evaluate()
        {
            // 컨트롤러의 PlayerLayer를 사용하여 가장 가까운 적을 탐색
            Transform target = _controller.DetectClosestTarget(_controller.PlayerLayer);
            
            if (target != null)
            {
                _controller.CurrentTarget = target;
                return NodeState.SUCCESS; // 적이 있으므로 전투 시퀀스 진행
            }

            return NodeState.FAILURE;
        }
    }
}