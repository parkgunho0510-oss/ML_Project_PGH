using UnityEngine;

namespace Wooduduk.Vfx
{
    public class FootstepEmitter : MarkerEmitterBase
    {
        [Header("Footstep")]
        [SerializeField] private float _strideLength = 0.6f; // 이 거리마다 한 발
        [SerializeField] private float _footSpacing = 0.15f; // 좌우 발 간격
        [SerializeField] private float _groundY = 0.02f;     // 바닥에서 살짝 띄움
        [SerializeField] private float _minMoveSpeed = 0.1f; // 이 속도 미만이면 스킵

        private Vector3 _lastPos;
        private float _distanceAccum;
        private bool _leftFoot;

        private void Awake()
        {
            _lastPos = transform.position;
        }
        private void Update()
        {
            Vector3 pos = transform.position;
            Vector3 delta = pos - _lastPos;
            delta.y = 0f;
            _lastPos = pos;

            float dist = delta.magnitude;
            if (dist < _minMoveSpeed * Time.deltaTime) return;

            _distanceAccum += dist;
            if (_distanceAccum < _strideLength) return;
            _distanceAccum = 0f;

            Vector3 moveDir = delta.normalized;
            Vector3 right = Vector3.Cross(Vector3.up, moveDir);
            float side = _leftFoot ? -1f : 1f;
            _leftFoot = !_leftFoot;

            Vector3 footPos = pos + right * (_footSpacing * side);
            footPos.y = _groundY;

            Spawn(footPos, Quaternion.LookRotation(moveDir, Vector3.up));
        }


    }
}