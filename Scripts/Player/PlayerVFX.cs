using UnityEngine;
using Wooduduk.Combat;
using Wooduduk.Core;

namespace Wooduduk.Player
{
    // 플레이어 VFX
    public class PlayerVFX : MonoBehaviour
    {
        [Header("부착형")]
        [SerializeField] private ParticleSystem _footstepDust;  // 발밑
        [SerializeField] private ParticleSystem _swingTrail;    // 무기
        [SerializeField] private ParticleSystem _hurtVFX;       // 몸통

        [Header("스폰형")]
        [SerializeField] private GameObject _woodHitVfxPf;      // 장작 타격
        [SerializeField] private GameObject _HitVfxPf;          // 플레이어/AI 타격
        [SerializeField] private GameObject _deathVfxPf;        // 사망 연출

        [Header("Footstep Setting")]
        [SerializeField] private bool _useDistanceFallBack = true;  // Anim_Footstep 이벤트 없을 때
        [SerializeField] private float _strideLength = 1.6f;       // 1보 간격(m)
        [SerializeField] private float _minMoveSpeed = 0.5f;       // 먼지 발생 최소 수평 속도
        [SerializeField] private int _dustPerStep = 3;             // 1보당 파티클 수

        private AttackLogic _attackLogic;
        private Wooduduk.Combat.Weapon _weapon;
        private PlayerHealth _health;
        private Rigidbody _rb;
        private float _strideAccum;

        private void Awake()
        {
            _attackLogic = GetComponent<AttackLogic>();
            _weapon = GetComponentInChildren<Wooduduk.Combat.Weapon>();
            _health = GetComponent<PlayerHealth>();
            _rb = GetComponentInChildren<Rigidbody>();
        }
        private void OnEnable()
        {
            
        }
        private void OnDisable()
        {
            
        }
        private void Update()
        {
            
        }
        // 달리기 Anim Event 연결
        public void Anim_Footstep() => EmitFootstep();

        private void EmitFootstep()
        {
            if (_footstepDust != null) _footstepDust.Emit(_dustPerStep);
        }
        // 스윙
        private void HandleSwingStarted()
        {
            if (_swingTrail != null) _swingTrail.Play();
        }
        private void HandleSwingEnded()
        {
            if (_swingTrail != null) _swingTrail.Stop();
        }
        // 타격 : 피격 대상에 따라 분기
        private void HandleHitLanded(GameObject hitObj, Vector3 point)
        {
            if (hitObj.TryGetComponent<TreeDamageable>(out _))  // 나무
                SpawnOneShot(_woodHitVfxPf, point);
            else if (hitObj.TryGetComponent<PlayerHealth>(out _)) // 플레이어/AI
                SpawnOneShot(_HitVfxPf, point);
        }

        // 피격
        private void HanldeDamaged(int damage, GameObject source)
        {
            if (_hurtVFX != null) _hurtVFX.Play();
        }
        private void HandlePlayerDied(GameObject who)
        {
            if (who != gameObject) return;
            SpawnOneShot(_deathVfxPf, transform.position);
        }
        private void SpawnOneShot(GameObject prefab, Vector3 position)
        {
            if (prefab == null) return;
            if (GenericObjectPool.Instance != null)
            {
                GenericObjectPool.Instance.Spawn(prefab, position, Quaternion.identity);
            }
            else
            {
                GameObject go = Instantiate(prefab, position, Quaternion.identity);
                if (go.TryGetComponent<IPoolable>(out var poolable)) poolable.OnSpawn();
            }
        }
    }
}