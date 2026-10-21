using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TreeDropTableData", menuName = "GameData/TreeDropTableData")]
public class TreeDropTable : ScriptableObject
{
    [Header("고정 드랍 아이템")]
    [SerializeField] private List<DropItemEntry> guaranteedDrops = new List<DropItemEntry>();

    [Header("이 중에서 하나만 랜덤 드랍")]
    [SerializeField] private List<DropItemEntry> randomOneDrops = new List<DropItemEntry>();

    [Header("랜덤 아이템 자체가 나올 확률")]
    [Range(0f, 1f)]
    [SerializeField] private float randomOneDropChance = 1f;

    public List<GameObject> CreateDropPrefabs()
    {
        List<GameObject> result = new List<GameObject>();

        AddGuaranteedDrops(result);
        AddRandomOneDrop(result);

        return result;
    }

    private void AddGuaranteedDrops(List<GameObject> result)
    {
        foreach (DropItemEntry dropEntry in guaranteedDrops)
        {
            if (dropEntry == null)
                continue;

            if (!dropEntry.IsValid())
                continue;

            int dropCount = dropEntry.GetDropCount();

            for (int i = 0; i < dropCount; i++)
            {
                result.Add(dropEntry.ItemPrefab);
            }
        }
    }

    private void AddRandomOneDrop(List<GameObject> result)
    {
        if (randomOneDrops == null || randomOneDrops.Count == 0)
            return;

        if (UnityEngine.Random.value > randomOneDropChance)
            return;

        DropItemEntry selectedDrop = GetRandomOneDrop();

        if (selectedDrop == null)
            return;

        int dropCount = selectedDrop.GetDropCount();

        for (int i = 0; i < dropCount; i++)
        {
            result.Add(selectedDrop.ItemPrefab);
        }
    }

    private DropItemEntry GetRandomOneDrop()
    {
        float totalWeight = 0f;

        foreach (DropItemEntry dropEntry in randomOneDrops)
        {
            if (dropEntry == null)
                continue;

            if (!dropEntry.IsWeightedValid())
                continue;

            totalWeight += dropEntry.Weight;
        }

        if (totalWeight <= 0f)
            return null;

        float randomValue = UnityEngine.Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (DropItemEntry dropEntry in randomOneDrops)
        {
            if (dropEntry == null)
                continue;

            if (!dropEntry.IsWeightedValid())
                continue;

            currentWeight += dropEntry.Weight;

            if (randomValue <= currentWeight)
                return dropEntry;
        }

        return null;
    }
}
