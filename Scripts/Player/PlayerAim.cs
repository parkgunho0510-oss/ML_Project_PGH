using UnityEngine;
using UnityEngine.InputSystem;

namespace Wooduduk.Player
{
    // 플레이어 시점 마우스 팔로우
    public class PlayerAim : MonoBehaviour
    {
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private Camera _cam;
        [SerializeField] private float _turnSpeed = 720f;

        private PlayerHealth _health;
        private Plane _ground;

        private void Awake()
        {
            if (_playerTransform == null) _playerTransform = transform;
            if (_cam == null) _cam = Camera.main;
            _ground = new Plane(Vector3.up, _playerTransform.position);

            _health = GetComponentInParent<PlayerHealth>();   // 사망 확인용
        }
        private void Update()
        {
            if (_health != null && !_health.IsAlive) return;
            if (_cam == null || Mouse.current == null) return;
            Ray ray = _cam.ScreenPointToRay(Mouse.current.position.ReadValue());

            _ground.SetNormalAndPosition(Vector3.up, _playerTransform.position);
            if (!_ground.Raycast(ray, out float enter)) return;

            Vector3 dir = ray.GetPoint(enter) - _playerTransform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) return;

            Quaternion target = Quaternion.LookRotation(dir, Vector3.up);
            _playerTransform.rotation = Quaternion.RotateTowards(_playerTransform.rotation, target, _turnSpeed * Time.deltaTime);

        }
    }
}