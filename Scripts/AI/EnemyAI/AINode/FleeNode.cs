using UnityEngine;
using UnityEngine.AI;
using Wooduduk.AI.BT;

namespace Wooduduk.AI.EnemyAI.AINode
{
    public class FleeNode : Node
    {
        private readonly BaseAIController _controller;
        private readonly float _runDuration;
        private readonly float _restDuration;
        private readonly float _fleeDistance; // 한 번에 탐색할 도주 반경

        private float _stateStartTime;
        private bool _isResting;

        private readonly float[] _escapeAngles = { 0f, 45f, -45f, 90f, -90f };

        // 매개변수로 달리기 시간(3초)과 휴식 시간(2초)을 받습니다.
        public FleeNode(BaseAIController controller, float runDuration = 3f, float restDuration = 2f, float fleeDistance = 5f)
        {
            _controller = controller;
            _runDuration = runDuration;
            _restDuration = restDuration;
            _fleeDistance = fleeDistance;

            _stateStartTime = Time.time;
            _isResting = false;
        }

        public override NodeState Evaluate()
        {
            // 타겟(플레이어)을 잃어버렸다면 실패 처리하여 배회(MoveToCenter) 노드로 넘깁니다.
            if (_controller.CurrentTarget == null)
            {
                // 다음번 발견 시 처음부터 다시 뛸 수 있도록 초기화
                _isResting = false;
                _stateStartTime = Time.time;
                return NodeState.FAILURE;
            }

            float elapsed = Time.time - _stateStartTime;

            if (_isResting)
            {
                _controller.StopMoving();

                // 휴식 시간이 끝났는지 체크
                if (elapsed >= _restDuration)
                {
                    _isResting = false;
                    _stateStartTime = Time.time;
                }
                return NodeState.RUNNING;
            }
            else
            {
                // 달리기 시간이 끝났는지 체크
                if (elapsed >= _runDuration)
                {
                    _isResting = true;
                    _stateStartTime = Time.time;
                    _controller.StopMoving();
                    return NodeState.RUNNING;
                }

                if (_controller.IsStopped || _controller.GetComponent<NavMeshAgent>().remainingDistance < 1f)
                {
                    if (TryGetEscapePosition(out Vector3 safePosition))
                    {
                        _controller.MoveTo(safePosition);
                    }
                    else
                    {
                        // 사방이 다 막힌 최악의 경우 일단 정지
                        _controller.StopMoving();
                    }
                }

                return NodeState.RUNNING;
            }
        }


        private bool TryGetEscapePosition(out Vector3 result)
        {
            // 플레이어의 완벽한 반대 방향 백터 계산
            Vector3 directionAway = (_controller.transform.position - _controller.CurrentTarget.position).normalized;
            directionAway.y = 0f;

            // 미리 정의된 각도 순서대로 대피 가능 여부 체크
            foreach (float angle in _escapeAngles)
            {
                // 방향 백터를 각도만큼 회전 (0도면 그대로, 90도면 우측 등)
                Vector3 testDirection = Quaternion.Euler(0, angle, 0) * directionAway;
                Vector3 testPosition = _controller.transform.position + (testDirection * _fleeDistance);

                if (NavMesh.SamplePosition(testPosition, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                {
                    // NavMesh 상의 갈 수 있는 길만을 기준으로 벽이 있는지 판별
                    if (!NavMesh.Raycast(_controller.transform.position, hit.position, out NavMeshHit rayHit, NavMesh.AllAreas))
                    {
                        result = hit.position;
                        return true; // 가장 먼저 발견된 뚫린 길로 즉시 결정
                    }
                }
            }

            result = Vector3.zero;
            return false;
        }
    }
}