using System;
using System.Collections.Generic;
using UnityEngine;
using Wooduduk.Data;

using DataManagerSO = Wooduduk.Data.DataSO.DataManagerSO;
using TraitDataSO = Wooduduk.Data.DataSO.TraitDataSO;
using TraitEffectDataSO = Wooduduk.Data.DataSO.TraitEffectDataSO;
using Wooduduk.Data.DataSystem;
using WeaponDataSO = Wooduduk.Data.DataSO.WeaponDataSO;

namespace Wooduduk.Lobby
{
    /// <summary>
    /// 로비 로드아웃 선택 상태를 관리합니다.
    ///
    /// DataManagerSO는 직접 연결하지 않고,
    /// 같은 GameObject의 LobbyInventoryManager를 통해 가져옵니다.
    /// </summary>
    [RequireComponent(typeof(LobbyInventoryManager))]
    public class DataDrivenLobbyLoadoutManager : MonoBehaviour
    {
        [Header("특성 슬롯 설정")]
        [SerializeField] private int maxTraitSlotCount = 1;

        [Header("디버그")]
        [SerializeField] private bool logOnStart = true;
        [SerializeField] private bool logTraitEffectDetail = true;

        private LobbyInventoryManager inventoryManager;
        private DataManagerSO dataManager;

        public WeaponDataSO SelectedWeapon { get; private set; }

        public IReadOnlyList<TraitDataSO> SelectedTraits => selectedTraits;

        public IReadOnlyList<WeaponDataSO> AllWeapons
        {
            get
            {
                ResolveReferences();

                if (dataManager == null || dataManager.weaponDataList == null)
                    return emptyWeapons;

                return dataManager.weaponDataList;
            }
        }

        public IReadOnlyList<TraitDataSO> AllTraits
        {
            get
            {
                ResolveReferences();

                if (dataManager == null || dataManager.traitDataList == null)
                    return emptyTraits;

                return dataManager.traitDataList;
            }
        }

        public IReadOnlyList<TraitEffectDataSO> AllTraitEffects
        {
            get
            {
                ResolveReferences();

                if (dataManager == null || dataManager.traitEffectDataList == null)
                    return emptyTraitEffects;

                return dataManager.traitEffectDataList;
            }
        }

        public IReadOnlyList<WeaponDataSO> AvailableWeapons => GetOwnedWeapons();
        public IReadOnlyList<TraitDataSO> AvailableTraits => GetOwnedTraits();

        public event Action OnLoadoutChanged;

        private readonly List<TraitDataSO> selectedTraits = new List<TraitDataSO>();

        private static readonly List<WeaponDataSO> emptyWeapons = new List<WeaponDataSO>();
        private static readonly List<TraitDataSO> emptyTraits = new List<TraitDataSO>();
        private static readonly List<TraitEffectDataSO> emptyTraitEffects = new List<TraitEffectDataSO>();

        private void Awake()
        {
            ResolveReferences();
            InitializeLoadout();
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (inventoryManager != null)
            {
                inventoryManager.OnInventoryChanged += OnInventoryChanged;
            }
        }

        private void OnDisable()
        {
            if (inventoryManager != null)
            {
                inventoryManager.OnInventoryChanged -= OnInventoryChanged;
            }
        }

        private void Start()
        {
            if (logOnStart)
            {
                PrintDebugInfo();

                if (logTraitEffectDetail)
                {
                    PrintTraitEffectDebugInfo();
                }
            }
        }

        private void ResolveReferences()
        {
            if (inventoryManager == null)
                inventoryManager = GetComponent<LobbyInventoryManager>();

            if (dataManager == null && inventoryManager != null)
                dataManager = inventoryManager.DataManager;
        }

        private void InitializeLoadout()
        {
            ResolveReferences();

            if (inventoryManager != null)
            {
                inventoryManager.EnsureInitialized();
                dataManager = inventoryManager.DataManager;
            }
            else
            {
                Debug.LogError("[DataDrivenLobbyLoadoutManager] LobbyInventoryManager를 찾지 못했습니다.");
                return;
            }

            if (dataManager == null)
            {
                Debug.LogError("[DataDrivenLobbyLoadoutManager] DataManagerSO를 찾지 못했습니다. LobbyInventoryManager에 DataManagerSO를 연결하세요.");
                return;
            }

            dataManager.Initialize();

            SelectedWeapon = FindDefaultOwnedWeapon();

            selectedTraits.Clear();

            NotifyLoadoutChanged();
        }

        private WeaponDataSO FindDefaultOwnedWeapon()
        {
            if (dataManager == null)
                return null;

            WeaponDataSO defaultWeapon = dataManager.GetDefaultWeapon();

            if (defaultWeapon != null && IsWeaponOwned(defaultWeapon))
                return defaultWeapon;

            IReadOnlyList<WeaponDataSO> ownedWeapons = GetOwnedWeapons();

            if (ownedWeapons.Count > 0)
                return ownedWeapons[0];

            return null;
        }

        private List<WeaponDataSO> GetOwnedWeapons()
        {
            ResolveReferences();

            List<WeaponDataSO> result = new List<WeaponDataSO>();

            if (dataManager == null || dataManager.weaponDataList == null)
                return result;

            foreach (WeaponDataSO weaponData in dataManager.weaponDataList)
            {
                if (weaponData == null || weaponData._data == null)
                    continue;

                if (IsWeaponOwned(weaponData))
                {
                    result.Add(weaponData);
                }
            }

            return result;
        }

        private List<TraitDataSO> GetOwnedTraits()
        {
            ResolveReferences();

            List<TraitDataSO> result = new List<TraitDataSO>();

            if (dataManager == null || dataManager.traitDataList == null)
                return result;

            foreach (TraitDataSO traitData in dataManager.traitDataList)
            {
                if (traitData == null || traitData._data == null)
                    continue;

                if (IsTraitOwned(traitData))
                {
                    result.Add(traitData);
                }
            }

            return result;
        }

        public void SelectWeapon(WeaponDataSO weaponData)
        {
            if (weaponData == null)
                return;

            if (!IsWeaponOwned(weaponData))
            {
                Debug.LogWarning($"[DataDrivenLobbyLoadoutManager] 보유하지 않은 무기는 선택할 수 없습니다: {weaponData.ID}");
                return;
            }

            if (SelectedWeapon == weaponData)
                return;

            SelectedWeapon = weaponData;

            if (UserManager.Instance != null)
            {
                UserManager.Instance.EquipWeapon(weaponData.ID);
            }

            NotifyLoadoutChanged();
        }

        public void SelectTrait(TraitDataSO traitData)
        {
            if (traitData == null)
                return;

            if (!IsTraitOwned(traitData))
            {
                Debug.LogWarning($"[DataDrivenLobbyLoadoutManager] 보유하지 않은 특성은 선택할 수 없습니다: {traitData.ID}");
                return;
            }

            if (selectedTraits.Contains(traitData))
            {
                selectedTraits.Remove(traitData);
                NotifyLoadoutChanged();
                return;
            }

            if (selectedTraits.Count >= maxTraitSlotCount)
            {
                selectedTraits.Clear();
            }

            selectedTraits.Add(traitData);

            if (UserManager.Instance != null)
            {
                UserManager.Instance.EquipTrait(traitData.ID);
            }

            NotifyLoadoutChanged();


        }

        public bool IsWeaponSelected(WeaponDataSO weaponData)
        {
            return SelectedWeapon == weaponData;
        }

        public bool IsTraitSelected(TraitDataSO traitData)
        {
            return selectedTraits.Contains(traitData);
        }

        public bool IsWeaponOwned(WeaponDataSO weaponData)
        {
            ResolveReferences();

            if (weaponData == null)
                return false;

            if (inventoryManager == null)
                return true;

            return inventoryManager.IsWeaponOwned(weaponData);
        }

        public bool IsTraitOwned(TraitDataSO traitData)
        {
            ResolveReferences();

            if (traitData == null)
                return false;

            if (inventoryManager == null)
                return true;

            return inventoryManager.IsTraitOwned(traitData);
        }

        public List<TraitEffectDataSO> GetSelectedTraitEffects()
        {
            if (selectedTraits.Count <= 0)
                return new List<TraitEffectDataSO>();

            TraitDataSO selectedTrait = selectedTraits[0];

            if (selectedTrait == null)
                return new List<TraitEffectDataSO>();

            return GetTraitEffects(selectedTrait);
        }

        public List<TraitEffectDataSO> GetTraitEffects(TraitDataSO traitData)
        {
            if (traitData == null)
                return new List<TraitEffectDataSO>();

            return GetTraitEffectsById(traitData.ID);
        }

        public List<TraitEffectDataSO> GetTraitEffectsById(string traitId)
        {
            ResolveReferences();

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

        private void OnInventoryChanged()
        {
            if (SelectedWeapon == null || !IsWeaponOwned(SelectedWeapon))
            {
                SelectedWeapon = FindDefaultOwnedWeapon();
            }

            for (int i = selectedTraits.Count - 1; i >= 0; i--)
            {
                TraitDataSO traitData = selectedTraits[i];

                if (traitData == null || !IsTraitOwned(traitData))
                {
                    selectedTraits.RemoveAt(i);
                }
            }

            NotifyLoadoutChanged();
        }

        public void PrintDebugInfo()
        {
            Debug.Log("========== [DataDrivenLobbyLoadoutManager] 기본 데이터 테스트 ==========");

            Debug.Log($"전체 무기 개수: {AllWeapons.Count}");
            Debug.Log($"보유 무기 개수: {AvailableWeapons.Count}");
            Debug.Log($"전체 특성 개수: {AllTraits.Count}");
            Debug.Log($"보유 특성 개수: {AvailableTraits.Count}");
            Debug.Log($"전체 특성 효과 개수: {AllTraitEffects.Count}");

            if (SelectedWeapon != null && SelectedWeapon._data != null)
            {
                Debug.Log($"현재 선택 무기: {SelectedWeapon._data._id} / {SelectedWeapon._data._name}");
            }
            else
            {
                Debug.LogWarning("현재 선택 무기 없음");
            }

            if (selectedTraits.Count <= 0)
            {
                Debug.Log("현재 선택 특성: 없음");
            }
            else
            {
                for (int i = 0; i < selectedTraits.Count; i++)
                {
                    TraitDataSO trait = selectedTraits[i];

                    if (trait == null || trait._data == null)
                        continue;

                    Debug.Log($"현재 선택 특성 {i}: {trait._data._id} / {trait._data._name}");
                }
            }

            Debug.Log("===================================================================");
        }

        public void PrintTraitEffectDebugInfo()
        {
            Debug.Log("========== [TraitEffect 연결 상태 확인] ==========");
            Debug.Log($"DataManagerSO traitEffectDataList 개수: {AllTraitEffects.Count}");

            foreach (TraitDataSO traitData in AllTraits)
            {
                if (traitData == null || traitData._data == null)
                    continue;

                List<TraitEffectDataSO> effects = GetTraitEffects(traitData);

                if (effects == null || effects.Count <= 0)
                {
                    Debug.LogWarning(
                        $"[TraitEffect 체크] 특성 '{traitData._data._name}'과 연결된 효과가 없습니다. " +
                        $"TraitData ID: '{traitData.ID}', Normalized ID: '{NormalizeId(traitData.ID)}'"
                    );

                    continue;
                }

                Debug.Log(
                    $"[TraitEffect 체크] 특성 '{traitData._data._name}' " +
                    $"ID: '{traitData.ID}', 연결된 효과 개수: {effects.Count}"
                );
            }

            Debug.Log("=================================================");
        }

        private string NormalizeId(string id)
        {
            if (string.IsNullOrEmpty(id))
                return string.Empty;

            return id.Trim();
        }

        private void NotifyLoadoutChanged()
        {
            OnLoadoutChanged?.Invoke();
        }
    }
}