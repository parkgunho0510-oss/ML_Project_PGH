using System;
using UnityEngine;
using Wooduduk.Core;
using Wooduduk.Data.Runtime;

namespace Wooduduk.Player
{
    public class WoodBag : MonoBehaviour, IWoodReceiver
    {
        // 서버 확정 : UI는 이 값/이벤트만 본다.
        public int CurrentWood { get; private set; }
        public event Action<int> OnWoodChanged;

        // 온라인 / NetworkPlayer SyncVar OnChange → 호출 (유일한 갱신 경로)
        public void SetDisplayWood(int serverConfirmed)
        {
            if (CurrentWood == serverConfirmed) return;
            CurrentWood = serverConfirmed;
            OnWoodChanged?.Invoke(CurrentWood);
        }

        // 오프라인 테스트 / NetworkPlayer 없는 로컬 씬에서만 PickupSensor가 호출
        public void OnWoodCollected(int amount, GameObject who = null)
            => SetDisplayWood(CurrentWood + amount);
    }
}
