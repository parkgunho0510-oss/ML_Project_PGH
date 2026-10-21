using UnityEngine;
using Wooduduk.Player;
using Wooduduk.AI.EnemyAI;

namespace Wooduduk.Combat
{
    // 애니메이터의 Attack State에 부착하여 공격이 강제로 끊길 때 히트박스를 닫아주는 역할
    public class AttackStateReset : StateMachineBehaviour
    {
        // 애니메이션 상태를 빠져나갈 때 (자연 종료, 피격 캔슬, 회피 캔슬 등 모든 상황) 무조건 호출됨
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //  공격 강제 종료
            if (animator.TryGetComponent<AttackLogic>(out var Attack))
            {
                Attack.Anim_Close();
            }
        }
    }
}