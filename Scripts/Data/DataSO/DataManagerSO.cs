using System.Collections.Generic;
using UnityEngine;
using Wooduduk.Data.Core.TSVConvert;

namespace Wooduduk.Data.DataSO
{
    [CreateAssetMenu(fileName = "DataManager", menuName = "GameData/DataManager")]
    public class DataManagerSO : ScriptableObject
    {
        // 에디터에서 에셋들을 드래그해서 넣어둘 리스트
        public List<CharacterDataSO> characterDataList;
        public List<TreeDataSO> treeDataList;
        public List<TraitDataSO> traitDataList;
        public List<SkinDataSO> skinDataList;
        public List<MatchDataSO> matchDataList;
        public List<ZoneDataSO> zoneDataList;
        public List<TraitEffectDataSO> traitEffectDataList; // 특성 레벨별 패시브 효과
        public List<WeaponDataSO> weaponDataList;

        // 런타임용 딕셔너리
        private Dictionary<string, CharacterDataSO> _characterDict;
        private Dictionary<string, TreeDataSO> _treeDict;
        private Dictionary<string, TraitDataSO> _traitDict;
        private Dictionary<string, SkinDataSO> _skinDict;
        private Dictionary<string, MatchDataSO> _matchDict;
        private Dictionary<string, ZoneDataSO> _zoneDict;
        private Dictionary<string, WeaponDataSO> _weaponDict;

        // 수동 초기화 (GameManager 등에서 호출)
        public void Initialize()
        {
            _characterDict = CreateDict(characterDataList);
            _treeDict = CreateDict(treeDataList);
            _traitDict = CreateDict(traitDataList);
            _skinDict = CreateDict(skinDataList);
            _matchDict = CreateDict(matchDataList);
            _zoneDict = CreateDict(zoneDataList);
            _weaponDict = CreateDict(weaponDataList);

            Debug.Log("DataManagerSO 초기화 완료!");
        }

        // DataManager 리스트 받아오기
        private Dictionary<string, T> CreateDict<T>(List<T> list) where T : ScriptableObject, ISheetData
        {
            var dict = new Dictionary<string, T>();
            foreach (var item in list)
            {
                if (!dict.ContainsKey(item.ID))
                    dict.Add(item.ID, item);
                else
                    Debug.LogWarning($"[DataManager] 중복 ID 발견: {item.ID}");
            }
            return dict;
        }

        public CharacterDataSO GetCharacterData(string id)
        {
            if (_characterDict == null) Initialize(); // 혹시라도 초기화 안 됐으면 호출
            return _characterDict.TryGetValue(id, out var so) ? so : null;
        }
        public TreeDataSO GetTreeData(string id)
        {
            if (_treeDict == null) Initialize();
            return _treeDict.TryGetValue(id, out var so) ? so : null;
        }

        public SkinDataSO GetSkin(string id)
        {
            if (_skinDict == null) Initialize(); // 초기화 안 되어 있으면 호출
            return _skinDict.TryGetValue(id, out var so) ? so : null;
        }

        public MatchDataSO GetMatchData(string id)
        {
            if (_matchDict == null) Initialize(); // 초기화 안 되어 있으면 호출
            return _matchDict.TryGetValue(id, out var so) ? so : null;
        }

        // 매치 설정은 보통 1개(M001)만 쓰므로 기본값 편의 함수 제공
        public MatchDataSO GetDefaultMatch()
        {
            if (_matchDict == null) Initialize();
            return matchDataList != null && matchDataList.Count > 0 ? matchDataList[0] : null;
        }

        public ZoneDataSO GetZoneData(string id)
        {
            if (_zoneDict == null) Initialize(); // 초기화 안 되어 있으면 호출
            return _zoneDict.TryGetValue(id, out var so) ? so : null;
        }

        // 존 축소 페이즈들 (StartTime 오름차순 정렬). 런타임이 경과시간으로 현재 페이즈 판단.
        public List<ZoneDataSO> GetZonePhases()
        {
            if (_zoneDict == null) Initialize();
            var list = new List<ZoneDataSO>(zoneDataList);
            list.Sort((a, b) => a._data._startTime.CompareTo(b._data._startTime));
            return list;
        }

        public TraitDataSO GetTraitData(string id)
        {
            if (_traitDict == null) Initialize(); // 초기화 안 되어 있으면 호출
            return _traitDict.TryGetValue(id, out var so) ? so : null;
        }

        // 특성의 레벨별 효과들 (런타임에서 현재 레벨 이하만 골라 적용)
        public List<TraitEffectDataSO> GetTraitEffects(string traitId)
        {
            var result = new List<TraitEffectDataSO>();
            if (traitEffectDataList == null) return result;
            foreach (var e in traitEffectDataList)
                if (e != null && e._data != null && e._data._traitId == traitId)
                    result.Add(e);
            return result;
        }

        public WeaponDataSO GetWeaponData(string id)
        {
            if (_weaponDict == null) Initialize(); // 초기화 안 되어 있으면 호출
            return _weaponDict.TryGetValue(id, out var so) ? so : null;
        }

        // 기본 지급 무기(기본 도끼) 반환
        public WeaponDataSO GetDefaultWeapon()
        {
            if (_weaponDict == null) Initialize();
            if (weaponDataList == null) return null;
            foreach (var w in weaponDataList)
                if (w != null && w._data != null && w._data._isDefault)
                    return w;
            return null;
        }
    }
}