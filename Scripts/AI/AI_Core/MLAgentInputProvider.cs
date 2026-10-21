using UnityEngine;
using Wooduduk.Core;

namespace Wooduduk.AI_Core
{
    public class MLAgentInputProvider : MonoBehaviour, IInputProvider
    {
        public Vector2 MoveAxis { get; private set;  }

        public bool AttackHeld { get; private set; }

        //Ml Agent OnActionReceived 에서 호출
        public void SetAction(Vector2 move, bool attack)
        {
            MoveAxis = move;
            AttackHeld = attack;
        }
    }
}
