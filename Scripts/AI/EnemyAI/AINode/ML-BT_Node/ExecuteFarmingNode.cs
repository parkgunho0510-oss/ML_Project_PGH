using Wooduduk.AI.BT;

namespace Wooduduk.AI.EnemyAI.AINode
{
    public class ExecuteFarmingNode : Node
    {
        private EnemyAIController _enemyController;

        public ExecuteFarmingNode(BaseAIController controller)
        {
            _enemyController = controller as EnemyAIController;
        }

        public override NodeState Evaluate()
        {
            if (_enemyController == null) return NodeState.FAILURE;

            // 파밍 브레인 활성화 (함수 내부에서 전투 브레인은 자동으로 꺼짐)
            _enemyController.SwitchToFarmingBrain();

            return NodeState.RUNNING;
        }
    }
}