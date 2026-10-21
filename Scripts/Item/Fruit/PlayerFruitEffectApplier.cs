using System.Collections.Generic;
using UnityEngine;


/// 핵심 기능:
/// 1. 열매를 먹었을 때 슬롯에 장착합니다.
/// 2. 최대 슬롯 개수를 초과하면 가장 나중에 먹은 열매를 새 열매로 교체합니다.
/// 3. 슬롯에 들어간 열매의 효과를 적용합니다.
/// 4. 슬롯에서 빠지는 열매의 효과를 제거합니다.
/// 5. 장작처럼 슬롯을 차지하지 않는 아이템은 즉시 효과만 적용합니다.


public class PlayerFruitSlotController : MonoBehaviour
{

    /// 현재 플레이어에게 적용 중인 열매 1개의 정보를 담는 내부 클래스입니다.
    
    private class ActiveFruitSlot
    {
        public FruitDataSO FruitData { get; }
        public object Source { get; }

        public ActiveFruitSlot(FruitDataSO fruitData)
        {
            FruitData = fruitData;

            // 각 열매 슬롯마다 고유한 Source를 만들어줍니다.
            // 나중에 열매가 교체될 때, 이 Source를 기준으로 해당 열매 효과만 제거합니다.
            Source = new object();
        }
    }

    [Header("열매 슬롯 최대 개수")]
    [SerializeField] private int maxFruitSlotCount = 2;

    // 현재 플레이어가 보유 중인 열매 슬롯 목록입니다.
    // 예: [불씨, 나뭇잎] 상태에서 수액을 먹으면 [불씨, 수액]으로 변경됩니다.
    private readonly List<ActiveFruitSlot> activeFruitSlots = new List<ActiveFruitSlot>();

    /// 열매를 획득했을 때 외부에서 호출하는 메서드입니다.
    public void CollectFruit(FruitDataSO fruitData)
    {
        if (fruitData == null)
            return;

        // 장작처럼 슬롯을 차지하지 않는 아이템은 즉시 효과만 적용합니다.
        if (!fruitData.OccupiesFruitSlot)
        {
            ApplyOneShotFruit(fruitData);
            return;
        }

        // 슬롯에 여유가 있으면 새 열매를 추가합니다.
        if (activeFruitSlots.Count < maxFruitSlotCount)
        {
            AddFruitToSlot(fruitData);
            return;
        }

        // 슬롯이 가득 찼으면 가장 나중에 먹은 열매와 교체합니다.
        ReplaceLastFruit(fruitData);
    }

    /// 새 열매를 빈 슬롯에 추가하고 효과를 적용합니다.
    private void AddFruitToSlot(FruitDataSO fruitData)
    {
        ActiveFruitSlot newSlot = new ActiveFruitSlot(fruitData);
        activeFruitSlots.Add(newSlot);

        ApplySlotEffect(newSlot);

        Debug.Log($"열매 장착: {fruitData.FruitName}");
    }

    /// 가장 나중에 먹은 열매를 새 열매로 교체합니다.
    private void ReplaceLastFruit(FruitDataSO newFruitData)
    {
        int replaceIndex = activeFruitSlots.Count - 1;

        ActiveFruitSlot oldSlot = activeFruitSlots[replaceIndex];
        RemoveSlotEffect(oldSlot);

        ActiveFruitSlot newSlot = new ActiveFruitSlot(newFruitData);
        activeFruitSlots[replaceIndex] = newSlot;

        ApplySlotEffect(newSlot);

        Debug.Log($"열매 교체: {oldSlot.FruitData.FruitName} → {newFruitData.FruitName}");
    }

    /// 슬롯에 들어온 열매의 효과를 적용합니다.
    private void ApplySlotEffect(ActiveFruitSlot slot)
    {
        FruitEffectContext context = new FruitEffectContext(
            gameObject,
            slot.FruitData,
            slot.Source
        );

        slot.FruitData.ApplyEffects(context);
    }

    /// 슬롯에서 빠지는 열매의 효과를 제거합니다.
    private void RemoveSlotEffect(ActiveFruitSlot slot)
    {
        FruitEffectContext context = new FruitEffectContext(
            gameObject,
            slot.FruitData,
            slot.Source
        );

        slot.FruitData.RemoveEffects(context);
    }

    /// 슬롯을 차지하지 않는 열매나 아이템의 효과를 즉시 적용합니다.
    private void ApplyOneShotFruit(FruitDataSO fruitData)
    {
        object source = new object();

        FruitEffectContext context = new FruitEffectContext(
            gameObject,
            fruitData,
            source
        );

        fruitData.ApplyEffects(context);

        Debug.Log($"즉시 획득 열매 적용: {fruitData.FruitName}");
    }
}