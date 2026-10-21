namespace Wooduduk.Network.Match
{
    public class MatchState
    {
        // 매칭 정보
        public string _matchId;

        public float _elapsedTime;

        // 생존 플레이어
        public int _alivePlayerCount;

        // 출구 생성 여부
        public bool _extractionOpened;
        public bool _isEnded;
    }
}
