using System.Collections.Generic;
using UnityEngine;
using Wooduduk.Data.Core.TSVConvert;

namespace Wooduduk.Data.DataSO
{
    [CreateAssetMenu(fileName = "MatchData", menuName = "GameData/MatchData")]
    public class MatchDataSO : ScriptableObject, ISheetData
    {
        // 데이터 보관용 필드
        public MatchConfigData _data;

        // ISheetData 인터페이스 구현
        public string ID
        {
            get { if (_data == null) _data = new MatchConfigData(); return _data._id; }
            set { if (_data == null) _data = new MatchConfigData(); _data._id = value; }
        }

        public void SetData(Dictionary<string, string> dict)
        {
            if (_data == null) _data = new MatchConfigData();

            _data._id = dict.ContainsKey("ID") ? dict["ID"] : "Unknown";
            _data._name = dict.ContainsKey("Name") ? dict["Name"] : "Unknown";

            _data._sessionTime = ParseFloat(dict, "SessionTime");
            _data._playerCount = ParseInt(dict, "PlayerCount");
            _data._extractionOpenTime = ParseFloat(dict, "ExtractionOpenTime");
            _data._readyCountdown = ParseFloat(dict, "ReadyCountdown");
            _data._tickInterval = ParseFloat(dict, "TickInterval");
            _data._mapRadius = ParseFloat(dict, "MapRadius");
            _data._timeOverDamage = ParseFloat(dict, "TimeOverDamage");
        }

        // 빈칸일 경우 0으로 처리하는 방어 코드
        private float ParseFloat(Dictionary<string, string> dict, string key)
        {
            return (dict.ContainsKey(key) && float.TryParse(dict[key], out float value)) ? value : 0f;
        }

        private int ParseInt(Dictionary<string, string> dict, string key)
        {
            return (dict.ContainsKey(key) && int.TryParse(dict[key], out int value)) ? value : 0;
        }
    }
}
