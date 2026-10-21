using System;

namespace Wooduduk.Lobby
{
    /// <summary>
    /// 로비에서 확정된 장착 정보를 인게임 시스템들에게 이벤트로 알려주는 통로입니다.
    ///
    /// 전달 정보:
    /// - 선택 맵
    /// - 장착 무기
    /// - 장착 특성
    /// - 장착 특성 레벨
    /// - 해당 레벨의 TraitEffectDataSO
    /// </summary>
    public static class InGameLoadoutEventBus
    {
        /// <summary>
        /// 로비에서 확정된 장착 정보가 준비되었을 때 1회 발행됩니다.
        /// </summary>
        public static event Action<RunLoadoutResolvedData> OnRunLoadoutResolved;

        public static void RaiseRunLoadoutResolved(RunLoadoutResolvedData resolvedData)
        {
            if (resolvedData == null || !resolvedData.IsValid)
                return;

            OnRunLoadoutResolved?.Invoke(resolvedData);
        }
    }
}