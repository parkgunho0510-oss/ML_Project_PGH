using UnityEngine;

namespace Wooduduk.Core
{
    public interface IInteractable
    {
        float HoldDuration { get; } // 홀드 완료 시 까지
        bool CanInteract(GameObject interactor); // 가능 조건
        void OnInteract(GameObject interactor); // 완료 시 실행
        void SetHighlight(bool on); // 근접 시 아웃라인
    }
}
