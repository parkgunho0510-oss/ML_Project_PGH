using UnityEngine;
using Wooduduk.AI.BT;
using Wooduduk.Core;

namespace Wooduduk.AI.EnemyAI.AINode
{
    public class DetectClosestExitNode : Node
    {
        private readonly BaseAIController _controller;

        public DetectClosestExitNode(BaseAIController controller)
        {
            _controller = controller;
        }

        public override NodeState Evaluate()
        {
            // 맵에 출구가 하나도 없다면 실패
            if (ExitPoint.AllExits.Count == 0) return NodeState.FAILURE;

            Transform closestExit = null;
            float minSqrDistance = float.MaxValue;
            Vector3 myPos = _controller.transform.position;

            // 등록된 모든 출구 중 가장 가까운 곳 탐색
            for (int i = 0; i < ExitPoint.AllExits.Count; i++)
            {
                float sqrDist = (myPos - ExitPoint.AllExits[i].transform.position).sqrMagnitude;
                if (sqrDist < minSqrDistance)
                {
                    minSqrDistance = sqrDist;
                    closestExit = ExitPoint.AllExits[i].transform;
                }
            }

            if (closestExit != null)
            {
                _controller.CurrentTarget = closestExit;
                return NodeState.SUCCESS;
            }

            return NodeState.FAILURE;
        }
    }
}