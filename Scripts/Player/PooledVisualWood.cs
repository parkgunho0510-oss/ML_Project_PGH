using UnityEngine;
using Wooduduk.Core;

namespace Wooduduk.Player
{
    public class PooledVisualWood : MonoBehaviour, IPoolable
    {
        private System.Action<GameObject> _returnAction;

        public void SetupPoolReturn(System.Action<GameObject> returnAction) => _returnAction = returnAction;
        public void OnSpawn() { }
        public void OnReturn() { }

        public void ReturnToPool()
        {
            _returnAction?.Invoke(gameObject);
        }
    }
}