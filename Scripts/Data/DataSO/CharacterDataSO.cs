using Wooduduk.Data.Core.TSVConvert;
using System.Collections.Generic;
using UnityEngine;

namespace Wooduduk.Data.DataSO
{
    [CreateAssetMenu(fileName = "CharacterData", menuName = "GameData/CharacterData")]
    public class CharacterDataSO : ScriptableObject, ISheetData
    {
        // 데이터 보관용 필드
        public CharacterData _data;

        // ISheetData 인터페이스 구현
        public string ID
        {
            get { if (_data == null) _data = new CharacterData(); return _data._id; }
            set { if (_data == null) _data = new CharacterData(); _data._id = value; }
        }

        public void SetData(Dictionary<string, string> dict)
        {
            if (_data == null) _data = new CharacterData();

            _data._id = dict.ContainsKey("ID") ? dict["ID"] : "Unknown";
            _data._name = dict.ContainsKey("Name") ? dict["Name"] : "Unknown";
            // 모든 수치에 방어 코드 적용
            _data._maxHp = ParseFloat(dict, "MaxHp");
            _data._defense = ParseFloat(dict, "Defense");
            _data._moveSpeed = ParseFloat(dict, "MoveSpeed");
        }

        // 각 수치 파싱 (빈칸일 경우 0으로 처리하는 방어 코드 추가) float
        private float ParseFloat(Dictionary<string, string> dict, string key)
        {
            return (dict.ContainsKey(key) && float.TryParse(dict[key], out float value)) ? value : 0f;
        }
    }
}