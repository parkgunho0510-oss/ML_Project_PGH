using UnityEngine;
using Wooduduk.Core;

public class FruitEffectContext
{
    public GameObject Collector { get; }
    public FruitDataSO FruitData { get; }
    public object Source { get; }

    public IPlayerStats PlayerStats { get; }

    public FruitEffectContext(GameObject collector, FruitDataSO fruitData, object source)
    {
        Collector = collector;
        FruitData = fruitData;
        Source = source;

        if (collector != null)
            PlayerStats = collector.GetComponent<IPlayerStats>();
    }
}