using UnityEngine;

namespace Wooduduk.AI.EnemyAI.State
{
    public class AIStateMachine
    {
        public AIState CurrentState { get; private set; }

        public void Initialize(AIState startState)
        {
            CurrentState = startState;
            CurrentState.Enter();
        }
        public void ChangeState(AIState newState)
        {
            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState?.Enter();
        }
    }
}
