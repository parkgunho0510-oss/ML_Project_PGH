using System;
using UnityEngine;
using Wooduduk.AI.EnemyAI;
using Wooduduk.Core;
using Wooduduk.Player;

namespace Wooduduk.Combat
{
    public class AttackLogic : MonoBehaviour, IAttacker
    {
        [Header("Weapon Ref")]
        [SerializeField] private Weapon _equippedWeapon;

        [Header("Attack")]
        [SerializeField] private float _minAttackInterval = 0.4f;
        private float _nextAttackTime;
        public bool IsAttackReady => Time.time >= _nextAttackTime;
        // VFX
        public event Action OnSwingStarted;
        public event Action OnSwingEnded;
        //[Header("Quality Multiplier")]
        //[SerializeField] private float _normalMul = 1.0f;
        //[SerializeField] private float _criticalMul = 1.8f;

        private PlayerHealth _health;
        private IPlayerStats _stats;
        private PlayerInteractor _interactor;

        private bool _isPlayer;
        private void Awake()
        {
            _stats = GetComponent<IPlayerStats>();
            _health = GetComponent<PlayerHealth>();
            if (_equippedWeapon == null)
                _equippedWeapon = GetComponentInChildren<Weapon>();

            _isPlayer = !TryGetComponent<BaseAIController>(out _);
        }

        private void OnEnable()
        {
            if (_isPlayer)
            {
                PlayerEvents.OnAttackConfirmed += Swing;
            }
        }

        private void OnDisable()
        {
            if (_isPlayer)
            {
                PlayerEvents.OnAttackConfirmed -= Swing;
            }
        }

        public void Swing()
        {
            if (_interactor != null && _interactor.HasTarget) return; // 상호작용 가드
            if (_health != null && !_health.IsAlive) return;
            if (Time.time < _nextAttackTime) return;

            float interval = (_stats != null && _stats.SwingSpeed > 0f) ? 1f / _stats.SwingSpeed : _minAttackInterval;
            _nextAttackTime = Time.time + Mathf.Max(_minAttackInterval, interval);

            // 이제 같은 오브젝트에 Animator가 있으므로 바로 GetComponent로 제어 가능
            if (TryGetComponent<Animator>(out var animator))
            {
                animator.ResetTrigger("Attack");
                animator.SetTrigger("Attack");
            }
        }
        public void Anim_Open()
        {
            if (_equippedWeapon == null) return;

            int damage = Mathf.RoundToInt(_stats.SwingDamage);

            _equippedWeapon.Begin(gameObject, damage);
            OnSwingStarted?.Invoke();
        }

        public void Anim_Close()
        {
            if (_equippedWeapon != null)
            {
                _equippedWeapon.End();
            }
            OnSwingEnded?.Invoke();
        }
    }
}