using System.Collections.Generic;
using UnityEngine;

using DataManagerSO = Wooduduk.Data.DataSO.DataManagerSO;
using WeaponDataSO = Wooduduk.Data.DataSO.WeaponDataSO;
using TraitDataSO = Wooduduk.Data.DataSO.TraitDataSO;
using TraitEffectDataSO = Wooduduk.Data.DataSO.TraitEffectDataSO;

namespace Wooduduk.Lobby
{
    /// <summary>
    /// 로비에서 선택한 ID 데이터를 실제 인게임 데이터로 변환하고,
    /// "무슨 무기 / 무슨 특성 / 몇 레벨인지"만 이벤트로 알려주는 스크립트입니다.
    ///
    /// 주의:
    /// 이 스크립트는 PlayerStats를 직접 참조하지 않습니다.
    /// 스탯 보정값도 계산하지 않습니다.
    /// </summary>
    public class InGameLoadoutApplier : MonoBehaviour
    {
        [Header("Data Manager")]
        [SerializeField] private DataManagerSO dataManager;

        [Header("옵션")]
        [SerializeField] private bool applyExistingDataOnEnable = true;
        [SerializeField] private bool preventDuplicateApply = true;
        [SerializeField] private bool clearDataAfterApply;

        private bool alreadyApplied;

        private void OnEnable()
        {
            LobbyRunDataStore.OnRunDataReady += OnRunDataReady;

            if (applyExistingDataOnEnable && LobbyRunDataStore.HasData)
            {
                ResolveAndPublishRunData(LobbyRunDataStore.Get());
            }
        }

        private void OnDisable()
        {
            LobbyRunDataStore.OnRunDataReady -= OnRunDataReady;
        }

        private void OnRunDataReady(LobbyRunLoadoutData runData)
        {
            ResolveAndPublishRunData(runData);
        }

        private void ResolveAndPublishRunData(LobbyRunLoadoutData runData)
        {
            if (preventDuplicateApply && alreadyApplied)
                return;

            if (runData == null || !runData.IsValid)
            {
                Debug.LogWarning("[InGameLoadoutApplier] 유효하지 않은 로비 선택 데이터입니다.");
                return;
            }

            if (dataManager == null)
            {
                Debug.LogError("[InGameLoadoutApplier] DataManagerSO가 연결되지 않았습니다.");
                return;
            }

            dataManager.Initialize();

            WeaponDataSO weaponData = ResolveWeaponData(runData.WeaponId);

            if (weaponData == null)
            {
                Debug.LogError($"[InGameLoadoutApplier] 무기 데이터를 찾지 못했습니다. WeaponId: {runData.WeaponId}");
                return;
            }

            RunWeaponResolvedData resolvedWeapon = new RunWeaponResolvedData(weaponData);
            List<RunTraitResolvedData> resolvedTraits = ResolveTraits(runData.TraitSelections);

            RunLoadoutResolvedData resolvedData = new RunLoadoutResolvedData(
                runData.MapName,
                resolvedWeapon,
                resolvedTraits
            );

            alreadyApplied = true;

            Debug.Log("========== [인게임 장착 정보 변환 완료] ==========");
            Debug.Log($"원본 데이터: {runData}");
            Debug.Log($"변환 데이터: {resolvedData}");
            Debug.Log("이제 InGameLoadoutEventBus.OnRunLoadoutResolved 이벤트를 발행합니다.");
            Debug.Log("===============================================");

            InGameLoadoutEventBus.RaiseRunLoadoutResolved(resolvedData);

            if (clearDataAfterApply)
            {
                LobbyRunDataStore.Clear();
            }
        }

        private WeaponDataSO ResolveWeaponData(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId))
                return null;

            WeaponDataSO weaponData = dataManager.GetWeaponData(weaponId);

            if (weaponData == null || weaponData._data == null)
                return null;

            Debug.Log(
                $"[InGameLoadoutApplier] 장착 무기 변환 성공: " +
                $"{weaponData._data._id} / {weaponData._data._name}"
            );

            return weaponData;
        }

        private List<RunTraitResolvedData> ResolveTraits(
            IReadOnlyList<LobbyRunTraitSelectionData> traitSelections
        )
        {
            List<RunTraitResolvedData> result = new List<RunTraitResolvedData>();

            if (traitSelections == null || traitSelections.Count <= 0)
            {
                Debug.Log("[InGameLoadoutApplier] 장착 특성 없음");
                return result;
            }

            foreach (LobbyRunTraitSelectionData selection in traitSelections)
            {
                if (selection == null || string.IsNullOrEmpty(selection.TraitId))
                    continue;

                TraitDataSO traitData = dataManager.GetTraitData(selection.TraitId);

                if (traitData == null || traitData._data == null)
                {
                    Debug.LogWarning($"[InGameLoadoutApplier] 특성 데이터를 찾지 못했습니다. TraitId: {selection.TraitId}");
                    continue;
                }

                List<TraitEffectDataSO> effects = GetTraitEffectsById(selection.TraitId);
                TraitEffectDataSO effectForLevel = FindEffectByLevel(effects, selection.Level);

                int resolvedLevel = selection.Level;

                if (effectForLevel != null && effectForLevel._data != null)
                {
                    resolvedLevel = effectForLevel._data._level;
                }

                RunTraitResolvedData resolvedTrait = new RunTraitResolvedData(
                    traitData,
                    effectForLevel,
                    resolvedLevel
                );

                result.Add(resolvedTrait);

                Debug.Log(
                    $"[InGameLoadoutApplier] 장착 특성 변환 성공: " +
                    $"{resolvedTrait.TraitId} / {resolvedTrait.TraitName} Lv.{resolvedTrait.Level}"
                );
            }

            return result;
        }

        private List<TraitEffectDataSO> GetTraitEffectsById(string traitId)
        {
            List<TraitEffectDataSO> result = new List<TraitEffectDataSO>();

            if (dataManager == null || dataManager.traitEffectDataList == null)
                return result;

            string normalizedTraitId = NormalizeId(traitId);

            if (string.IsNullOrEmpty(normalizedTraitId))
                return result;

            foreach (TraitEffectDataSO effectData in dataManager.traitEffectDataList)
            {
                if (effectData == null || effectData._data == null)
                    continue;

                string effectTraitId = NormalizeId(effectData._data._traitId);

                if (effectTraitId == normalizedTraitId)
                {
                    result.Add(effectData);
                }
            }

            return result;
        }

        private TraitEffectDataSO FindEffectByLevel(List<TraitEffectDataSO> effects, int targetLevel)
        {
            if (effects == null || effects.Count <= 0)
                return null;

            int safeLevel = Mathf.Max(1, targetLevel);

            for (int i = 0; i < effects.Count; i++)
            {
                TraitEffectDataSO effect = effects[i];

                if (effect == null || effect._data == null)
                    continue;

                if (effect._data._level == safeLevel)
                    return effect;
            }

            for (int i = 0; i < effects.Count; i++)
            {
                TraitEffectDataSO effect = effects[i];

                if (effect == null || effect._data == null)
                    continue;

                if (effect._data._level == 1)
                    return effect;
            }

            return effects[0];
        }

        private string NormalizeId(string id)
        {
            if (string.IsNullOrEmpty(id))
                return string.Empty;

            return id.Trim();
        }
    }
}