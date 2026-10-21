namespace Wooduduk.AI.EnemyAI.State
{
    public abstract class AIState
    {
        protected BaseAIController _controller;
        protected AIStateMachine _stateMachine;

        public AIState(BaseAIController controller, AIStateMachine stateMachine)
        {
            _controller = controller;
            _stateMachine = stateMachine;
        }
        public virtual void Enter() { }
        public virtual void LogicUpdate() { }
        public virtual void Exit() { }
    }
}