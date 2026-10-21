using System;

namespace Wooduduk.Lobby
{
    /// <summary>
    /// 로비에서 확정한 런 시작 데이터를 임시로 보관하는 저장소입니다.
    ///
    /// 같은 씬에서 사용할 것이므로 씬 이동용이 아니라,
    /// 로비 UI와 인게임 시스템 사이의 데이터 전달용입니다.
    /// </summary>
    public static class LobbyRunDataStore
    {
        public static LobbyRunLoadoutData Current { get; private set; }

        public static event Action<LobbyRunLoadoutData> OnRunDataReady;

        public static bool HasData
        {
            get
            {
                return Current != null && Current.IsValid;
            }
        }

        /// <summary>
        /// 런 데이터를 저장합니다.
        /// notifyImmediately가 true면 저장 직후 인게임 쪽에 알립니다.
        /// </summary>
        public static void Set(LobbyRunLoadoutData data, bool notifyImmediately = true)
        {
            Current = data;

            if (notifyImmediately)
            {
                NotifyCurrentDataReady();
            }
        }

        public static LobbyRunLoadoutData Get()
        {
            return Current;
        }

        /// <summary>
        /// 현재 저장된 런 데이터를 인게임 시스템에 알립니다.
        /// </summary>
        public static void NotifyCurrentDataReady()
        {
            if (!HasData)
                return;

            OnRunDataReady?.Invoke(Current);
        }

        public static void Clear()
        {
            Current = null;
        }
    }
}