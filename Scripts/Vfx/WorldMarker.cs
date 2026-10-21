using System;
using UnityEngine;
using Wooduduk.Core;

namespace Wooduduk.Vfx
{
    public class WorldMarker : MonoBehaviour, IPoolable
    {
        [SerializeField] private float _lifetime = 300f;
        [SerializeField] private float _fadeDuration = 1f;
        [SerializeField] private string _colorProperty = "_BaseColor";

        private Renderer _renderer;
        private MaterialPropertyBlock _mpb;
        private int _colorId;
        private Color _baseColor;
        private float _age;
        private Action<GameObject> _returnToPool;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _mpb = new MaterialPropertyBlock();
            _colorId = Shader.PropertyToID(_colorProperty);
            _baseColor = _renderer.sharedMaterial.HasProperty(_colorId)
                ? _renderer.sharedMaterial.GetColor(_colorId) : Color.white;
        }
        private void Update()
        {
            _age += Time.deltaTime;
            if (_age < _lifetime) return;

            float t = (_age - _lifetime) / Mathf.Max(0.01f, _fadeDuration);
            if (t >= 1f)
            {
                _returnToPool?.Invoke(gameObject);
                return;
            }
            SetAlpha(Mathf.Lerp(_baseColor.a, 0f, t));
        }
        // 풀 생성 시 1회 자신을 풀로 되돌리는 델리게이트 주입
        public void SetupPoolReturn(Action<GameObject> returnAction) => _returnToPool = returnAction;

        // 풀에서 꺼낼 때
        public void OnSpawn()
        {
            _age = 0f;
            SetAlpha(_baseColor.a);
        }
        public void OnReturn() { }
        private void SetAlpha(float a)
        {
            _renderer.GetPropertyBlock(_mpb);
            Color c = _baseColor;
            c.a = a;
            _mpb.SetColor(_colorId, c);
            _renderer.SetPropertyBlock(_mpb);
        }
    }
}