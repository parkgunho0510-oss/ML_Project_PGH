using System.Collections.Generic;
using UnityEngine;
using Wooduduk.Data.Core.TSVConvert;

namespace Wooduduk.Data.DataSO
{
    [CreateAssetMenu(fileName = "ZoneData", menuName = "GameData/ZoneData")]
    public class ZoneDataSO : ScriptableObject, ISheetData
    {
        // 데이터 보관용 필드
        public ZoneData _data;

        // ISheetData 인터페이스 구현
        public string ID
        {
            get { if (_data == null) _data = new ZoneData(); return _data._id; }
            set { if (_data == null) _data = new ZoneData(); _data._id = value; }
        }

        public void SetData(Dictionary<string, string> dict)
        {
            if (_data == null) _data = new ZoneData();

            _data._id = dict.ContainsKey("ID") ? dict["ID"] : "Unknown";
            _data._name = dict.ContainsKey("Name") ? dict["Name"] : "Unknown";

            _data._startTime = ParseFloat(dict, "StartTime");
            _data._shrinkDuration = ParseFloat(dict, "ShrinkDuration");
            _data._targetRatio = ParseFloat(dict, "TargetRatio");
            _data._outDamage = ParseFloat(dict, "OutDamage");
        }

        // 빈칸일 경우 0으로 처리하는 방어 코드
        private float ParseFloat(Dictionary<string, string> dict, string key)
        {
            return (dict.ContainsKey(key) && float.TryParse(dict[key], out float value)) ? value : 0f;
        }
    }
}
