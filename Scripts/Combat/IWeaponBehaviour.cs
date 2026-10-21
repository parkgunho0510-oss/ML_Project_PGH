using UnityEngine;

namespace Wooduduk.Combat
{
    public interface IWeaponBehaviour
    {
        void Begin(GameObject owner, int damage); // 공격 시작
        void End();                               // 공격 종료
    }
}
