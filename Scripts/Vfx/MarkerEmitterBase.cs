using UnityEngine;
using Wooduduk.Core;

namespace Wooduduk.Vfx
{
    public abstract class MarkerEmitterBase : MonoBehaviour
    {
        [Header("Marker")]
        [SerializeField] protected GameObject _markerPrefab;  

        // 월드 위치/회전에 마커 배치
        protected void Spawn(Vector3 position, Quaternion rotation)
        {
            if (_markerPrefab == null || GenericObjectPool.Instance == null) return;
            GenericObjectPool.Instance.Spawn(_markerPrefab, position, rotation);
        }
    }
}