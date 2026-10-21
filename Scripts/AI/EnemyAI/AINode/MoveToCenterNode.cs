using UnityEngine;
using UnityEngine.AI;
using Wooduduk.AI.BT;

namespace Wooduduk.AI.EnemyAI.AINode
{
    public class MoveToCenterNode : Node
    {
        private readonly BaseAIController controller;

        private Vector3 roamDestination;
        private bool hasRoamTarget = false;

        // 배회 설정값
        private readonly float roamRadius = 4f;    // 한 번에 배회할 거리
        private readonly float coneAngle = 45f;   // 중앙 기준 좌우 탐색 각도

        public MoveToCenterNode(BaseAIController controller)
        {
            this.controller = controller;
        }

        public override NodeState Evaluate()
        {
            if (controller.IsStopped)
            {
                hasRoamTarget = false;
            }

            if (!hasRoamTarget || Vector3.Distance(controller.transform.position, roamDestination) <= controller.AttackRange)
            {
                if (TryGetRoamPosition(out roamDestination))
                {
                    hasRoamTarget = true;
                    controller.MoveTo(roamDestination);
                }
                else
                {
                    // 길찾기 실패 시 이번 틱은 실패 처리
                    return NodeState.FAILURE;
                }
            }

            return NodeState.RUNNING;
        }


        private bool TryGetRoamPosition(out Vector3 result)
        {
            // 현재 위치에서 중앙을 향하는 방향 벡터
            Vector3 directionToCenter = controller.DefaultCenterPoint - controller.transform.position;
            directionToCenter.y = 0f;
            directionToCenter.Normalize();

            // 지정된 각도(-45도 ~ +45도) 내에서 Y축 기준 랜덤 회전값 생성
            float randomAngle = Random.Range(-coneAngle, coneAngle);
            Quaternion rotation = Quaternion.Euler(0f, randomAngle, 0f);

            // 중앙을 향하는 벡터를 랜덤 각도만큼 회전시켜 최종 전진 방향 결정
            Vector3 moveDirection = rotation * directionToCenter;

            // 전진 방향으로 roamRadius 만큼 떨어진 목표 좌표 계산
            Vector3 targetPoint = controller.transform.position + (moveDirection * roamRadius);

            // NavMesh.SamplePosition을 통해 해당 좌표가 실제 이동 가능한 NavMesh 위인지 검증
            if (NavMesh.SamplePosition(targetPoint, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }

            result = Vector3.zero;
            return false;
        }
    }
}