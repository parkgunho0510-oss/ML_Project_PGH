using Wooduduk.AI.BT;

namespace Wooduduk.AI.EnemyAI.AINode
{
    public class ExecuteCombatNode : Node
    {
        private EnemyAIController _enemyController;

        public ExecuteCombatNode(BaseAIController controller)
        {
            _enemyController = controller as EnemyAIController;
        }

        public override NodeState Evaluate()
        {
            if (_enemyController == null) return NodeState.FAILURE;

            // 전투 브레인 활성화 (함수 내부에서 파밍 브레인은 자동으로 꺼짐)
            _enemyController.SwitchToCombatBrain();

            // 이미 활성화되어 조종 중이므로 RUNNING 유지하여 하위 ML에 제어권 전임
            return NodeState.RUNNING;
        }
    }
}