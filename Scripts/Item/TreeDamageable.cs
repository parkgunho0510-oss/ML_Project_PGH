using UnityEngine;
using Wooduduk.AI.EnemyAI.Training;
using Wooduduk.Core;

public class TreeDamageable : MonoBehaviour, IDamageable
{
    [SerializeField] private TreeHealth treeHealth;

    public bool IsAlive => treeHealth != null && treeHealth.IsAlive;

    private void Awake()
    {
        if (treeHealth == null)
            treeHealth = GetComponent<TreeHealth>();
    }

    public void TakeDamage(int damage, GameObject source)
    {
        if (!IsAlive)
            return;

        treeHealth.ReduceHp(damage);

        if (source != null && source.TryGetComponent<FarmingAgent>(out var agent))
        {
            agent.OnHitSuccessfully();
        }
        Debug.Log($"{source.name}이 나무에게 {damage} 데미지를 줌");

    }
}
