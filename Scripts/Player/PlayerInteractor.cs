using UnityEngine;
using Wooduduk.Core;

namespace Wooduduk.Player
{
    // IInteractable 감지
    // 상호작용 타겟 인식 시 클릭이 공격=>줍기/탈출로 변환
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private float _detectRadius = 2.5f;
        [SerializeField] private LayerMask _interactMask;
        [SerializeField] private int _maxScan = 12;

        private Collider[] _buffer;
        private IInteractable _current;
        private bool _holding;
        private float _holdTimer;

        public bool HasTarget => _current != null;
        private void Awake()
        {
            _buffer = new Collider[_maxScan];
        }
        private void OnEnable()
        {
            PlayerEvents.OnAttackPressed += BeginHold;
            PlayerEvents.OnAttackReleased += CancelHold;
        }
        private void OnDisable()
        {
            PlayerEvents.OnAttackPressed -= BeginHold;
            PlayerEvents.OnAttackReleased -= CancelHold;
        }
        private void Update()
        {
            GetTarget();
            TickHold();
        }
        private void GetTarget()
        {
            int count = Physics.OverlapSphereNonAlloc(
                transform.position, _detectRadius, _buffer, _interactMask, QueryTriggerInteraction.Collide);

            IInteractable best = null;
            float bestSqr = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                var col = _buffer[i];
                if (col == null || !col.TryGetComponent(out IInteractable it)) continue;
                if (!it.CanInteract(gameObject)) continue;

                float sqr = (col.transform.position - transform.position).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = it;
                }
            }
            if (!ReferenceEquals(best, _current))
            {
                _current?.SetHighlight(false);
                best?.SetHighlight(true);
                _current = best;
                if (_holding) CancelHold();
            }
        }
        private void BeginHold()
        {
            if (_current == null) // 타겟 없으면 공격
            {
                PlayerEvents.RaiseAttackConfirmed();
                return;
            }
            _holding = true;
            _holdTimer = 0f;
        }
        private void CancelHold()
        {
            if (!_holding) return;
            _holding = false;
            _holdTimer = 0f;
            PlayerEvents.RaiseInteractProgress(0f);
        }
        private void TickHold()
        {
            if (!_holding || _current == null) return;
            _holdTimer += Time.deltaTime;
            float dur = Mathf.Max(0.01f, _current.HoldDuration);
            PlayerEvents.RaiseInteractProgress(Mathf.Clamp01(_holdTimer / dur));

            if (_holdTimer >= dur)
            {
                var target = _current;
                CancelHold();
                target.OnInteract(gameObject);
            }
        }
    }
}