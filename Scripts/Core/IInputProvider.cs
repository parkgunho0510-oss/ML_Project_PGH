
using UnityEngine;

namespace Wooduduk.Core
{
    public interface IInputProvider
    {
        Vector2 MoveAxis { get; }
        bool AttackHeld { get; }
    }
}
