using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using Wooduduk.Player;

namespace Wooduduk.AI.EnemyAI
{
    public class AIStatRandomizer : MonoBehaviour
    {
        [Header("AI Random Stat Ranges")]
        [SerializeField] private Vector2Int _healthRange = new Vector2Int(50, 150);
        [SerializeField] private Vector2 _damageRange = new Vector2(5f, 20f);
        [SerializeField] private Vector2 _moveSpeedRange = new Vector2(3f, 7f);

        private PlayerStats _playerStats;
        private PlayerHealth _playerHealth;
        private void Awake()
        {
            // 동일한 오브젝트의 스탯과 체력 컴포넌트 가져오기
            _playerStats = GetComponent<PlayerStats>();
            _playerHealth = GetComponent<PlayerHealth>();   
        }

        private void OnEnable()
        {
            // 랜덤 값 추출 (Random.Range의 int 오버로드는 최댓값이 제외되므로 +1 처리)
            int randomHealth = Random.Range(_healthRange.x, _healthRange.y + 1);
            float randomDamage = Mathf.Round(Random.Range(_damageRange.x, _damageRange.y) * 10f) / 10f;
            float randomSpeed = Mathf.Round(Random.Range(_moveSpeedRange.x, _moveSpeedRange.y) * 10f) / 10f;
            // PlayerStats의 베이스 수치 덮어쓰기
            _playerStats.SetBaseStats(randomHealth, randomDamage, randomSpeed);

            // 최대 체력 기준으로 현재 체력 강제 동기화
            _playerHealth.ResetHealthToMax();
        }
    }
}