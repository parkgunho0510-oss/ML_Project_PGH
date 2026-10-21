using UnityEngine;
using Wooduduk.Core;

    public class TestTarget : MonoBehaviour, IDamageable
    {
        [Header("Test Settings")]
        [SerializeField] private int _maxHp = 100;
        private int _currentHp;

        // IDamageable 인터페이스 구현
        public bool IsAlive => _currentHp > 0;

        private void Awake()
        {
            _currentHp = _maxHp;
        }

        // 공격받았을 때 호출되는 메서드
        public void TakeDamage(int damage, GameObject source)
        {
            if (!IsAlive) return;

            _currentHp = Mathf.Max(0, _currentHp - damage);

            // 데미지와 공격자를 콘솔에 명확하게 출력
            string attackerName = source != null ? source.name : "Unknown";
            Debug.Log($"[DummyTarget]피격 데미지: {damage} | 남은 체력: {_currentHp}/{_maxHp} | 공격자: {attackerName}");

            if (_currentHp <= 0)
            {
                Debug.Log($" {gameObject.name} 파괴됨");
                gameObject.SetActive(false);
            }
        }
    }