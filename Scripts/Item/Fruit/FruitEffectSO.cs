using UnityEngine;

public abstract class FruitEffectSO : ScriptableObject
{
    public abstract void Apply(FruitEffectContext context);
    public abstract void Remove(FruitEffectContext context);
}