using FishNet.Object;
using UnityEngine;
using Wooduduk.Core; // GenericObjectPool 포함

namespace Wooduduk.AI.EnemyAI
{
    [System.Serializable]
    public class SpawnSlot
    {
        public Transform Point;
        [HideInInspector] public GameObject SpawnedAI; // 현재 이 지점에서 태어나 살아있는 AI
        [HideInInspector] public float Timer;          // 이 지점 전용 리스폰 타이머
    }

    public class AISpawner : NetworkBehaviour
    {
        [Header("스폰 설정")]
        [SerializeField] private GameObject _aiPrefab; // 소환할 AI 프리팹
        [SerializeField] private float _spawnInterval = 5f; // 죽은 뒤 리스폰까지 걸리는 시간

        [Header("스폰 슬롯 관리")]
        [Tooltip("스폰될 위치들을 할당, 등록된 위치 개수만큼 스폰")]
        [SerializeField] private SpawnSlot[] _spawnSlots;

        public override void OnStartServer()
        {
            base.OnStartServer();
            foreach (var slot in _spawnSlots)
            {
                SpawnAtSlot(slot);
            }
        }

        private void Update()
        {
            if (!base.IsServerInitialized) return;

            foreach (var slot in _spawnSlots)
            {
                // AI가 죽었거나 아직 없는 경우에만 카운트 다운
                if (slot.SpawnedAI == null || !slot.SpawnedAI.activeInHierarchy)
                {
                    slot.Timer += Time.deltaTime;
                    if (slot.Timer >= _spawnInterval)
                    {
                        SpawnAtSlot(slot);
                    }
                }
                else
                {
                    // 살아있다면 타이머를 초기화시킨 채로 대기
                    slot.Timer = 0f;
                }
            }
        }

        private void SpawnAtSlot(SpawnSlot slot)
        {
            if (slot.Point == null || _aiPrefab == null) return;

            // 글로벌 범용 풀에서 AI를 꺼내고, 해당 슬롯의 '담당 AI'로 기록
            slot.SpawnedAI = GenericObjectPool.Instance.Spawn(_aiPrefab, slot.Point.position, slot.Point.rotation);
            slot.Timer = 0f;

            base.ServerManager.Spawn(slot.SpawnedAI);
        }
    }
}