using UnityEngine;
using Wooduduk.Core;
using Wooduduk.Data.Runtime;

namespace Wooduduk.Player
{
    // 서버가 계산한 최종 스탯 -> 인게임 PlayerStats 주입용 IBaseStats 어댑터
    public class PlayerRuntimeBaseStats : IBaseStats
    {
        private readonly PlayerRuntimeData _r;
        public PlayerRuntimeBaseStats(PlayerRuntimeData r) { _r = r; }

        public float MoveSpeed => _r._currentMoveSpeed;
        public float SwingRange => _r._currentAttackRange;
        public float SwingDamage => _r._currentAttackDamage;
        // 간격(초/회) → 속도(회/초). PlayerFactory가 특성 %까지 반영한 최종 간격 기준
        public float SwingSpeed => _r._currentAttackInterval > 0f ? 1f / _r._currentAttackInterval : 0f;
        public int MaxHealth => Mathf.RoundToInt(_r._currentMaxHp);
        public float Defense => _r._currentDefense;
    }
}
