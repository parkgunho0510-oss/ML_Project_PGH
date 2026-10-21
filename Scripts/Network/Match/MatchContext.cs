using UnityEngine;
using Wooduduk.Core;

namespace Wooduduk.Network.Match
{
    /// <summary>
    /// Unity에서 사용하는 세션 관리자
    /// 현재 플레이 중인 세션을 관리
    /// </summary>
    public class MatchContext : MonoBehaviour
    {
        public static MatchContext Instance;

        private MatchRegistry _matchRegistry;

        /// <summary>
        /// 현재 플레이 중인 세션
        /// </summary>
        public MatchSession CurrentSession { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            _matchRegistry = new MatchRegistry();
        }

        private void OnEnable()
        {
            ServerEvents.OnMatchStarted += CreateSession;
            ServerEvents.OnMatchEnded += HandleMatchEnded;
        }

        private void OnDisable()
        {
            ServerEvents.OnMatchStarted -= CreateSession;
            ServerEvents.OnMatchEnded -= HandleMatchEnded;
        }

        /// <summary>
        /// 게임 시작
        /// </summary>
        public void CreateSession(string matchId)
        {
            // CurrentSession = _matchManager.CreateMatch(matchId);
        }

        /// <summary>
        /// 매치 끝
        /// </summary>
        private void HandleMatchEnded(MatchResultData result)
        {
            EndSession();
        }

        /// <summary>
        /// 게임 종료
        /// </summary>
        public void EndSession()
        {
            if (CurrentSession == null) return;

            _matchRegistry.RemoveMatch(CurrentSession.State._matchId);

            CurrentSession = null;
        }
    }
}