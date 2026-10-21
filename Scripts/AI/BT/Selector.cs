using System.Collections.Generic;

namespace Wooduduk.AI.BT
{
    public class Selector : Node
    {
        private readonly List<Node> _nodes;
        public Selector(List<Node> nodes) { this._nodes = nodes; }

        public override NodeState Evaluate()
        {
            foreach (var node in _nodes)
            {
                switch (node.Evaluate())
                {
                    case NodeState.RUNNING: return NodeState.RUNNING;
                    case NodeState.SUCCESS: return NodeState.SUCCESS;
                    case NodeState.FAILURE: continue;
                }
            }
            return NodeState.FAILURE;
        }
    }
}
