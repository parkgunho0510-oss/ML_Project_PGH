namespace Wooduduk.Data
{
    // 무기 능력치 (밖에서 장작으로 제작/해금, 들고 온 무기로 시작, 인겜에서 상대 무기 줍기 가능)
    [System.Serializable]
    public class WeaponData
    {
        public string _id;
        public string _name;
        public float _attackDamage;
        public float _attackInterval; // 공격 간격(초). 낮을수록 빠름
        public float _attackRange;    // 공격 사거리(리치)
        public float _maxHpBonus;     // 이 무기를 들었을 때 최대 체력 보정
        public int _unlockCost;       // 제작/해금에 드는 장작
        public bool _isDefault;       // 기본 지급 무기 여부 (기본 도끼)
    }
}
