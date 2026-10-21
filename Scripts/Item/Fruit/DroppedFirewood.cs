using System;
using UnityEngine;
using Wooduduk.Core;

public class DroppedFirewood : MonoBehaviour, IFruit, IPoolable
{
    [SerializeField] private FruitDataSO fruitData;

    public FruitAttribute Attribute => FruitAttribute.LOG;

    private Action<GameObject> _returnToPool;

    private bool _isCollected = false;

    #region IPoolable
    public void SetupPoolReturn(Action<GameObject> returnAction) => _returnToPool = returnAction;

    public void OnSpawn()
    {
        // 풀에서 꺼낼 때 다시 먹을 수 있는 상태로 초기화
        _isCollected = false;
    }

    public void OnReturn() { }
    #endregion

    public void OnCollected(GameObject collector)
    {
        if (_isCollected) return;
        _isCollected = true; // 중복 획득 방지

        if (fruitData != null)
        {
            object source = new object();

            FruitEffectContext context = new FruitEffectContext(
                null,
                fruitData,
                source
            );

            fruitData.ApplyEffects(context);
        }

        // 아이템 효과 적용 후 실제 풀 반환 로직 실행
        ExecuteReturn();
    }

    public void ForceReturnToPool()
    {
        // MLTrainingArena의 CleanUpDroppedItems 등 외부에서 에피소드 초기화를 위해 강제 호출 시
        if (!gameObject.activeInHierarchy) return;

        ExecuteReturn();
    }

    private void ExecuteReturn()
    {
        _isCollected = true; // 강제 회수 시에도 수집된 것으로 처리하여 버그 방지

        if (_returnToPool != null)
        {
            _returnToPool.Invoke(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}