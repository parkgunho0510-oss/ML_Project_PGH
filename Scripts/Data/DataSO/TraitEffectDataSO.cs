using System.Collections.Generic;
using UnityEngine;
using Wooduduk.Data.Core;
using Wooduduk.Data.Core.TSVConvert;

namespace Wooduduk.Data.DataSO
{
    [CreateAssetMenu(fileName = "TraitEffectData", menuName = "GameData/TraitEffectData")]
    public class TraitEffectDataSO : ScriptableObject, ISheetData
    {
        public TraitEffectData _data;

        public string ID
        {
            get { if (_data == null) _data = new TraitEffectData(); return _data._id; }
            set { if (_data == null) _data = new TraitEffectData(); _data._id = value; }
        }

        public void SetData(Dictionary<string, string> dict)
        {
            if (_data == null) _data = new TraitEffectData();

            _data._id = dict.ContainsKey("ID") ? dict["ID"] : "Unknown";
            _data._name = dict.ContainsKey("Name") ? dict["Name"] : "Unknown";
            _data._traitId = dict.ContainsKey("TraitId") ? dict["TraitId"] : "";
            _data._level = ParseInt(dict, "Level");
            _data._unlockCost = ParseInt(dict, "UnlockCost");
            _data._maxHP = ParseFloat(dict, "MaxHp");
            _data._moveSpeed = ParseFloat(dict, "MoveSpeed");
            _data._attackDamage = ParseFloat(dict, "AttackDamage");
            _data._attackSpeed = ParseFloat(dict, "AttackSpeed");
            _data._attackRange = ParseFloat(dict, "AttackRange");
            _data._woodGainlue = ParseFloat(dict, "WoodGain");
            _data._woodHeal = ParseFloat(dict, "WoodHeal");
        }

        private int ParseInt(Dictionary<string, string> dict, string key)
        {
            return (dict.ContainsKey(key) && int.TryParse(dict[key], out int value)) ? value : 0;
        }

        private float ParseFloat(Dictionary<string, string> dict, string key)
        {
            return (dict.ContainsKey(key) && float.TryParse(dict[key], out float value)) ? value : 0f;
        }
    }
}
