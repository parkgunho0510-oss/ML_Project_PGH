namespace Wooduduk.Data
{
    // 존 축소 페이즈 한 줄. StartTime부터 ShrinkDuration 동안 반경이 TargetRatio까지 줄어듦.
    // 여러 줄(Z001 Base, Z002 Shrink1 ...)로 단계별 축소를 표현. 20분 초과 전역불은 Match_Config.TimeOverDamage가 담당.
    [System.Serializable]
    public class ZoneData
    {
        public string _id;
        public string _name;

        public float _startTime;          // 이 축소가 시작되는 시각(초)
        public float _shrinkDuration;     // 목표 반경까지 줄어드는 데 걸리는 시간(초)
        public float _targetRatio = 1f;   // 줄어든 뒤 반경 비율 (0~1, Match_Config.MapRadius 기준)
        public float _outDamage;          // 경계(산불) 밖 1틱당 데미지
    }
}
