using System.Collections.Generic;
using Unity.Services.Multiplayer;
using Wooduduk.Data;

namespace Wooduduk.Network.Match
{
    /// <summary>
    /// 서버 기준
    /// MatchSession 여러 개를 관리한다.
    /// </summary>
    public class MatchRegistry
    {
        /// <summary>
        /// 현재 생성된 모든 매치
        /// Key : MatchId
        /// Value : MatchSession
        /// </summary>
        private readonly Dictionary<string, MatchSession> _matches = new();
        /// <summary>
        /// 외부에서 읽기 전용으로 세션 목록 접근
        /// </summary>
        public IReadOnlyDictionary<string, MatchSession> AllMatches => _matches;

        // 모든 세션이 공유하는 존 축소 페이즈 (서버 시작 시 1회 주입)
        private List<ZoneData> _zonePhases;
        public void SetZonePhases(List<ZoneData> zonePhases) => _zonePhases = zonePhases;

        /// <summary>
        /// 매치 생성
        /// </summary>
        public MatchSession CreateMatch(MatchConfigData config)
        {
            string matchId = System.Guid.NewGuid().ToString();

            var session = new MatchSession(matchId, config, _zonePhases);

            _matches.Add(matchId, session);

            return session;
        }

        /// <summary>
        /// 특정 매치 가져오기
        /// (디버그 / 관리용)
        /// </summary>
        public MatchSession GetMatch(string matchId)
        {
            return _matches.TryGetValue(matchId, out var session) ? session : null;
        }

        /// <summary>
        /// 매치 삭제
        /// 게임 종료 시 호출 메모리 정리
        /// </summary>
        public void RemoveMatch(string matchId)
        {
            if (_matches.TryGetValue(matchId, out var session))
            {
                // 세션이 완전히 사라지기 전에 네트워크 객체들을 먼저 정리!
                session.Cleanup();
                _matches.Remove(matchId);
            }
        }
    }
}