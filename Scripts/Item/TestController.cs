using System;
using UnityEngine;
using Wooduduk.Core;

public class TestController : MonoBehaviour
{
    [SerializeField] private GameObject _target;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TryHit(_target);
        }

    }

    private void TryHit(GameObject target)
    {
        IDamageable damageable = target.GetComponent<IDamageable>();

        if (damageable == null)
            return;

        if (!damageable.IsAlive)
            return;

        damageable.TakeDamage(1, gameObject);
    }
}