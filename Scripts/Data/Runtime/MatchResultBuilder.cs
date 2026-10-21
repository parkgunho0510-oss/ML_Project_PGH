using Wooduduk.Network.Match;

namespace Wooduduk.Data.Runtime
{
    public static class MatchResultBuilder
    {
        public static MatchResultData Build(PlayerRuntimeData rd, float survivalTime = 0f)
        {
            return new MatchResultData
            {
                _userId = rd._userId,
                _userNick = rd._userNick,
                _isExtracted = rd._isExtracted,
                _isDead = rd._isDead,
                _earnedWood = rd._isExtracted ? rd._collectedWood : 0,  
                _earnedRankPoint = 0,            // TODO : 랭크 기획 확정 후
                _survivalTime = survivalTime  // TODO : 세션 시작시간 기준(손찬성)
            };
        }
    }
}
