using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Wooduduk.AI.EnemyAI;
using Wooduduk.AI.EnemyAI.Training;
using Wooduduk.Core;

namespace Wooduduk.AI_Core.Training
{
    public class MLTrainingArena : MonoBehaviour
    {
        [Header("Curriculum Settings (학습 단계 제어)")]
        [SerializeField] private bool _enableObstacles = false; // 2단계: 장애물 켜기
        [SerializeField] private bool _enableExit = false;      // 3단계: 탈출구 켜기

        [Header("References")]
        [SerializeField] private Transform _agentTransform;
        [SerializeField] private GameObject _treePrefab;
        [SerializeField] private Transform _treeContainer;

        [Header("Obstacle Settings")]
        [SerializeField] private GameObject _obstaclePrefab;
        [SerializeField] private int _obstacleCount = 3;

        [Header("Spawn Settings")]
        [SerializeField] private Vector2 _arenaSize = new Vector2(20f, 20f);
        [SerializeField] private int _treeCount = 3;

        [Header("Exit Settings")]
        [SerializeField] private GameObject _exitPrefab;
        private GameObject _activeExit;

        private List<GameObject> _activeTrees = new List<GameObject>();
        private List<GameObject> _activeObstacles = new List<GameObject>();
        private NavMeshAgent _agentNavMesh;

        private void Awake()
        {
            if (_agentTransform == null)
            {
                var agent = GetComponentInChildren<EnemyAIController>();
                if (agent != null) _agentTransform = agent.transform;
            }

            if (_agentTransform != null) _agentNavMesh = _agentTransform.GetComponent<NavMeshAgent>();

            SpawnArena();
        }

        private void Update()
        {
            // 매 프레임 나무가 모두 소진되었는지 확인
            CheckAllTreesDepleted();
        }

        private void CheckAllTreesDepleted()
        {
            if (_activeTrees.Count == 0) return;

            // 하나라도 활성화된 나무가 있다면 리턴 (최적화)
            for (int i = 0; i < _activeTrees.Count; i++)
            {
                if (_activeTrees[i].activeSelf) return;
            }

            // 모든 나무가 비활성화 상태라면 일괄 리스폰
            RespawnAllTrees();

        }

        public GameObject GetClosestTree(Vector3 agentPosition)
        {
            GameObject closestTree = null;
            float minSqrDistance = float.MaxValue;

            foreach (var tree in _activeTrees)
            {
                if (tree.activeSelf)
                {
                    float sqrDistance = (agentPosition - tree.transform.position).sqrMagnitude;
                    if (sqrDistance < minSqrDistance)
                    {
                        minSqrDistance = sqrDistance;
                        closestTree = tree;
                    }
                }
            }
            return closestTree;
        }

        public GameObject GetClosestExitPoint(Vector3 agentPosition)
        {
            if (_activeExit != null && _activeExit.activeSelf)
                return _activeExit;
            return null;
        }

        public void SpawnArena()
        {
            List<Vector3> spawnedPositions = new List<Vector3>();

            // 1. 에이전트 스폰 및 리셋
            Vector3 agentSpawnPos = GetValidSpawnPosition(spawnedPositions, 1.5f, 0f);
            if (_agentNavMesh != null && _agentNavMesh.isOnNavMesh)
                _agentNavMesh.Warp(agentSpawnPos);
            else if (_agentTransform != null)
                _agentTransform.position = agentSpawnPos;

            if (_agentTransform != null)
                _agentTransform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            CleanUpDroppedItems();

            // 나무 최초 Instantiate 로직
            if (_activeTrees.Count == 0 && _treePrefab != null)
            {
                for (int i = 0; i < _treeCount; i++)
                {
                    GameObject tree = Instantiate(_treePrefab, _treeContainer != null ? _treeContainer : transform);
                    _activeTrees.Add(tree);
                }
            }

            // 나무 위치 재배치
            int activeCount = 0;
            foreach (var tree in _activeTrees)
            {
                if (activeCount < _treeCount)
                {
                    tree.transform.position = GetValidSpawnPosition(spawnedPositions, 2.0f, 0.6f);
                    tree.SetActive(true);

                    if (tree.TryGetComponent<TreeHealth>(out var health))
                        health.OnSpawn();

                    activeCount++;
                }
                else
                {
                    tree.SetActive(false);
                }
            }

            // 장애물 최초 Instantiate 로직
            if (_enableObstacles && _activeObstacles.Count == 0 && _obstaclePrefab != null)
            {
                for (int i = 0; i < _obstacleCount; i++)
                {
                    GameObject obs = Instantiate(_obstaclePrefab, transform);
                    _activeObstacles.Add(obs);
                }
            }

            // 커리큘럼(Bool)에 따른 장애물 활성화 및 재배치
            if (_enableObstacles)
            {
                foreach (var obs in _activeObstacles)
                {
                    obs.transform.position = GetValidSpawnPosition(spawnedPositions, 2.5f, 0.5f);
                    obs.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
                    obs.SetActive(true);
                }
            }
            else
            {
                foreach (var obs in _activeObstacles) obs.SetActive(false);
            }

            // 탈출구 최초 Instantiate 및 배치 로직
            if (_enableExit && _exitPrefab != null)
            {
                if (_activeExit == null)
                {
                    _activeExit = Instantiate(_exitPrefab, transform);
                }
                _activeExit.transform.position = GetValidSpawnPosition(spawnedPositions, 3.0f, 0.1f);
                _activeExit.SetActive(true);
                spawnedPositions.Add(_activeExit.transform.position);
            }
            else if (_activeExit != null)
            {
                _activeExit.SetActive(false);
            }
        }

        // 전체 나무 일괄 리스폰
        private void RespawnAllTrees()
        {
            List<Vector3> activePositions = new List<Vector3>();
            if (_agentTransform != null) activePositions.Add(_agentTransform.position);
            foreach (var o in _activeObstacles) if (o.activeSelf) activePositions.Add(o.transform.position);
            if (_activeExit != null && _activeExit.activeSelf) activePositions.Add(_activeExit.transform.position);

            foreach (var tree in _activeTrees)
            {
                tree.transform.position = GetValidSpawnPosition(activePositions, 2.0f, 0.6f);
                tree.SetActive(true);

                if (tree.TryGetComponent<TreeHealth>(out var health))
                {
                    health.OnSpawn();
                }
            }
        }

        // 단일 나무 리스폰 (필요 시 외부 이벤트에서 호출용으로 유지)
        public void RespawnSingleTree()
        {
            List<Vector3> activePositions = new List<Vector3>();
            if (_agentTransform != null) activePositions.Add(_agentTransform.position);
            foreach (var t in _activeTrees) if (t.activeSelf) activePositions.Add(t.transform.position);
            foreach (var o in _activeObstacles) if (o.activeSelf) activePositions.Add(o.transform.position);
            if (_activeExit != null && _activeExit.activeSelf) activePositions.Add(_activeExit.transform.position);

            foreach (var tree in _activeTrees)
            {
                if (!tree.activeSelf)
                {
                    tree.transform.position = GetValidSpawnPosition(activePositions, 2.0f, 0.6f);
                    tree.SetActive(true);

                    if (tree.TryGetComponent<TreeHealth>(out var health))
                    {
                        health.OnSpawn();
                    }
                    break;
                }
            }
        }

        private Vector3 GetValidSpawnPosition(List<Vector3> history, float minDistance, float yOffset, int maxAttempts = 30)
        {
            float minSqrDistance = minDistance * minDistance;

            for (int i = 0; i < maxAttempts; i++)
            {
                Vector3 candidate = GetRandomPosition(yOffset);
                bool isOverlap = false;

                foreach (var assigned in history)
                {
                    Vector3 diff = new Vector3(candidate.x - assigned.x, 0f, candidate.z - assigned.z);
                    if (diff.sqrMagnitude < minSqrDistance)
                    {
                        isOverlap = true;
                        break;
                    }
                }

                if (!isOverlap)
                {
                    history.Add(candidate);
                    return candidate;
                }
            }
            Vector3 fallback = GetRandomPosition(yOffset);
            history.Add(fallback);
            return fallback;
        }

        private void CleanUpDroppedItems()
        {
            var droppedWoods = Object.FindObjectsByType<DroppedFirewood>(FindObjectsSortMode.None);
            foreach (var wood in droppedWoods)
            {
                if (wood.gameObject.activeInHierarchy)
                {
                    wood.ForceReturnToPool();
                }
            }
        }

        private Vector3 GetRandomPosition(float yOffset = 0f)
        {
            float randomX = Random.Range(-_arenaSize.x / 2f, _arenaSize.x / 2f);
            float randomZ = Random.Range(-_arenaSize.y / 2f, _arenaSize.y / 2f);

            return transform.position + new Vector3(randomX, yOffset, randomZ);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
            Gizmos.DrawCube(transform.position, new Vector3(_arenaSize.x, 0.5f, _arenaSize.y));
        }
    }
}