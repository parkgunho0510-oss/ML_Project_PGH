using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wooduduk.Lobby
{
    /// <summary>
    /// 로비에서 선택한 특성 정보입니다.
    /// 현재는 기본 레벨 1로 사용합니다.
    /// 나중에 특성 강화 시스템이 생기면 level 값을 실제 보유 레벨로 넣으면 됩니다.
    /// </summary>
    [Serializable]
    public class LobbyRunTraitSelectionData
    {
        [SerializeField] private string traitId;
        [SerializeField] private int level;

        public string TraitId => traitId;
        public int Level => level;

        public LobbyRunTraitSelectionData(string traitId, int level)
        {
            this.traitId = traitId;
            this.level = Mathf.Max(1, level);
        }

        public override string ToString()
        {
            return $"{traitId} Lv.{level}";
        }
    }

    /// <summary>
    /// 로비에서 게임 시작 시 확정한 선택 데이터입니다.
    ///
    /// ScriptableObject 자체를 넘기지 않고 ID와 레벨만 저장합니다.
    /// 인게임에서는 이 ID를 기준으로 DataManagerSO에서 다시 데이터를 찾습니다.
    /// </summary>
    [Serializable]
    public class LobbyRunLoadoutData
    {
        [SerializeField] private string mapName;
        [SerializeField] private string weaponId;
        [SerializeField] private List<LobbyRunTraitSelectionData> traitSelections = new List<LobbyRunTraitSelectionData>();

        public string MapName => mapName;
        public string WeaponId => weaponId;
        public IReadOnlyList<LobbyRunTraitSelectionData> TraitSelections => traitSelections;

        /// <summary>
        /// 기존 코드 호환용입니다.
        /// 특성 ID만 필요한 경우 사용할 수 있습니다.
        /// </summary>
        public IReadOnlyList<string> TraitIds
        {
            get
            {
                List<string> result = new List<string>();

                foreach (LobbyRunTraitSelectionData selection in traitSelections)
                {
                    if (selection == null || string.IsNullOrEmpty(selection.TraitId))
                        continue;

                    result.Add(selection.TraitId);
                }

                return result;
            }
        }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrEmpty(mapName)
                    && !string.IsNullOrEmpty(weaponId);
            }
        }

        /// <summary>
        /// 현재 로비에서는 특성 레벨 시스템이 없으므로,
        /// traitIds로 들어온 특성들은 전부 Level 1로 처리합니다.
        /// </summary>
        public LobbyRunLoadoutData(string mapName, string weaponId, IEnumerable<string> traitIds)
        {
            this.mapName = mapName;
            this.weaponId = weaponId;
            this.traitSelections = new List<LobbyRunTraitSelectionData>();

            if (traitIds == null)
                return;

            foreach (string traitId in traitIds)
            {
                if (string.IsNullOrEmpty(traitId))
                    continue;

                traitSelections.Add(new LobbyRunTraitSelectionData(traitId, 1));
            }
        }

        /// <summary>
        /// 나중에 특성 레벨까지 로비에서 정하게 되면 이 생성자를 사용하면 됩니다.
        /// </summary>
        public LobbyRunLoadoutData(
            string mapName,
            string weaponId,
            IEnumerable<LobbyRunTraitSelectionData> traitSelections
        )
        {
            this.mapName = mapName;
            this.weaponId = weaponId;
            this.traitSelections = new List<LobbyRunTraitSelectionData>();

            if (traitSelections == null)
                return;

            foreach (LobbyRunTraitSelectionData selection in traitSelections)
            {
                if (selection == null || string.IsNullOrEmpty(selection.TraitId))
                    continue;

                this.traitSelections.Add(selection);
            }
        }

        public override string ToString()
        {
            string traitText = traitSelections == null || traitSelections.Count <= 0
                ? "없음"
                : string.Join(", ", traitSelections);

            return $"Map: {mapName}, Weapon: {weaponId}, Traits: {traitText}";
        }
    }
}