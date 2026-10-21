namespace Wooduduk.Data
{
    [System.Serializable]
    public class MatchConfigData
    {
        public string _id;
        public string _name;

        public float _sessionTime = 1200f;     // 총 게임(레이드) 시간(초) - 15~20분
        public int _playerCount = 10;             // 시작 인원 (플레이어+봇)
        public float _extractionOpenTime = 1f;  // 탈출구 개방 시각(초) - 5분 경과 시 / 테스트용으로 1초 뒤 바로 개방
        public float _readyCountdown = 3f;        // 전투 시작 전 카운트다운(초)
        public float _tickInterval = 1f;          // 데미지/처리 틱 주기(초)
        public float _mapRadius = 50f;            // 맵 전체 반경(월드 유닛) - 경계 계산 기준
        public float _timeOverDamage = 500f;      // 시간초과 시 전역 산불 1틱당 데미지 (체력만큼 버팀: 풀피면 약 2초)
    }
}
