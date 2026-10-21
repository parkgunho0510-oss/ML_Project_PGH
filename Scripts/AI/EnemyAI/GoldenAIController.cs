using UnityEngine;
using Wooduduk.AI.EnemyAI.State;

namespace Wooduduk.AI.EnemyAI
{
    public class GoldenAIController : BaseAIController
    {
        public FleeState FleeState { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            // 도망 상태
            FleeState = new FleeState(this, StateMachine);
        }

        protected override void OnEnable()
        {
            base.OnEnable(); // Agent 초기화 실행

            if (FleeState != null)
            {
                StateMachine.Initialize(FleeState);
            }
        }
    }
}