using UnityEngine;
using Wooduduk.AI.BT;
using Wooduduk.Player; // WoodBag 참조용

namespace Wooduduk.AI.EnemyAI.AINode
{
    public class CheckWoodCountNode : Node
    {
        private readonly BaseAIController _controller;
        private readonly int _targetCount;
        private WoodBag _woodBag;

        public CheckWoodCountNode(BaseAIController controller, int targetCount)
        {
            _controller = controller;
            _targetCount = targetCount;
            _woodBag = _controller.GetComponent<WoodBag>(); 
        }

        public override NodeState Evaluate()
        {
            // 가방이 없거나, 목표 개수에 도달하지 못했다면 실패
            if (_woodBag == null || _woodBag.CurrentWood < _targetCount)
            {
                return NodeState.FAILURE;
            }

            // 50개 이상 모았으면 성공 반환하여 다음 노드(출구 탐색)
            return NodeState.SUCCESS; 
        }
    }
}