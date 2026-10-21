using System;
using System.Collections.Generic;
using UnityEngine;
using Wooduduk.Data.DataSystem;

using DataManagerSO = Wooduduk.Data.DataSO.DataManagerSO;
using WeaponDataSO = Wooduduk.Data.DataSO.WeaponDataSO;
using TraitDataSO = Wooduduk.Data.DataSO.TraitDataSO;
using TraitEffectDataSO = Wooduduk.Data.DataSO.TraitEffectDataSO;

namespace Wooduduk.Lobby
{
    /// <summary>
    /// 로비에서 사용하는 임시 인벤토리 / 재화 매니저입니다.
    ///
    /// 이 스크립트만 DataManagerSO 에셋을 Inspector에서 직접 연결합니다.
    /// 나머지 로비 컨트롤러들은 이 매니저를 통해 데이터에 접근합니다.
    /// </summary>
    public class LobbyInventoryManager : MonoBehaviour
    {
        [Header("Data Manager")]
        [Tooltip("팀 데이터 담당자가 만든 DataManagerSO 에셋입니다. 이 필드만 직접 연결하면 됩니다.")]
        [SerializeField] private DataManagerSO dataManager;

        [Header("임시 재화")]
        [SerializeField] private int wood = 9999;

        public DataManagerSO DataManager => dataManager;
        public int Wood => wood;

        public event Action OnInventoryChanged;

        private readonly HashSet<string> ownedWeaponIds = new HashSet<string>();
        private readonly HashSet<string> ownedTraitIds = new HashSet<string>();

        private bool initialized;

        private void Awake()
        {
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            if (initialized)
                return;

            initialized = true;

            if (dataManager == null)
            {
                Debug.LogError("[LobbyInventoryManager] DataManagerSO가 연결되지 않았습니다.");
                return;
            }

            dataManager.Initialize();

            RegisterDefaultWeapons();
        }

        private void RegisterDefaultWeapons()
        {
            ownedWeaponIds.Clear();

            if (dataManager == null || dataManager.weaponDataList == null)
                return;

            foreach (WeaponDataSO weaponData in dataManager.weaponDataList)
            {
                if (weaponData == null || weaponData._data == null)
                    continue;

                if (weaponData._data._isDefault)
                {
                    ownedWeaponIds.Add(weaponData.ID);
                }
            }
        }

        public bool IsWeaponOwned(WeaponDataSO weaponData)
        {
            if (weaponData == null)
                return false;

            if (weaponData._data != null && weaponData._data._isDefault)
                return true;

            return ownedWeaponIds.Contains(weaponData.ID);
        }

        public bool IsTraitOwned(TraitDataSO traitData)
        {
            if (traitData == null)
                return false;

            return ownedTraitIds.Contains(traitData.ID);
        }

        public int GetWeaponCost(WeaponDataSO weaponData)
        {
            if (weaponData == null || weaponData._data == null)
                return 0;

            return Mathf.Max(0, weaponData._data._unlockCost);
        }

        public int GetTraitCost(TraitDataSO traitData)
        {
            if (traitData == null)
                return 0;

            List<TraitEffectDataSO> effects = GetTraitEffectsById(traitData.ID);

            if (effects == null || effects.Count <= 0)
                return 0;

            TraitEffectDataSO levelOneEffect = FindLevelOneEffect(effects);

            if (levelOneEffect == null || levelOneEffect._data == null)
                return 0;

            return Mathf.Max(0, levelOneEffect._data._unlockCost);
        }

        public bool TryBuyWeapon(WeaponDataSO weaponData)
        {
            EnsureInitialized();

            if (weaponData == null || weaponData._data == null)
                return false;

            if (IsWeaponOwned(weaponData))
            {
                Debug.Log($"[LobbyInventoryManager] 이미 보유한 무기입니다: {weaponData._data._name}");
                return false;
            }

            int cost = GetWeaponCost(weaponData);

            if (wood < cost)
            {
                Debug.LogWarning($"[LobbyInventoryManager] 장작이 부족합니다. 필요: {cost}, 보유: {wood}");
                return false;
            }

            wood -= cost;
            ownedWeaponIds.Add(weaponData.ID);

            if (UserManager.Instance != null)
            {
                UserManager.Instance.AddWeapon(weaponData.ID);
            }
            else
            {
                Debug.LogWarning("[LobbyInventoryManager] UserManager.Instance가 없어 무기 구매 정보를 저장하지 못했습니다.");
            }

            Debug.Log($"[LobbyInventoryManager] 무기 구매 완료: {weaponData._data._id} / {weaponData._data._name}");

            NotifyInventoryChanged();

            return true;
        }

        public bool TryBuyTrait(TraitDataSO traitData)
        {
            EnsureInitialized();

            if (traitData == null || traitData._data == null)
                return false;

            if (IsTraitOwned(traitData))
            {
                Debug.Log($"[LobbyInventoryManager] 이미 보유한 특성입니다: {traitData._data._name}");
                return false;
            }

            int cost = GetTraitCost(traitData);

            if (wood < cost)
            {
                Debug.LogWarning($"[LobbyInventoryManager] 장작이 부족합니다. 필요: {cost}, 보유: {wood}");
                return false;
            }

            wood -= cost;
            ownedTraitIds.Add(traitData.ID);

            if (UserManager.Instance != null)
            {
                UserManager.Instance.AddTrait(traitData.ID);
            }
            else
            {
                Debug.LogWarning("[LobbyInventoryManager] UserManager.Instance가 없어 특성 구매 정보를 저장하지 못했습니다.");
            }

            Debug.Log($"[LobbyInventoryManager] 특성 구매 완료: {traitData._data._id} / {traitData._data._name}");

            NotifyInventoryChanged();

            return true;
        }

        public void AddWood(int amount)
        {
            if (amount <= 0)
                return;

            wood += amount;

            NotifyInventoryChanged();
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

        private TraitEffectDataSO FindLevelOneEffect(List<TraitEffectDataSO> effects)
        {
            for (int i = 0; i < effects.Count; i++)
            {
                TraitEffectDataSO effect = effects[i];

                if (effect != null && effect._data != null && effect._data._level == 1)
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

        private void NotifyInventoryChanged()
        {
            OnInventoryChanged?.Invoke();
        }
    }
}