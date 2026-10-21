using Wooduduk.Network.Match;
using System;
using Wooduduk.Data;

namespace Wooduduk.Core
{
    /// <summary>
    /// ServerEvents
    ///  - 로그인
    ///  - 유저 데이터 로드
    ///  - 로그아웃
    ///  - 매치 참가
    ///  - 매치 시작
    ///  - 매치 종료
    ///
    /// 게임 플레이 이벤트가 아닌
    /// 서버 및 계정 흐름 이벤트만 관리
    /// </summary>
    public static class ServerEvents
    {
        /// <summary>
        /// 로그인 성공
        /// Raise : FirebaseManager
        /// Listen : SaveManager
        /// uid 전달
        /// </summary>
        public static Action<string, string> OnLoginSucceeded;
        public static void RaiseLoginSucceeded(string uid, string nickname)
        {
            OnLoginSucceeded?.Invoke(uid, nickname);
        }

        /// <summary>
        /// 유저 데이터 로드 완료
        /// Raise : SaveManager
        /// Listen : UserManager
        ///  LobbyUI
        ///  TitleUI
        /// UserData 전달
        /// </summary>
        public static event Action<UserData> OnUserDataLoaded;
        public static void RaiseUserDataLoaded(UserData data)
        {
            OnUserDataLoaded?.Invoke(data);
        }

        /// <summary>
        /// 로그아웃
        /// Raise : LogoutButton, FirebaseManager
        /// Listen : SaveManager, UserManager
        /// 현재 유저 데이터 저장
        /// 캐시 정리
        /// 로그인 화면 이동
        /// </summary>
        public static event Action<UserData> OnLogout;

        public static void RaiseLogout(UserData data)
        {
            OnLogout?.Invoke(data);
        }

        // =========================
        // MATCH FLOW
        // =========================


        /// <summary>
        /// 매치 참가 성공
        /// Raise : MatchManager
        /// Listen : LobbyUI, MatchUI
        /// matchId 전달
        /// </summary>
        public static event Action<string> OnMatchJoined;

        public static void RaiseMatchJoined(string matchId)
        {
            OnMatchJoined?.Invoke(matchId);
        }

        /// <summary>
        /// 매치 시작
        /// Raise : MatchManager
        /// Listen : SessionManager, LoadingUI
        /// matchId 전달
        /// </summary>
        public static event Action<string> OnMatchStarted;

        public static void RaiseMatchStarted(string matchId)
        {
            OnMatchStarted?.Invoke(matchId);
        }

        /// <summary>
        /// 매치 종료
        /// Raise : SessionManager 
        /// Listen : SaveManager, ResultUI, UserManager
        /// 레이드 종료 후
        /// 정산 및 저장 처리
        /// </summary>
        public static event Action<MatchResultData> OnMatchEnded;

        public static void RaiseMatchEnded(MatchResultData result)
        {
            OnMatchEnded?.Invoke(result);
        }

    }
}
