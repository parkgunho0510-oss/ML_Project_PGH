namespace Wooduduk.Data
{
    [System.Serializable]
    public class CharacterData
    {
        public string _id;
        public string _name;
        public float _maxHp;
        public float _defense;
        public float _moveSpeed;

        // 공격 능력치(데미지/간격/사거리)는 무기(WeaponData)에서 가져옴
    }
}