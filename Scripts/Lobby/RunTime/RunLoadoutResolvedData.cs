using System;
using System.Collections.Generic;
using UnityEngine;

using WeaponDataSO = Wooduduk.Data.DataSO.WeaponDataSO;
using TraitDataSO = Wooduduk.Data.DataSO.TraitDataSO;
using TraitEffectDataSO = Wooduduk.Data.DataSO.TraitEffectDataSO;

namespace Wooduduk.Lobby
{
    /// <summary>
    /// 인게임에 전달할 장착 무기 정보입니다.
    /// </summary>
    [Serializable]
    public class RunWeaponResolvedData
    {
        [SerializeField] private string weaponId;
        [SerializeField] private string weaponName;
        [SerializeField] private WeaponDataSO weaponData;

        public string WeaponId => weaponId;
        public string WeaponName => weaponName;
        public WeaponDataSO WeaponData => weaponData;

        public RunWeaponResolvedData(WeaponDataSO weaponData)
        {
            this.weaponData = weaponData;

            if (weaponData != null && weaponData._data != null)
            {
                weaponId = weaponData._data._id;
                weaponName = weaponData._data._name;
            }
            else
            {
                weaponId = string.Empty;
                weaponName = string.Empty;
            }
        }

        public override string ToString()
        {
            return $"{weaponId} / {weaponName}";
        }
    }

    /// <summary>
    /// 인게임에 전달할 장착 특성 정보입니다.
    /// 핵심은 TraitId, TraitName, Level입니다.
    /// TraitEffectDataSO는 해당 레벨의 효과 데이터입니다.
    /// </summary>
    [Serializable]
    public class RunTraitResolvedData
    {
        [SerializeField] private string traitId;
        [SerializeField] private string traitName;
        [SerializeField] private int level;
        [SerializeField] private TraitDataSO traitData;
        [SerializeField] private TraitEffectDataSO traitEffectData;

        public string TraitId => traitId;
        public string TraitName => traitName;
        public int Level => level;
        public TraitDataSO TraitData => traitData;
        public TraitEffectDataSO TraitEffectData => traitEffectData;

        public RunTraitResolvedData(
            TraitDataSO traitData,
            TraitEffectDataSO traitEffectData,
            int level
        )
        {
            this.traitData = traitData;
            this.traitEffectData = traitEffectData;
            this.level = Mathf.Max(1, level);

            if (traitData != null && traitData._data != null)
            {
                traitId = traitData._data._id;
                traitName = traitData._data._name;
            }
            else
            {
                traitId = string.Empty;
                traitName = string.Empty;
            }
        }

        public override string ToString()
        {
            return $"{traitId} / {traitName} Lv.{level}";
        }
    }

    /// <summary>
    /// 로비 선택 ID를 실제 데이터로 변환한 결과입니다.
    ///
    /// 이 데이터는 스탯 계산값이 아니라,
    /// "이번 판에 무엇을 장착했는지"를 알려주는 정보입니다.
    /// </summary>
    [Serializable]
    public class RunLoadoutResolvedData
    {
        [SerializeField] private string mapName;
        [SerializeField] private RunWeaponResolvedData weapon;
        [SerializeField] private List<RunTraitResolvedData> traits = new List<RunTraitResolvedData>();

        public string MapName => mapName;
        public RunWeaponResolvedData Weapon => weapon;
        public IReadOnlyList<RunTraitResolvedData> Traits => traits;

        /// <summary>
        /// 기존 무기 시스템에서 WeaponDataSO가 바로 필요할 수 있으므로 편의 프로퍼티를 둡니다.
        /// </summary>
        public WeaponDataSO WeaponData
        {
            get
            {
                if (weapon == null)
                    return null;

                return weapon.WeaponData;
            }
        }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrEmpty(mapName)
                    && weapon != null
                    && weapon.WeaponData != null;
            }
        }

        public RunLoadoutResolvedData(
            string mapName,
            RunWeaponResolvedData weapon,
            List<RunTraitResolvedData> traits
        )
        {
            this.mapName = mapName;
            this.weapon = weapon;
            this.traits = traits ?? new List<RunTraitResolvedData>();
        }

        public override string ToString()
        {
            string weaponText = weapon == null ? "없음" : weapon.ToString();
            string traitText = traits == null || traits.Count <= 0
                ? "없음"
                : string.Join(", ", traits);

            return $"Map: {mapName}, Weapon: {weaponText}, Traits: {traitText}";
        }
    }
}