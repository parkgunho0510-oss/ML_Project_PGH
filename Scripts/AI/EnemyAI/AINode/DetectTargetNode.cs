using UnityEngine;
using Wooduduk.AI.BT;

namespace Wooduduk.AI.EnemyAI.AINode
{
    public class DetectTargetNode : Node
    {
        private readonly BaseAIController controller;
        private readonly LayerMask targetLayer;

        public DetectTargetNode(BaseAIController controller, LayerMask targetLayer)
        {
            this.controller = controller;
            this.targetLayer = targetLayer;
        }

        public override NodeState Evaluate()
        {
            Transform target = controller.DetectClosestTarget(targetLayer);
            if (target != null)
            {
                controller.CurrentTarget = target;
                return NodeState.SUCCESS;
            }
            return NodeState.FAILURE;
        }
    }
}