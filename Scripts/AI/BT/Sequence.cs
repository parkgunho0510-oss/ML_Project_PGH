using System.Collections.Generic;

namespace Wooduduk.AI.BT
{
    public class Sequence : Node
    {
        private readonly List<Node> _nodes;
        public Sequence(List<Node> nodes) { this._nodes = nodes; }

        public override NodeState Evaluate()
        {
            bool anyChildIsRunning = false;
            foreach (var node in _nodes)
            {
                switch (node.Evaluate())
                {
                    case NodeState.FAILURE: return NodeState.FAILURE;
                    case NodeState.SUCCESS: continue;
                    case NodeState.RUNNING:
                        anyChildIsRunning = true;
                        return NodeState.RUNNING;
                }
            }
            return anyChildIsRunning ? NodeState.RUNNING : NodeState.SUCCESS;
        }
    }
}
