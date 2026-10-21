using System;
using UnityEngine;

[Serializable]
public class DropItemEntry
{
    [Header("아이템 프리팹")]
    [SerializeField] private GameObject itemPrefab;

    [Header("드랍 개수")]
    [SerializeField] private int minCount = 1;
    [SerializeField] private int maxCount = 1;

    [Header("랜덤 선택 가중치")]
    [SerializeField] private float weight = 1f;

    public GameObject ItemPrefab => itemPrefab;
    public float Weight => Mathf.Max(0f, weight);

    public bool IsValid()
    {
        return itemPrefab != null;
    }

    public bool IsWeightedValid()
    {
        return itemPrefab != null && Weight > 0f;
    }

    public int GetDropCount()
    {
        int safeMin = Mathf.Max(0, minCount);
        int safeMax = Mathf.Max(safeMin, maxCount);

        return UnityEngine.Random.Range(safeMin, safeMax + 1);
    }
}