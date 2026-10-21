using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Wooduduk.Core
{
    // 데미지 받는 대상 (플레이어, AI, 나무 등)
    // PlayerAttack은 이 인터페이스
    public interface IDamageable
    {
        bool IsAlive { get; }
        void TakeDamage(int damage, GameObject source);
    }
}
