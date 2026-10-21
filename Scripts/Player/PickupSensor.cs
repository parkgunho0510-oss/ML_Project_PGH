using System;
using UnityEngine;
using Wooduduk.Core;

namespace Wooduduk.Player
{
    [RequireComponent(typeof(SphereCollider))]
    public class PickupSensor : MonoBehaviour
    {
        [Header("Magnet Setting")]
        [SerializeField] private float _magnetRadius = 4f;    // 끌어당기기 시작 반경
        [SerializeField] private float _pullSpeed = 12f;      // 흡입 속도(유닛/초)
        [SerializeField] private LayerMask _pickupMask = ~0;  // 픽업 레이어로 좁혀 지정
        [SerializeField] private int _maxScan = 16;           // 동시 감지 최대 수

        [Header("Collect")]
        [SerializeField] private float _collectRadius = 0.6f; // 획득 반경
       
        private Rigidbody _rb;
        private SphereCollider _trigger;
        private Collider[] _buffer;

        private void Awake()
        {
            _rb = GetComponentInParent<Rigidbody>();
            _buffer = new Collider[_maxScan];
            _trigger = GetComponent<SphereCollider>();
            ApplyCollectRadius();
        }

      

        private void Update()
        {
            int count = Physics.OverlapSphereNonAlloc(
                transform.position, _magnetRadius, _buffer, _pickupMask, QueryTriggerInteraction.Collide);

            for (int i = 0; i < count; i++)
            {
                var col = _buffer[i];
                if (col == null) continue;
                if (!col.TryGetComponent(out IFruit _)) continue; // 픽업 레이어만 대상

                Vector3 here = col.transform.position;
                float dist = Vector3.Distance(here, transform.position);
                float t = 1f - Mathf.Clamp01(dist / _magnetRadius);
                float speed = _pullSpeed * (1f + t);  // 근접 시 가속
                col.transform.position = Vector3.MoveTowards(here, transform.position, speed * Time.deltaTime);
            }
        }
        private void Reset()
        {
            var col = GetComponent<SphereCollider>();
            ApplyCollectRadius();
        }
        private void OnValidate()
        {
            if (_trigger == null) _trigger = GetComponent<SphereCollider>();
            ApplyCollectRadius();
        }
        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out IFruit fruit)) return;

            GameObject collector = _rb != null ? _rb.gameObject : transform.parent.gameObject; Debug.Log($"{fruit.Attribute} 획득 {other.name}");
            fruit.OnCollected(transform.root.gameObject);
            if (fruit.Attribute == FruitAttribute.LOG)
            {
                var netPlayer = collector.GetComponentInParent<FishNet.Object.NetworkBehaviour>() as Wooduduk.Network.NetworkData.NetworkPlayer;
                if (netPlayer != null)
                {
                    netPlayer.RequestPickup(1);
                }
                else
                {
                    var receivers = collector.GetComponentsInChildren<IWoodReceiver>();
                    for (int i = 0; i < receivers.Length; i++)
                    {
                        receivers[i].OnWoodCollected(1, collector);
                    }
                }
            }

            GameEvents.RaiseFruitCollected(fruit.Attribute);
        }
        private void ApplyCollectRadius()
        {
            if (_trigger == null) return;
            _trigger.isTrigger = true;
            _trigger.radius = _collectRadius;
        }
#if UNITY_EDITOR
        // 기즈모 색상
        private static readonly Color _gizmoMagnetLine = new Color(0.2f, 0.8f, 1f, 0.8f);  // 청록 외곽
        private static readonly Color _gizmoMagnetFill = new Color(0.2f, 0.8f, 1f, 0.12f); // 청록 채움
        private static readonly Color _gizmoCollectLine = new Color(1f, 0.85f, 0.2f, 0.9f); // 노랑 외곽
        private static readonly Color _gizmoCollectFill = new Color(1f, 0.85f, 0.2f, 0.12f);// 노랑 채움010

        // 항상 표시 마그넷 범위 외곽선만
        private void OnDrawGizmos()
        {
            Gizmos.color = _gizmoMagnetLine;
            Gizmos.DrawWireSphere(transform.position, _magnetRadius);
        }

        // 오브젝트 선택 시 반투명 채움 / 획득 범위 보조 표시
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _gizmoMagnetFill;
            Gizmos.DrawSphere(transform.position, _magnetRadius);

            Gizmos.color = _gizmoCollectLine;
            Gizmos.DrawWireSphere(transform.position, _collectRadius);
            Gizmos.color = _gizmoCollectFill;
            Gizmos.DrawSphere(transform.position, _collectRadius);
        }
#endif
    }
}
