using UnityEngine;
using Wooduduk.AI.BT;
using Wooduduk.Core;

namespace Wooduduk.AI.EnemyAI.AINode
{
    public class AttackTargetNode : Node
    {
        private readonly BaseAIController _controller;
        private float _lastAttackTime = -999f;

        public AttackTargetNode(BaseAIController controller)
        {
            _controller = controller;
        }

        public override NodeState Evaluate()
        {
            // 타겟이 유효하지 않으면 실패
            if (!_controller.IsTargetValid()) return NodeState.FAILURE;

            // 타겟 바라보기
            Vector3 directionToTarget = _controller.CurrentTarget.position - _controller.transform.position;
            directionToTarget.y = 0f;

            // 방향 벡터가 0이 아닐 때만 회전 (제자리에 완벽히 겹쳤을 때의 에러 방지)
            if (directionToTarget.sqrMagnitude > 0.001f)
            {
                _controller.transform.rotation = Quaternion.LookRotation(directionToTarget);
            }

            // 쿨타임 계산 (PlayerStats의 SwingSpeed 활용. 예: 속도가 1이면 1초, 2면 0.5초)
            float cooldown = 2.0f / Mathf.Max(0.1f, _controller.Stats.SwingSpeed);

            if (Time.time - _lastAttackTime >= cooldown)
            {
                // 플레이어의 차징 시스템을 사용하지 않으므로 기본 타격(NORMAL)으로 강제 스윙
                if (_controller.TryGetComponent<Combat.AttackLogic>(out var attackLogic))
                {
                    attackLogic.Swing();
                }
                _lastAttackTime = Time.time;
                return NodeState.SUCCESS;
            }

            // 쿨타임 대기 중에는 현재 시퀀스를 유지(대기)하기 위해 RUNNING 반환
            return NodeState.RUNNING;
        }
    }
}