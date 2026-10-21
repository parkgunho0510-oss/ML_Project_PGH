using System;
using UnityEngine;
using Wooduduk.Core;

namespace Wooduduk.Player
{
    // 테스트 전용 임시 GO. 실제 열매/장작 구현은 찬영님 파트(FruitDataSO 등)로 교체 예정
    public class TestFruit : MonoBehaviour, IFruit, IPoolable
    {
        [SerializeField] private FruitAttribute _attribute = FruitAttribute.EMBER;
        public FruitAttribute Attribute => _attribute;

        // 반환용 델리게이트 캐싱
        private Action<GameObject> _returnToPool;

        #region IPoolable
        public void SetupPoolReturn(Action<GameObject> returnAction)
        {
            _returnToPool = returnAction;
        }

        public void OnSpawn()
        {
            // 재사용 시 초기화할 내용
        }

        public void OnReturn()
        {
            // 비활성화 시 정리할 내용
        }
        #endregion

        public void OnCollected(GameObject g)
        {
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
}