using UnityEngine;
using Wooduduk.Core;
using Wooduduk.Data;

namespace Wooduduk.Network.Firebase
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private UserRepository _repository;
        private UserService _service;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                _repository = new UserRepository();
                _service = new UserService();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            ServerEvents.OnLoginSucceeded += LoadUserData;
            ServerEvents.OnLogout += HandleLogout;
        }

        private void OnDisable()
        {
            ServerEvents.OnLoginSucceeded -= LoadUserData;
            ServerEvents.OnLogout -= HandleLogout;
        }

        // 로드 흐름
        public void LoadUserData(string userId, string nickname)
        {
            _repository.GetUserData(userId, snapshot =>
            {
                UserData userData;

                if (snapshot == null || !snapshot.Exists)
                {
                    userData = _service.CreateUserData(userId);
                    SaveUserData(userData);
                }
                else
                {
                    userData = JsonUtility.FromJson<UserData>(snapshot.GetRawJsonValue());
                }

                ServerEvents.RaiseUserDataLoaded(userData);
            });
        }

        // 저장 흐름
        public void SaveUserData(UserData userData)
        {
            if (userData == null) return;

            string json = JsonUtility.ToJson(userData);

            _repository.SaveUserData(userData._userId, json);
        }

        // 로그아웃 저장
        private void HandleLogout(UserData user)
        {
            SaveUserData(user);
        }
    }
}
