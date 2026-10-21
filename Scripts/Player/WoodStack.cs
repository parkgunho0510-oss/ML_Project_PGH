using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Wooduduk.Core;

namespace Wooduduk.Player
{
    public class WoodStack : MonoBehaviour, IWoodReceiver
    {
        [Header("Stack")]
        [SerializeField] private GameObject _woodPrefab;
        [SerializeField] private Transform _bagAnchor;
        [SerializeField] private int _woodPerStack = 3; // n개 먹을 때마가 1개 스택
        [SerializeField] private Vector3 _stackOffset = new Vector3(0f, 0.25f, 0f);

        private int _pending;
        private int _stackCount;

        private List<GameObject> _visualStacks = new List<GameObject>();
        public void OnWoodCollected(int amount, GameObject who = null)
        {
            if (who != null && !transform.IsChildOf(who.transform)) return;
            //수집자 아니면 무시

            _pending += amount;
            int per = Mathf.Max(1, _woodPerStack);
            while (_pending >= per)
            {
                _pending -= per;
                SpawnOne();
            }

        }
        private void SpawnOne()
        {
            if (_woodPrefab == null || _bagAnchor == null) return;

            var go = GenericObjectPool.Instance.Spawn(_woodPrefab, _bagAnchor.position, Quaternion.identity);

            go.transform.SetParent(_bagAnchor);
            go.transform.localPosition = _stackOffset * _stackCount;
            go.transform.localRotation = Quaternion.identity;

            _visualStacks.Add(go);
            _stackCount++;
        }

        public void ClearStack()
        {
            _pending = 0;
            _stackCount = 0;

            foreach (var wood in _visualStacks)
            {
                if (wood != null)
                {
                    if (wood.TryGetComponent<PooledVisualWood>(out var pooledComponent))
                    {
                        pooledComponent.ReturnToPool();
                    }
                    else
                    {
                        wood.SetActive(false);
                    }
                }
            }
            _visualStacks.Clear();
        }
    }
}
