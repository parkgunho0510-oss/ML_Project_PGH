using System.Collections.Generic;
using UnityEngine;
using Wooduduk.Core;

namespace Wooduduk.Player
{
    // 베이스 스탯 + 모디파이어 합산 -> 최종 스탯
    // 최종 값 = 베이스 스탯 * (1 + sum(percent))
    public class PlayerStats : MonoBehaviour, IPlayerStats
    {
        // TODO : DataSO 분리
        [Header("BaseStats")]
        [SerializeField] private float _baseMoveSpeed = 5f;
        [SerializeField] private float _baseSwingRange = 2.0f;
        [SerializeField] private float _baseSwingDamage = 10f;
        [SerializeField] private float _baseSwingSpeed = 1.0f;
        [SerializeField] private float _baseDefense = 0f;
        [SerializeField] private int _baseMaxHealth = 100;

        private readonly Dictionary<StatId, List<StatModifier>> _modifiers = new();

        public void Initialize(IBaseStats bs)
        {
            if (bs == null) return;
            _baseMoveSpeed = bs.MoveSpeed;
            _baseSwingRange = bs.SwingRange;
            _baseSwingDamage = bs.SwingDamage;
            _baseSwingSpeed = bs.SwingSpeed;
            _baseDefense = bs.Defense;
            _baseMaxHealth = bs.MaxHealth;
        }
        // IPlayerStats
        public float MoveSpeed => _baseMoveSpeed * (1f + Sum(StatId.MOVE_SPEED));
        public float SwingRange => _baseSwingRange * (1f + Sum(StatId.SWING_RANGE));
        public float SwingDamage => _baseSwingDamage * (1f + Sum(StatId.SWING_DAMAGE));
        public float SwingSpeed => _baseSwingSpeed * (1f + Sum(StatId.SWING_SPEED));
        public float Defense => _baseDefense * (1f + Sum(StatId.DEFENSE));
        public int MaxHealth => Mathf.RoundToInt(_baseMaxHealth * (1f + Sum(StatId.MAX_HEALTH)));
        public void AddModifier(StatId id, StatModifier modifier)
        {
            if (!_modifiers.TryGetValue(id, out var list))
                _modifiers[id] = list = new List<StatModifier>();
            list.Add(modifier);
        }
        public void RemoveModifier(StatId id, StatModifier modifier)
        {
            if (!_modifiers.TryGetValue(id, out var list)) return;
            int removed = list.RemoveAll(m => ReferenceEquals(m.source, modifier.source));
        }

        #region Share
        private float Sum(StatId id)
        {
            if (!_modifiers.TryGetValue(id, out var list)) return 0f;
            float pct = 0f;
            for (int i = 0; i < list.Count; i++) pct += list[i].percent;
            return pct;
        }

        public void SetBaseStats(int maxHealth, float swingDamage, float moveSpeed)
        {
            _baseMaxHealth = maxHealth;
            _baseSwingDamage = swingDamage;
            _baseMoveSpeed = moveSpeed;
        }
        #endregion
    }
}