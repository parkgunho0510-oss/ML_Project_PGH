using System.Collections.Generic;
using UnityEngine;
using Wooduduk.Core;
using Wooduduk.Network.Firebase;

namespace Wooduduk.Data.DataSystem
{
    public class UserManager : MonoBehaviour
    {
        public static UserManager Instance { get; private set; }

        public UserData CurrentUserData { get; private set; }

        private void Awake()
        {
            if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
            else { Destroy(gameObject); return; }
        }

        private void OnEnable()
        {
            // 로그인 후 데이터 로드 완료 이벤트
            ServerEvents.OnUserDataLoaded += SetUserData;

            // 로그아웃 시 상태 초기화
            ServerEvents.OnLogout += ClearCurrentUser;
        }

        private void OnDisable()
        {
            ServerEvents.OnUserDataLoaded -= SetUserData;
            ServerEvents.OnLogout -= ClearCurrentUser;
        }

        /// <summary>
        /// 서버/DB에서 로드된 유저 데이터를 적용
        /// </summary>
        private void SetUserData(UserData userData)
        {
            if (userData == null)
            {
                Debug.LogWarning("[UserManager] 받은 UserData가 null입니다.");
                return;
            }
            CurrentUserData = userData;
            Debug.Log($"[UserManager] 유저 데이터 적용: {userData._userId}");
        }

        /// <summary>
        /// 로그아웃 시 현재 유저 상태 초기화
        /// </summary>
        private void ClearCurrentUser(UserData user)
        {
            if (CurrentUserData == null) return;

            Debug.Log($"[UserManager] 유저 데이터 초기화: {CurrentUserData._userId}");
            CurrentUserData = null;
        }

        /// <summary>
        /// 안전 접근용 (다른 시스템에서 null 체크 줄이기)
        /// </summary>
        public bool TryGetUser(out UserData user)
        {
            user = CurrentUserData;
            return user != null;
        }

        /// <summary>
        /// 현재 유저 데이터에 무기를 추가합니다.
        /// 추가 성공 시 SaveManager를 통해 저장합니다.
        /// </summary>
        public bool AddWeapon(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId))
            {
                Debug.LogWarning("[UserManager] 추가할 weaponId가 비어 있습니다.");
                return false;
            }

            if (!HasCurrentUser())
                return false;

            EnsureUserDataLists();

            if (CurrentUserData._ownedWeaponIds.Contains(weaponId))
            {
                Debug.Log($"[UserManager] 이미 보유 중인 무기입니다: {weaponId}");
                return false;
            }

            CurrentUserData._ownedWeaponIds.Add(weaponId);

            Debug.Log($"[UserManager] 무기 추가 완료: {weaponId}");

            SaveCurrentUserData();

            return true;
        }

        /// <summary>
        /// 현재 유저 데이터에 특성을 추가합니다.
        ///
        /// UserData에는 보유 특성 ID 리스트가 따로 없고,
        /// _traitLevels 리스트로 특성 해금/레벨 상태를 관리합니다.
        /// level 0 = 미해금
        /// level 1 이상 = 해금
        /// </summary>
        public bool AddTrait(string traitId)
        {
            if (string.IsNullOrEmpty(traitId))
            {
                Debug.LogWarning("[UserManager] 추가할 traitId가 비어 있습니다.");
                return false;
            }

            if (!HasCurrentUser())
                return false;

            EnsureUserDataLists();

            TraitProgress progress = FindTraitProgress(traitId);

            if (progress == null)
            {
                progress = new TraitProgress
                {
                    _traitId = traitId,
                    _level = 1
                };

                CurrentUserData._traitLevels.Add(progress);

                Debug.Log($"[UserManager] 특성 추가 완료: {traitId} / Lv.1");

                SaveCurrentUserData();

                return true;
            }

            if (progress._level > 0)
            {
                Debug.Log($"[UserManager] 이미 해금된 특성입니다: {traitId} / Lv.{progress._level}");
                return false;
            }

            progress._level = 1;

            Debug.Log($"[UserManager] 특성 해금 완료: {traitId} / Lv.1");

            SaveCurrentUserData();

            return true;
        }

        /// <summary>
        /// 현재 유저가 해당 무기를 보유 중인지 확인합니다.
        /// </summary>
        public bool IsWeaponOwned(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId))
                return false;

            if (CurrentUserData == null || CurrentUserData._ownedWeaponIds == null)
                return false;

            return CurrentUserData._ownedWeaponIds.Contains(weaponId);
        }

        /// <summary>
        /// 현재 유저가 해당 특성을 해금했는지 확인합니다.
        /// _traitLevels에 존재하고 level이 1 이상이면 해금된 상태로 봅니다.
        /// </summary>
        public bool IsTraitUnlocked(string traitId)
        {
            TraitProgress progress = FindTraitProgress(traitId);

            return progress != null && progress._level > 0;
        }

        /// <summary>
        /// 현재 유저의 특성 레벨을 가져옵니다.
        /// 없거나 미해금이면 0을 반환합니다.
        /// </summary>
        public int GetTraitLevel(string traitId)
        {
            TraitProgress progress = FindTraitProgress(traitId);

            if (progress == null)
                return 0;

            return Mathf.Max(0, progress._level);
        }

        /// <summary>
        /// 현재 장착 무기를 변경합니다.
        /// 장착 상태도 UserData에 저장합니다.
        /// </summary>
        public bool EquipWeapon(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId))
            {
                Debug.LogWarning("[UserManager] 장착할 weaponId가 비어 있습니다.");
                return false;
            }

            if (!HasCurrentUser())
                return false;

            EnsureUserDataLists();

            if (!CurrentUserData._ownedWeaponIds.Contains(weaponId))
            {
                Debug.LogWarning($"[UserManager] 보유하지 않은 무기는 장착할 수 없습니다: {weaponId}");
                return false;
            }

            CurrentUserData._equippedWeaponId = weaponId;

            Debug.Log($"[UserManager] 장착 무기 변경: {weaponId}");

            SaveCurrentUserData();

            return true;
        }

        /// <summary>
        /// 현재 장착 특성을 변경합니다.
        /// 장착 상태도 UserData에 저장합니다.
        /// 현재 UserData 구조상 특성 슬롯은 1개입니다.
        /// </summary>
        public bool EquipTrait(string traitId)
        {
            if (string.IsNullOrEmpty(traitId))
            {
                Debug.LogWarning("[UserManager] 장착할 traitId가 비어 있습니다.");
                return false;
            }

            if (!HasCurrentUser())
                return false;

            if (!IsTraitUnlocked(traitId))
            {
                Debug.LogWarning($"[UserManager] 해금되지 않은 특성은 장착할 수 없습니다: {traitId}");
                return false;
            }

            CurrentUserData._equippedTraitId = traitId;

            Debug.Log($"[UserManager] 장착 특성 변경: {traitId}");

            SaveCurrentUserData();

            return true;
        }

        /// <summary>
        /// 현재 유저 데이터를 저장합니다.
        /// SaveManager.Instance.SaveUserData(CurrentUserData)를 호출합니다.
        /// </summary>
        private void SaveCurrentUserData()
        {
            if (CurrentUserData == null)
            {
                Debug.LogWarning("[UserManager] 저장할 CurrentUserData가 없습니다.");
                return;
            }

            if (SaveManager.Instance == null)
            {
                Debug.LogError("[UserManager] SaveManager.Instance가 없습니다. 씬에 SaveManager가 있는지 확인하세요.");
                return;
            }

            SaveManager.Instance.SaveUserData(CurrentUserData);

            Debug.Log("[UserManager] 유저 데이터 저장 요청 완료");
        }

        private bool HasCurrentUser()
        {
            if (CurrentUserData != null)
                return true;

            Debug.LogWarning("[UserManager] CurrentUserData가 없습니다.");
            return false;
        }

        private void EnsureUserDataLists()
        {
            if (CurrentUserData == null)
                return;

            if (CurrentUserData._ownedSkinIds == null)
                CurrentUserData._ownedSkinIds = new List<string>();

            if (CurrentUserData._traitLevels == null)
                CurrentUserData._traitLevels = new List<TraitProgress>();

            if (CurrentUserData._ownedWeaponIds == null)
                CurrentUserData._ownedWeaponIds = new List<string>();
        }

        private TraitProgress FindTraitProgress(string traitId)
        {
            if (string.IsNullOrEmpty(traitId))
                return null;

            if (CurrentUserData == null || CurrentUserData._traitLevels == null)
                return null;

            for (int i = 0; i < CurrentUserData._traitLevels.Count; i++)
            {
                TraitProgress progress = CurrentUserData._traitLevels[i];

                if (progress == null)
                    continue;

                if (progress._traitId == traitId)
                    return progress;
            }

            return null;
        }

        private void OnApplicationQuit()
        {
            SaveCurrentUserData();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
                SaveCurrentUserData();
        }

        private void OnApplicationFocus(bool focus)
        {
            if (!focus)
                SaveCurrentUserData();
        }
    }

}