using UnityEngine;
using Wooduduk.Core;

namespace Wooduduk.Item
{
    public class DroppedWeapon : MonoBehaviour, IInteractable
    {
        [SerializeField] private float _holdDuration = 0.4f;
        [SerializeField] private string _weaponId;  // 드랍 시 주입할 무기

        public float HoldDuration => _holdDuration;

        public bool CanInteract(GameObject interactor) => true;
        

        public void OnInteract(GameObject interactor)
        {
            // TODO : interactor에 _weaponId 장착
            Debug.Log($"{interactor.name}가 {_weaponId} 획득했다!");
            Destroy(gameObject); // TODO : 풀매니저 연동
        }

        public void SetHighlight(bool on)
        {
            // TODO : 아웃라인 토글
        }
    }
}