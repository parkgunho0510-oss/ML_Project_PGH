using UnityEngine;

namespace Wooduduk.Data.Runtime
{
    public class PlayerRuntimeData
    {
        // 유저 식별자(Firebase UID)
        public string _userId;
        public string _userNick;
        // 캐릭터 정보
        public string _characterDataId;
        // 위치
        public Vector3 _position;
        //변화 수치
        public float _currentMaxHp;
        public float _currentHp;
        public float _currentAttackDamage;
        public float _currentAttackRange;
        public float _currentAttackInterval;
        public float _currentDefense;
        public float _currentMoveSpeed;
        // 특성 무기(게임 시작 전 세팅, 현재 특성, 무기)
        public string _equippedTraitId;
        public string _equippedWeaponId;
        // 모은 재화, 킬, 생존여부
        public int _collectedWood;
        public int _killCount;
        public bool _isExtracted;
        public bool _isDead;
        public bool _isBot;
    }
}
