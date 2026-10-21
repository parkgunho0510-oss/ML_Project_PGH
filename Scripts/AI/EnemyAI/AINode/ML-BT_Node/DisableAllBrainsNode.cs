using Unity.MLAgents;
using Wooduduk.AI.BT;

namespace Wooduduk.AI.EnemyAI.AINode
{
    public class DisableAllBrainsNode : Node
    {
        private EnemyAIController _controller;

        public DisableAllBrainsNode(BaseAIController controller)
        {
            _controller = controller as EnemyAIController;
        }

        public override NodeState Evaluate()
        {
            if (Academy.Instance.IsCommunicatorOn)
            {
                // 학습 중에는 컴포넌트를 비활성화하면 통신이 파괴되므로,
                // 뇌를 끄지 않고 실패(Failure)를 반환하여 상태 전환을 막습니다.
                return NodeState.FAILURE;
            }
            
            if (_controller != null)
            {
                // 두 브레인의 GameObject를 모두 비활성화하고 물리 속도를 0으로 초기화
                _controller.DisableAllBrains();
            }

            return NodeState.SUCCESS;
        }
    }
}