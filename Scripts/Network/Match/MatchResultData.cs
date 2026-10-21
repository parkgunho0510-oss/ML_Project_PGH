namespace Wooduduk.Network.Match
{
    [System.Serializable]
    public class MatchResultData // 데이터 정산
    {
        public string _userId;
        public string _userNick;

        // 탈출, 죽음 여부
        public bool _isExtracted;
        public bool _isDead;

        // 획득 목재, 랭크 포인트
        public int _earnedWood;
        public int _earnedRankPoint;

        // 생존 시간
        public float _survivalTime;
    }
}
