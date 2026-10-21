using System;
using UnityEngine;


public class TreeHealth : MonoBehaviour
{
    [SerializeField] private int maxHp = 5;

    private int currentHp;
    private Action<GameObject> _returnToPool;
    public bool IsAlive => currentHp > 0;

    public event Action OnDead;

    private void Awake()
    {
        currentHp = maxHp;
    }

    public void ReduceHp(int damage)
    {
        if (!IsAlive)
            return;

        currentHp -= damage;

        if (currentHp <= 0)
        {
            currentHp = 0;
            OnDead?.Invoke();

            if (_returnToPool != null)
            {
                _returnToPool.Invoke(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }

        }
    }

    #region IPoolable
    public void SetupPoolReturn(Action<GameObject> returnAction)
    {
        _returnToPool = returnAction;
    }

    public void OnSpawn()
    {
        currentHp = maxHp;
    }

    public void OnReturn()
    {
    }
    #endregion
}
