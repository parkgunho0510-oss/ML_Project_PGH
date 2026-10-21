using Firebase.Database;
using Firebase.Extensions;
using System;
using UnityEngine;

namespace Wooduduk.Network.Firebase
{
    public class UserRepository
    {
        private DatabaseReference _db;

        public UserRepository()
        {
            _db = FirebaseDatabase.DefaultInstance.RootReference;
        }

        // 유저 데이터 가져오기
        public void GetUserData(string userId, Action<DataSnapshot> callback)
        {
            _db.Child("Users").Child(userId).GetValueAsync().ContinueWithOnMainThread(task =>
                {
                    if (task.IsFaulted || task.IsCanceled)
                    {
                        Debug.LogError("유저 로드 실패");
                        callback?.Invoke(null);
                        return;
                    }

                    callback?.Invoke(task.Result);
                });
        }

        // 유저 데이터 저장
        public void SaveUserData(string userId, string json)
        {
            _db.Child("Users").Child(userId).SetRawJsonValueAsync(json);
        }
    }
}
