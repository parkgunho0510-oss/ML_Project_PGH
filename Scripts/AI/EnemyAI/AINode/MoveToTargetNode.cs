using UnityEngine;
using Wooduduk.AI.BT;

namespace Wooduduk.AI.EnemyAI.AINode
{
    public class MoveToTargetNode : Node
    {
        private readonly BaseAIController controller;

        public MoveToTargetNode(BaseAIController controller)
        {
            this.controller = controller;
        }

        public override NodeState Evaluate()
        {
            if (controller.CurrentTarget == null) return NodeState.FAILURE;

            controller.MoveTo(controller.CurrentTarget.position);

            float distance = Vector3.Distance(controller.transform.position, controller.CurrentTarget.position);

            if (distance <= controller.AttackRange)
            {
                return NodeState.SUCCESS; // 공격 범위 도달
            }

            return NodeState.RUNNING;// 이동 중
        }
    }
}
