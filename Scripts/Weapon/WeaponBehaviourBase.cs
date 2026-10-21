using Wooduduk.Combat;
using UnityEngine;

namespace Wooduduk.Weapon
{
    public abstract class WeaponBehaviourBase : MonoBehaviour, IWeaponBehaviour
    {
        protected GameObject Owner { get; private set; }
        protected int Damage { get; private set; }
        public virtual void Begin(GameObject owner, int damage)
        {
            Owner = owner;
            Damage = damage;
        }
        public abstract void End();
    }
}
