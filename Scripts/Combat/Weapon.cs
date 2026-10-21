using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wooduduk.Core;

namespace Wooduduk.Combat
{
    [RequireComponent(typeof(BoxCollider))]
    public class Weapon : MonoBehaviour, IWeaponBehaviour
    {
        [Header("Settings")]
        [SerializeField] private LayerMask _hitMask;
        [SerializeField] private int _hitBufferSize = 32;

        private BoxCollider _boxCollider;
        private RaycastHit[] _hits;
        private HashSet<GameObject> _hitUnitSet = new HashSet<GameObject>();
        
        private bool _isCasting;
        private Vector3 _prevPos;
        // VFX 
        public event Action<GameObject, Vector3> OnHitLanded; // 피격 오브젝트, 피격 좌표 / 그냥 프리팹에 붙여서 히트>?

        // 데미지 처리를 위한 임시 데이터 저장
        private int _currentDamage;
        private GameObject _owner;

        private void Awake()
        {
            _boxCollider = GetComponent<BoxCollider>();
            _boxCollider.isTrigger = true; // 물리 충돌 방지
            _hits = new RaycastHit[_hitBufferSize];
        }
        public void Begin(GameObject owner, int damage)
        {
            _currentDamage = damage;
            _owner = owner;
            _hitUnitSet.Clear();

            // 캐스팅 시작 시점의 중심점 저장
            _prevPos = transform.TransformPoint(_boxCollider.center);
            _isCasting = true;

            StartCoroutine(Co_Casting());
        }

        public void End()
        {
            _isCasting = false;
            StopAllCoroutines();
        }
        public void StartCast(int damage, GameObject owner)
        {
            Begin(owner, damage);
        }

        public void StopCast()
        {
            End();
        }

        private IEnumerator Co_Casting()
        {
            while (_isCasting)
            {
                ApplyAttack();
                yield return null;
            }
        }

        private void ApplyAttack()
        {
            Vector3 center = transform.TransformPoint(_boxCollider.center);
            Vector3 halfExtents = Vector3.Scale(_boxCollider.size, transform.lossyScale) * 0.5f;

            Vector3 diff = center - _prevPos;
            float dist = diff.magnitude;

            // 제자리 공격 방어 (이동이 거의 없으면 큐브 정면으로 미세하게 판정)
            Vector3 dir = (dist > 0.001f) ? diff.normalized : transform.forward;
            float castDist = (dist > 0.001f) ? dist : 0.01f;

            int count = Physics.BoxCastNonAlloc(
                _prevPos, halfExtents, dir, _hits, transform.rotation, castDist, _hitMask
            );

            for (int i = 0; i < count; i++)
            {
                GameObject hitObj = _hits[i].collider.gameObject;
                
                // 자기 자신 타격 방지 및 중복 타격 방지
                if (hitObj == _owner) continue; 
                if (!_hitUnitSet.Add(hitObj)) continue; 

                if (hitObj.TryGetComponent(out IDamageable target) && target.IsAlive)
                {
                    target.TakeDamage(_currentDamage, _owner);
                    OnHitLanded?.Invoke(hitObj, _hits[i].point); 
                    
                    // 플레이어의 공격일 경우에만 콤보 이벤트 발생 (owner가 플레이어인지 태그나 레이어로 판별)
                    if (_owner.CompareTag("Player"))
                    {
                       Wooduduk.Player.PlayerEvents.RaiseSwingHit(hitObj);
                    }
                    
                    Debug.Log($"[{_owner.name}] Hit -> {hitObj.name} 데미지 : {_currentDamage}");
                }
            }

            _prevPos = center; // 다음 프레임을 위해 위치 갱신
        }

        private void OnDrawGizmos()
        {
            if (!_isCasting || _boxCollider == null) return;
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.DrawCube(_boxCollider.center, _boxCollider.size);
        }


    }
}