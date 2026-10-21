using Wooduduk.Core;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;

namespace Wooduduk.Network.Firebase
{
    public class FirebaseManager : MonoBehaviour
    {
        public static FirebaseManager Instance { get; private set; }

        private FirebaseAuth _auth;

        public string UserId => _auth?.CurrentUser?.UserId;

        private void Awake()
        {
            if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
            else { Destroy(gameObject); return; }
        }

        private void Start()
        {
            InitializeFirebase();
        }

        private void InitializeFirebase()
        {
            FirebaseApp.CheckAndFixDependenciesAsync()
                .ContinueWithOnMainThread(task =>
                {
                    DependencyStatus status = task.Result;

                    if (status == DependencyStatus.Available)
                    {
                        Debug.Log("Firebase 초기화 성공");

                        _auth = FirebaseAuth.DefaultInstance;

                        LoginAnonymously();
                    }
                    else
                    {
                        Debug.LogError($"Firebase 초기화 실패 : {status}");
                    }
                });
        }

        private void LoginAnonymously()
        {
            // 이미 로그인된 상태인지 확인
            if (_auth.CurrentUser != null)
            {
                Debug.Log($"이미 로그인되어 있습니다: {_auth.CurrentUser.UserId}");
                ServerEvents.RaiseLoginSucceeded(_auth.CurrentUser.UserId, "");
                return;
            }

            _auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("로그인 취소됨");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError($"로그인 실패 : {task.Exception}");
                    return;
                }
                string uid = _auth.CurrentUser.UserId;
                Debug.Log($"로그인 성공 : {uid}");

                ServerEvents.RaiseLoginSucceeded(uid,"");  
            });
        }
    }
}