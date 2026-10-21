using UnityEngine;
using Wooduduk.Core;

[CreateAssetMenu(menuName = "Fruit/Effects/Stat Modifier")]
public class StatModifierFruitEffectSO : FruitEffectSO
{
    [Header("적용할 스탯")]
    [SerializeField] private StatId statId;

    [Header("퍼센트 증가량")]
    [SerializeField] private float percent;

    public override void Apply(FruitEffectContext context)
    {
        if (context == null)
            return;

        if (context.PlayerStats == null)
            return;

        StatModifier modifier = new StatModifier
        {
            percent = percent,
            source = context.Source
        };

        context.PlayerStats.AddModifier(statId, modifier);
    }

    public override void Remove(FruitEffectContext context)
    {
        if (context == null)
            return;

        if (context.PlayerStats == null)
            return;

        StatModifier modifier = new StatModifier
        {
            percent = percent,
            source = context.Source
        };

        context.PlayerStats.RemoveModifier(statId, modifier);
    }
}