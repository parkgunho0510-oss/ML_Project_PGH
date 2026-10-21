using System;
using UnityEngine;

namespace Wooduduk.Core
{
    public enum StatId
    {
        MOVE_SPEED,
        SWING_RANGE,
        SWING_DAMAGE,
        SWING_SPEED,
        MAX_HEALTH,
        DEFENSE,
    }

    // 스탯 가산용 모디파이어
    // 최종값 = base * (1 + percent) 
    [Serializable]
    public struct StatModifier
    {
        public float percent;
        public object source;   // 식별용 객체 (아이템, 효과 등)
    }

    // PlayerStats 인터페이스
    // 외부 시스템 연동을 위한 인터페이스
    public interface IPlayerStats
    {
        float MoveSpeed { get; }
        float SwingRange { get; }
        float SwingDamage { get; }
        float SwingSpeed { get; }
        float Defense { get; }
        int MaxHealth { get; }

        void AddModifier(StatId statId, StatModifier modifier);
        void RemoveModifier(StatId statId, StatModifier source);
    }

}