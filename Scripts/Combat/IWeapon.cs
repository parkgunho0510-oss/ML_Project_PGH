using System;
using System.Collections.Generic;
using System.Text;

namespace Wooduduk.Combat
{
    public interface IWeapon
    {
        string WeaponId {  get; }
        string DisplayName { get; }
        int AttackDamage { get; }
        float AttackRange { get; }
        float SwingSpeed { get; }
        int HealthBonus { get; }
    }
}
