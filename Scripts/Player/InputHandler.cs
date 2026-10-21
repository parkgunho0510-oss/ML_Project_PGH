using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Wooduduk.Core;

namespace Wooduduk.Player
{
    // Player 입력 처리
    // PC 키마우스 -> IInputProvider 인터페이스로 추상화
    // TODO : 모바일 가상 조이스틱 => Touch 컨트롤 바인딩 추가
    public class InputHandler : MonoBehaviour
    {

        private void OnMove(InputValue value)
        {
            var v = value.Get<Vector2>();
            PlayerEvents.RaiseMove(v);
        }

        // Attack 액션 (Button)
        // isPressed: true = 눌림(Press), false = 뗌(Release)
        private void OnAttack(InputValue value)
        {
            if (value.isPressed) PlayerEvents.RaiseAttackPressed();
            else PlayerEvents.RaiseAttackReleased();
        }
    }
}
