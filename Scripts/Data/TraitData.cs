namespace Wooduduk.Data
{
    // 특성 신원 + 성장 정보 (게임 시작 전 세팅, 1슬롯, 장작으로 레벨 해금)
    [System.Serializable]
    public class TraitData
    {
        public string _id;
        public string _name;
        public int _maxLevel;          // 최대 레벨
        public string _activeSkillId;  // 최대 레벨 도달 시 해금되는 액티브 스킬 ID
    }
}
