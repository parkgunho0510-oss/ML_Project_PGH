using System.Collections.Generic;
using UnityEngine;
using Wooduduk.Data.Core.TSVConvert;

namespace Wooduduk.Data.DataSO
{
    [CreateAssetMenu(fileName = "TraitData", menuName = "GameData/TraitData")]
    public class TraitDataSO : ScriptableObject, ISheetData
    {
        public TraitData _data;

        public string ID
        {
            get { if (_data == null) _data = new TraitData(); return _data._id; }
            set { if (_data == null) _data = new TraitData(); _data._id = value; }
        }

        public void SetData(Dictionary<string, string> dict)
        {
            if (_data == null) _data = new TraitData();

            _data._id = dict.ContainsKey("ID") ? dict["ID"] : "Unknown";
            _data._name = dict.ContainsKey("Name") ? dict["Name"] : "Unknown";
            _data._maxLevel = ParseInt(dict, "MaxLevel");
            _data._activeSkillId = dict.ContainsKey("ActiveSkillId") ? dict["ActiveSkillId"] : "";
        }

        private int ParseInt(Dictionary<string, string> dict, string key)
        {
            return (dict.ContainsKey(key) && int.TryParse(dict[key], out int value)) ? value : 0;
        }
    }
}
