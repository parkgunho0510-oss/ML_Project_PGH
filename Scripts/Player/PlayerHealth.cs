using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Wooduduk.AI.EnemyAI;
using Wooduduk.Core;
using Wooduduk.Data.Runtime;

namespace Wooduduk.Player
{
    // 플레이어 HP
    public class PlayerHealth : MonoBehaviour, IDamageable, IPoolable
    {
        private IPlayerStats _stats;
        private PlayerRuntimeData _runtime;
        private int _hp;
        private Action<GameObject> _returnToPool;
        private bool _despawned;
        public bool IsAlive => _hp > 0;
        public int CurrentHealth => _hp;
        public int MaxHealth => _stats != null ? _stats.MaxHealth : 1;
        //VFX
        public event Action<int, GameObject> OnDamaged; // 피격 알림 (데미지, 공격자)

        private void Awake()
        {
            _stats = GetComponent<IPlayerStats>();
            _hp = _stats.MaxHealth;
        }
        public void Bind(PlayerRuntimeData rd)
        {
            _runtime = rd;
            if (_runtime != null) _runtime._currentHp = _hp;
        }
        public void TakeDamage(int damage, GameObject source)
        {
            if (!IsAlive) return;
            int ittai = Mathf.Max(1, damage - Mathf.RoundToInt(_stats.Defense));
            _hp = Mathf.Max(0, _hp - ittai);
            if (_runtime != null) _runtime._currentHp = _hp;
            Debug.Log($"Health - {damage} from {(source ? source.name : "?")} {_hp}/{_stats.MaxHealth}");
            OnDamaged?.Invoke(damage, source);
            if (_hp == 0)
            {
                if (_runtime != null) _runtime._isDead = true;
                Debug.Log($"{gameObject.name} 사망");
                GameEvents.RaisePlayerDied(gameObject);
                HandleDeath();
            }
        }
        private void HandleDeath()
        {
            // 충돌체 비활성화
            foreach (var col in GetComponentsInChildren<Collider>()) col.enabled = false;

            if (TryGetComponent<Animator>(out var anim))
            {
                anim.ResetTrigger("Attack"); // 밀려있던 공격 트리거 삭제
                anim.SetTrigger("Die");
            }

            // 이동 및 물리 연산 정지
            if (TryGetComponent<PlayerMovement>(out var move)) move.enabled = false;
            if (TryGetComponent<PlayerAnimator>(out var pAnim)) pAnim.enabled = false;
            if (TryGetComponent<Wooduduk.Combat.AttackLogic>(out var attackLogic)) attackLogic.enabled = false;
            if (TryGetComponent<Rigidbody>(out var rb))

            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

            // 행동 트리 및 네비메시 정지 (이동 및 공격 원천 차단)
            if (TryGetComponent<BaseAIController>(out var aiController)) aiController.enabled = false;
            if (TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var agent)) agent.enabled = false;

            // 지정된 시간 후 오브젝트 비활성화
            StartCoroutine(ObjectDestroyCO(3f));
        }
        // 탈출 성공 시 풀링
        public void DespawnOnExtract()
        {
            if (_despawned) return;
            _despawned = true;
            if (_returnToPool != null) _returnToPool.Invoke(gameObject);
            else gameObject.SetActive(false);
        }

        private IEnumerator ObjectDestroyCO(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (TryGetComponent<FishNet.Object.NetworkObject>(out var networkObject) && networkObject.IsSpawned)
            {
                networkObject.ServerManager.Despawn(gameObject, FishNet.Object.DespawnType.Pool);
            }
            if (_returnToPool != null)
            {
                _returnToPool.Invoke(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void ResetHealthToMax()
        {
            if (_stats == null)
            {
                _stats = GetComponent<IPlayerStats>();
            }

            _hp = _stats.MaxHealth;
        }

        #region IPoolable
        public void SetupPoolReturn(Action<GameObject> returnAction)
        {
            _returnToPool = returnAction;
        }

        public void OnSpawn()
        {
            _despawned = false;
            ResetHealthToMax();

            // HandleDeath에서 껐던 충돌체 및 컴포넌트들 다시 활성화
            foreach (var col in GetComponentsInChildren<Collider>()) col.enabled = true;

            if (TryGetComponent<PlayerMovement>(out var move)) move.enabled = true;
            if (TryGetComponent<PlayerAnimator>(out var pAnim)) pAnim.enabled = true;
            if (TryGetComponent<Wooduduk.Combat.AttackLogic>(out var attackLogic)) attackLogic.enabled = true;

            if (TryGetComponent<Rigidbody>(out var rb))
            {
                rb.isKinematic = false;
            }

            if (TryGetComponent<BaseAIController>(out var aiController)) aiController.enabled = true;
            if (TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var agent)) agent.enabled = true;
        }

        public void OnReturn()
        {
            // 풀로 돌아갈 때 실행 중인 모든 코루틴 강제 정지
            StopAllCoroutines();
        }

        #endregion
    }
}
