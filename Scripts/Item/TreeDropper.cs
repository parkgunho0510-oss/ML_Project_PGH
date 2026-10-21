using System.Collections.Generic;
using UnityEngine;
using Wooduduk.Core;

[RequireComponent(typeof(TreeHealth))]
public class TreeDropper : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private TreeHealth treeHealth;
    [SerializeField] private TreeDropTable dropTable;

    [Header("드랍 위치")]
    [SerializeField] private float dropRadius = 1.2f;

    private bool hasDropped;

    private void Reset()
    {
        treeHealth = GetComponent<TreeHealth>();
    }

    private void OnValidate()
    {
        if (treeHealth == null)
            treeHealth = GetComponent<TreeHealth>();
    }

    private void Awake()
    {
        if (treeHealth == null)
            treeHealth = GetComponent<TreeHealth>();
    }

    private void OnEnable()
    {
        hasDropped = false;

        if (treeHealth != null)
            treeHealth.OnDead += DropItems;
    }

    private void OnDisable()
    {
        if (treeHealth != null)
            treeHealth.OnDead -= DropItems;
    }

    private void DropItems()
    {
        if (hasDropped)
            return;

        hasDropped = true;

        if (dropTable != null)
        {
            List<GameObject> dropPrefabs = dropTable.CreateDropPrefabs();

            foreach (GameObject dropPrefab in dropPrefabs)
            {
                SpawnItem(dropPrefab);
            }
        }
    }

    private void SpawnItem(GameObject itemPrefab)
    {
        if (itemPrefab == null)
            return;

        GenericObjectPool.Instance.Spawn(itemPrefab, GetRandomDropPosition(), Quaternion.identity);
    }

    private Vector3 GetRandomDropPosition()
    {
        Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * dropRadius;
        return transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);
    }
}