using System.Collections.Generic;

namespace Wooduduk.Data
{
    // 특성별 해금 레벨 (Dictionary 대신 직렬화용 구조체 리스트)
    [System.Serializable]
    public class TraitProgress
    {
        public string _traitId;  // TR001 ...
        public int _level;       // 현재 해금 레벨 (0 = 미해금)
    }

    [System.Serializable]
    public class UserData
    {
        public string _userId;
        public string _userNick;
        public int _rankPoint;
        public int _totalWood;   // 해금/제작에 쓰는 재화(장작)
        public int _jamCount;

        // 스킨 여부
        public List<string> _ownedSkinIds; // 보유 스킨
        public string _selectedSkinId; // 사용할 스킨

        // 특성 (메타 progression)
        public List<TraitProgress> _traitLevels;  // 특성별 해금 레벨
        public string _equippedTraitId;           // 장착한 특성 (1슬롯)

        // 무기 (메타 progression)
        public List<string> _ownedWeaponIds;      // 보유(제작) 무기
        public string _equippedWeaponId;          // 들고 들어갈 무기
    }
}