using System.Collections.Generic;
using UnityEngine;
using Wooduduk.Data.Core.TSVConvert;

namespace Wooduduk.Data.DataSO
{
    [CreateAssetMenu(fileName = "SkinData", menuName = "GameData/SkinData")]
    public class SkinDataSO : ScriptableObject, ISheetData
    {
        public SkinData _data;
        public GameObject skinPrefab;
        public string ID
        {
            get { if (_data == null) _data = new SkinData(); return _data._id; }
            set { if (_data == null) _data = new SkinData(); _data._id = value; }
        }

        public void SetData(Dictionary<string, string> dict)
        {
            if (_data == null) _data = new SkinData();

            _data._id = dict.ContainsKey("ID") ? dict["ID"] : "Unknown";
            _data._name = dict.ContainsKey("Name") ? dict["Name"] : "Unknown";
            _data._price = ParseInt(dict, "Price");
        }

        // 각 수치 파싱 (빈칸일 경우 0으로 처리하는 방어 코드 추가) int
        private int ParseInt(Dictionary<string, string> dict, string key)
        {
            return (dict.ContainsKey(key) && int.TryParse(dict[key], out int value)) ? value : 99999;
        }
    }
}