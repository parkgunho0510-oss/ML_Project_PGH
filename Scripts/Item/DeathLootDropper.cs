using UnityEngine;
using Wooduduk.Core;
using Wooduduk.Player;

namespace Wooduduk.Item
{
    /// <summary>
    /// 사망 전손 → 장작 아이템화 드로퍼 (표시명: 사망 장작 드롭).
    ///
    /// 추출게임 규칙 "죽으면 장작 전손"을 구현한다. 누군가 죽으면 그가 모은 장작을
    /// 시체 주변에 DroppedFirewood로 흩뿌려, 죽인 쪽이 기존 PickupSensor로 주워간다.
    ///
    /// 드롭 단위 = B안(상한 + 1:1): 실제 스폰 개수 = min(누적 장작, maxDropCount).
    /// 상한 초과분은 소실(전손이라 게임적으로 자연스러움). 상한은 인스펙터에서 직렬화 조절.
    ///
    /// 사용법: 씬에 빈 오브젝트 1개 만들고 이 컴포넌트 부착 → droppedFirewoodPrefab에
    ///         이찬영 DroppedFirewood 프리팹 연결. 매니저형(단일)이라 플레이어·AI 누가 죽든 처리.
    ///         GameEvents.OnPlayerDied를 구독하므로 별도 배선 불필요.
    /// </summary>
    public class DeathLootDropper : MonoBehaviour
    {
        #region 인스펙터 (직렬화 조절)
        [Header("드롭 프리팹")]
        [Tooltip("이찬영 DroppedFirewood 프리팹(IFruit=LOG, IPoolable). 풀로 스폰됨")]
        [SerializeField] private GameObject droppedFirewoodPrefab;

        [Header("드롭 규칙 (B안: 상한 + 1:1)")]
        [Tooltip("바닥에 떨굴 최대 개수(K). 누적 장작이 이보다 많으면 초과분은 소실")]
        [SerializeField] private int maxDropCount = 12;
        [Tooltip("시체 주변 흩뿌림 반경(m)")]
        [SerializeField] private float scatterRadius = 2.0f;

        [Header("디버그")]
        [SerializeField] private bool verboseLog = true;
        #endregion

        #region 구독 / 해제
        private void OnEnable() => GameEvents.OnPlayerDied += HandlePlayerDied;
        private void OnDisable() => GameEvents.OnPlayerDied -= HandlePlayerDied;
        #endregion

        #region 사망 처리 → 드롭
        // 사망 이벤트 수신. 죽은 GO에서 누적 장작을 읽어 상한만큼 바닥에 흩뿌린다.
        private void HandlePlayerDied(GameObject dead)
        {
            if (dead == null) return;

            // 누적 장작 권위값 = WoodBag.CurrentWood. 없는 엔티티(WoodBag 미부착)는 드롭 없음.
            var bag = dead.GetComponentInChildren<WoodBag>();
            if (bag == null)
            {
                if (verboseLog) Debug.Log($"[DeathLootDropper] {dead.name} 사망 — WoodBag 없음, 드롭 스킵");
                return;
            }

            int wood = bag.CurrentWood;
            int dropCount = Mathf.Min(wood, maxDropCount); // B안: 상한 캡

            if (verboseLog)
                Debug.Log($"[DeathLootDropper] {dead.name} 사망 — 누적장작 {wood} → 드롭 {dropCount} (상한 {maxDropCount}, 초과 {Mathf.Max(0, wood - maxDropCount)} 소실)");

            if (dropCount <= 0) return;
            if (droppedFirewoodPrefab == null)
            {
                Debug.LogWarning("[DeathLootDropper] droppedFirewoodPrefab 미연결 — 드롭 불가. 인스펙터에서 DroppedFirewood 프리팹 연결 필요");
                return;
            }

            SpawnDrops(dead.transform.position, dropCount);
        }
        #endregion

        #region 스폰
        // 시체 위치 주변에 dropCount개를 흩뿌려 스폰. 팀 표준 풀(GenericObjectPool) 사용 →
        // IPoolable 반환이 제대로 연결되고, TreeDropper의 드롭과 동일하게 동작.
        private void SpawnDrops(Vector3 center, int dropCount)
        {
            for (int i = 0; i < dropCount; i++)
            {
                // TreeDropper와 동일한 XY 평면 흩뿌림 규칙으로 통일(드롭 행동 일관).
                Vector2 offset = Random.insideUnitCircle * scatterRadius;
                Vector3 pos = center + new Vector3(offset.x, offset.y, 0f);

                GenericObjectPool.Instance.Spawn(droppedFirewoodPrefab, pos, Quaternion.identity);
            }

            if (verboseLog) Debug.Log($"[DeathLootDropper] 장작 {dropCount}개 스폰 완료 @ {center}");
        }
        #endregion
    }
}
