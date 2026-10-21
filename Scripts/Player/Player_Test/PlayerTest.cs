// ── (신규) PlayerTestBootstrap.cs ── SO 없이 즉시 테스트 (Play 누르면 콘솔에 결과)
using System.Collections.Generic;
using UnityEngine;
using Wooduduk.Combat;
using Wooduduk.Core;
using Wooduduk.Data;
using Wooduduk.Data.Core;

namespace Wooduduk.Player
{
    [DefaultExecutionOrder(-100)] 
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerTestBootstrap : MonoBehaviour
    {
        [Header("캐릭터")]
        [SerializeField] private float _maxHp = 100, _moveSpeed = 5, _defense = 10;
        [Header("무기")]
        [SerializeField] private float _atkDamage = 25, _atkInterval = 0.5f, _atkRange = 2.5f, _hpBonus = 20;
        [Header("특성")]
        [SerializeField] private StatType _traitStat = StatType.ATTACKDAMAGE;
        [SerializeField] private float _traitPercent = 0.3f;     // +30%
        [Header("피격 테스트")]
        [SerializeField] private int _rawDamage = 30;

        private PlayerStats _stats;

        private void Awake()
        {
            _stats = GetComponent<PlayerStats>();

            var character = new CharacterData
            {
                _id = "CH_TEST",
                _name = "TestPlayer",
                _maxHp = _maxHp,
                _moveSpeed = _moveSpeed,
                _defense = _defense
            };
            var weaponData = new WeaponData
            {
                _id = "WP_TEST",
                _name = "TestWeapon",
                _attackDamage = _atkDamage,
                _attackInterval = _atkInterval,
                _attackRange = _atkRange,
                _maxHpBonus = _hpBonus
            };
            IWeapon weapon = new WeaponRuntime(weaponData);

            _stats.Initialize(new LoadoutBaseStats(character, weapon));   // 베이스 주입
            Log("베이스");

            var traitData = new TraitData { _id = "TR_TEST", _name = "테스트특성", _maxLevel = 1 };
            var effect = new TraitEffectData { _id = "TE_TEST", _traitId = "TR_TEST", _level = 1, _stat = _traitStat, _value = _traitPercent };
            ITrait trait = new PlayerTrait(traitData, new List<TraitEffectData> { effect }, 1);
            trait.Apply(_stats, this);                                   // 특성 적용
            Log($"특성({_traitStat} +{_traitPercent * 100:F0}%)");
        }

        private void Start()
        {
            if (TryGetComponent<PlayerHealth>(out var hp))
                hp.TakeDamage(_rawDamage, gameObject);                  // 방어 감산 확인
        }

        private void Log(string tag) => Debug.Log($"[Test:{tag}] DMG {_stats.SwingDamage:F1} / RANGE {_stats.SwingRange:F1} / SPD {_stats.SwingSpeed:F2} / HP {_stats.MaxHealth} / DEF {_stats.Defense:F0} / MOVE {_stats.MoveSpeed:F1}");
    }
}