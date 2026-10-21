using System;
using System.Collections;
using UnityEngine;
using Wooduduk.Core;

namespace Wooduduk.Player
{
    [RequireComponent(typeof(ParticleSystem))]
    public class OneShotVFX : MonoBehaviour, IPoolable
    {
        [SerializeField] private float _lifetime = 2f;  // TODO : Data SO로 분리
        private ParticleSystem _ps;
        private Action<GameObject> _returnToPool;

        private void Awake() => _ps = GetComponent<ParticleSystem>();

        public void SetupPoolReturn(Action<GameObject> returnAction) => _returnToPool = returnAction;

        public void OnSpawn()
        {
            _ps.Play();
            StartCoroutine(ReturnRoutine());
        }

        public void OnReturn()
        {
            StopAllCoroutines();
            _ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private IEnumerator ReturnRoutine()
        {
            yield return new WaitForSeconds(_lifetime);

            if (_returnToPool != null) _returnToPool.Invoke(gameObject);
            else gameObject.SetActive(false);
        }
    }
}