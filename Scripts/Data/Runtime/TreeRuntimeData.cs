using UnityEngine;

namespace Wooduduk.Data.Runtime
{
    public class TreeRuntimeData
    {
        // 나무 정보
        public string _treeInstanceId;
        public Vector3 _position;
        public float _currentMaxHp;
        public float _currentHp;
        public float _currentMoveSpeed;
        public int _currentWoodAmount;

        // 나무 파괴 생성
        public bool _isDestroyed;
    }
}
