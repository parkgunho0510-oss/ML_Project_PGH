using System;
using UnityEngine;
using Wooduduk.Core;

namespace Wooduduk.Player
{
    // Player 모듈 내부 이벤트 채널
    public static class PlayerEvents
    {
        // 입력
        public static event Action<Vector2> OnMove;
        public static event Action OnAttackPressed;
        public static event Action OnAttackReleased;
        public static event Action OnAttackConfirmed;

        // 상호작용 진행도 (UI 게이지)
        public static event Action<float> OnInteractProgress;
        

        // 차지잉
        //public static event Action OnChargeStarted;
        //public static event Action<float> OnChargeProgress;
        //public static event Action<ChargeQuality> OnChargeReleased;

        // 스윙
        public static event Action<GameObject> OnSwingHit;   // 히트 시점에 타격한 오브젝트 전달

        // 이벤트 메서드
        public static void RaiseMove(Vector2 v) => OnMove?.Invoke(v);
        public static void RaiseAttackPressed() => OnAttackPressed?.Invoke();
        public static void RaiseAttackReleased() => OnAttackReleased?.Invoke();
        //public static void RaiseChargeStarted() => OnChargeStarted?.Invoke();
        //public static void RaiseChargeProgress(float p) => OnChargeProgress?.Invoke(p);
        //public static void RaiseChargeReleased(ChargeQuality q) => OnChargeReleased?.Invoke(q);
        public static void RaiseSwingHit(GameObject target) => OnSwingHit?.Invoke(target);
        public static void RaiseInteractProgress(float p ) => OnInteractProgress?.Invoke(p);
        public static void RaiseAttackConfirmed() => OnAttackConfirmed?.Invoke();
    }
}