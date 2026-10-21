using Wooduduk.Data.Core;

namespace Wooduduk.Data
{
    // 특성의 레벨별 패시브 효과 한 줄 (전부 스탯 증가라 EffectType 없이 Target+Value)
    [System.Serializable]
    public class TraitEffectData
    {
        public string _id;          // 이 효과 줄 고유 ID (TE001 ...)
        public string _name;        // 설명용 이름
        public string _traitId;     // 어떤 특성인지 (TR01 ...)
        public int _level;          // 이 효과가 적용되는 레벨
        public int _unlockCost;     // 이 레벨 해금에 드는 장작
        public float _maxHP;        // 증가 수치 (%)
        public float _moveSpeed;        // 증가 수치 (%)
        public float _attackDamage;        // 증가 수치 (%)
        public float _attackSpeed;        // 증가 수치 (%)
        public float _attackRange;        // 증가 수치 (%)
        public float _woodGainlue;        // 증가 수치 (%)
        public float _woodHeal;        // 증가 수치 (%)
        public StatType _stat;        // 증가 수치 (%)
        public float _value;        // 증가 수치 (%)
    }
}
