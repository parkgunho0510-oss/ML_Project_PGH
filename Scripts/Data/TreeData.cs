using UnityEngine;

namespace Wooduduk.Data
{
    [System.Serializable]
    public class TreeData
    {
        public string _id;
        public string _name;
        public int _maxHp;
        [Range(1, 10)]
        public int _woodAmount;     // 캐면 얻는 장작 양
        public float _moveSpeed;    // 0이면 고정 나무, >0이면 도망가는 나무 정령 등의 이동 속도
    }
}