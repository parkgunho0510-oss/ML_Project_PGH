using Wooduduk.Data.Core;
using Wooduduk.Data.DataSO;

namespace Wooduduk.Data.Runtime
{
    /// <summary>
    /// 게임 시작 시 UserData + DataSO들을 합쳐 PlayerRuntimeData 생성.
    /// 기본값(무기=공격, 캐릭터=몸) → 장착 특성의 현재 레벨 효과(%)를 곱해 최종 스탯 계산.
    /// </summary>
    public static class PlayerFactory
    {
        public static PlayerRuntimeData Create(UserData userData, CharacterDataSO characterData, WeaponDataSO weaponData, DataManagerSO data)
        {
            // 1) 기본값: 공격은 무기, 몸은 캐릭터 (+무기 체력 보정)
            float maxHp = characterData._data._maxHp + weaponData._data._maxHpBonus;
            float attackDamage = weaponData._data._attackDamage;
            float attackRange = weaponData._data._attackRange;
            float attackInterval = weaponData._data._attackInterval;
            float defense = characterData._data._defense;
            float moveSpeed = characterData._data._moveSpeed;

            // 2) 장착 특성의 "현재 레벨" 효과만 % 적용
            string traitId = userData._equippedTraitId;
            int level = GetTraitLevel(userData, traitId);
            if (data != null && !string.IsNullOrEmpty(traitId) && level > 0)
            {
                foreach (var e in data.GetTraitEffects(traitId))
                {
                    if (e == null || e._data == null || e._data._level != level) continue;

                    var d = e._data;

                    if (d._maxHP != 0) maxHp *= 1f + d._maxHP / 100f;

                    if (d._moveSpeed != 0) moveSpeed *= 1f + d._moveSpeed / 100f;

                    if (d._attackDamage != 0) attackDamage *= 1f + d._attackDamage / 100f;

                    if (d._attackSpeed != 0) attackInterval *= 1f + d._attackSpeed / 100f;

                    if (d._attackRange != 0) attackRange *= 1f + d._attackRange / 100f;

                    //if (d._woodGainlue != 0) ; // 필요하면 woodGain 같은 변수 추가해서 연결

                    //if (d._woodHeal != 0) ; // 필요하면 회복 시스템 연결
                }
            }

            // 3) 최종 스탯으로 런타임 생성
            return new PlayerRuntimeData
            {
                _userId = userData._userId,
                _userNick = userData._userNick,
                _characterDataId = characterData.ID,

                _currentMaxHp = maxHp,
                _currentHp = maxHp,                 // 시작은 풀피
                _currentAttackDamage = attackDamage,
                _currentAttackRange = attackRange,
                _currentAttackInterval = attackInterval,
                _currentDefense = defense,
                _currentMoveSpeed = moveSpeed,

                // 장착 정보
                _equippedTraitId = traitId,
                _equippedWeaponId = userData._equippedWeaponId,

                // 런타임 초기화
                _collectedWood = 0,
                _killCount = 0,
                _isExtracted = false,
                _isDead = false,
                _isBot = false
            };
        }

        // UserData의 특성별 해금 레벨에서 해당 특성 현재 레벨 조회 (없으면 0)
        private static int GetTraitLevel(UserData userData, string traitId)
        {
            if (userData._traitLevels == null || string.IsNullOrEmpty(traitId)) return 0;
            foreach (var tp in userData._traitLevels)
                if (tp != null && tp._traitId == traitId) return tp._level;
            return 0;
        }
    }
}
