using UnityEngine;
using Wooduduk.AI.BT;
using Wooduduk.Player;

namespace Wooduduk.AI.EnemyAI.AINode
{
    public class CheckFarmingTargetNode : Node
    {
        private BaseAIController _controller;
        private WoodBag _woodBag;

        public CheckFarmingTargetNode(BaseAIController controller)
        {
            _controller = controller;
            _woodBag = _controller.GetComponent<WoodBag>();
        }

        public override NodeState Evaluate()
        {
            if (_woodBag == null) return NodeState.FAILURE;

            //  목표 수집량 달성 시 -> 출구 탐색
            if (_woodBag.CurrentWood >= _controller.TargetWoodCount)
            {
                Transform exitTarget = _controller.DetectClosestTarget(_controller.ExitLayer);
                // 출구를 찾았든 못 찾았든 CurrentTarget을 갱신 (못 찾았으면 null이 들어감)
                _controller.CurrentTarget = exitTarget;
            }
            // 2목표 수집량 미달 시 -> 나무 탐색
            else
            {
                Transform treeTarget = _controller.DetectClosestTarget(_controller.TreeLayer);
                // 나무를 찾았든 못 찾았든 CurrentTarget을 갱신 (못 찾았으면 null이 들어감)
                _controller.CurrentTarget = treeTarget;
            }

            // 핵심: 나무나 출구가 시야에 없더라도 무조건 SUCCESS를 반환하여 
            // 다음 노드인 ExecuteFarmingNode가 실행되도록 길을 열어줍니다.
            return NodeState.SUCCESS;
        }
    }
}