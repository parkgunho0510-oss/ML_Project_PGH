using UnityEngine;
using Wooduduk.Combat;
using Wooduduk.Core;
using Wooduduk.Data;

namespace Wooduduk.Player
{
    public class LoadoutBaseStats : IBaseStats
    {
        private readonly CharacterData _c;
        private readonly IWeapon _w;
        public LoadoutBaseStats(CharacterData c, IWeapon w) { _c = c; _w = w; }
        public float MoveSpeed => _c._moveSpeed;
        public float SwingRange => _w?.AttackRange ?? 0f;
        public float SwingDamage => _w?.AttackDamage ?? 0f;
        public float SwingSpeed => _w?.SwingSpeed ?? 0f;
        public int MaxHealth => Mathf.RoundToInt(_c._maxHp) + (_w?.HealthBonus ?? 0);
        public float Defense => _c._defense;
    }

}
