using System;
using UnityEngine;
using Wooduduk.Core;

public class DroppedFruit : MonoBehaviour, IFruit, IPoolable
{
    [SerializeField] private FruitDataSO fruitData;
    private bool _isCollected = false;
    public FruitAttribute Attribute
    {
        get
        {
            if (fruitData == null)
                return FruitAttribute.LOG;

            return fruitData.Attribute;
        }
    }
    private Action<GameObject> _returnToPool;

    #region IPoolable
    public void SetupPoolReturn(Action<GameObject> returnAction) => _returnToPool = returnAction;
    public void OnSpawn() {

        _isCollected = false;
    }
    public void OnReturn() { }
    #endregion
    public void OnCollected(GameObject collector)
    {
        if (_isCollected) return;
        _isCollected = true;

        if (fruitData == null)
        {
            Destroy(gameObject);
            return;
        }

        //PlayerFruitSlotController fruitSlotController =
        //    collector.GetComponent<PlayerFruitSlotController>();

        //if (fruitSlotController == null)
        //    return;

        //fruitSlotController.CollectFruit(fruitData);

        if (_returnToPool != null) _returnToPool.Invoke(gameObject);
        else gameObject.SetActive(false);
    }
}