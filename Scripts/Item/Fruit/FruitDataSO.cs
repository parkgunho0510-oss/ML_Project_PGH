using System.Collections.Generic;
using UnityEngine;
using Wooduduk.Core;

[CreateAssetMenu(menuName = "Fruit/Fruit Data")]
public class FruitDataSO : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] private string fruitName;
    [SerializeField] private FruitAttribute attribute;
    [SerializeField] private Sprite icon;

    [Header("열매 슬롯 사용 여부")]
    [SerializeField] private bool occupiesFruitSlot = true;

    [Header("효과 목록")]
    [SerializeField] private List<FruitEffectSO> effects = new List<FruitEffectSO>();

    public string FruitName => fruitName;
    public FruitAttribute Attribute => attribute;
    public Sprite Icon => icon;
    public bool OccupiesFruitSlot => occupiesFruitSlot;

    public void ApplyEffects(FruitEffectContext context)
    {
        foreach (FruitEffectSO effect in effects)
        {
            if (effect == null)
                continue;

            effect.Apply(context);
        }
    }

    public void RemoveEffects(FruitEffectContext context)
    {
        foreach (FruitEffectSO effect in effects)
        {
            if (effect == null)
                continue;

            effect.Remove(context);
        }
    }
}