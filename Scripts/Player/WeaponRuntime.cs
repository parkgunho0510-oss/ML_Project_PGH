using UnityEngine;
using Wooduduk.Combat;
using Wooduduk.Data;

namespace Wooduduk.Player
{
    public class WeaponRuntime : IWeapon
    {
        private readonly WeaponData _d;
        public WeaponRuntime(WeaponData d) { _d = d; }
        public string WeaponId => _d._id;
        public string DisplayName => _d._name;
        public int AttackDamage => Mathf.RoundToInt(_d._attackDamage);
        public float SwingSpeed => _d._attackInterval > 0f ? 1f / _d._attackInterval : 0f; 
        public int HealthBonus => Mathf.RoundToInt(_d._maxHpBonus);
        public float AttackRange => _d._attackRange;
    }

}
