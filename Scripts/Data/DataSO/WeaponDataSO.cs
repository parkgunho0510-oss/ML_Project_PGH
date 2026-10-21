using System.Collections.Generic;
using UnityEngine;
using Wooduduk.Data.Core.TSVConvert;

namespace Wooduduk.Data.DataSO
{
    [CreateAssetMenu(fileName = "WeaponData", menuName = "GameData/WeaponData")]
    public class WeaponDataSO : ScriptableObject, ISheetData
    {
        public WeaponData _data;

        public string ID
        {
            get { if (_data == null) _data = new WeaponData(); return _data._id; }
            set { if (_data == null) _data = new WeaponData(); _data._id = value; }
        }

        public void SetData(Dictionary<string, string> dict)
        {
            if (_data == null) _data = new WeaponData();

            _data._id = dict.ContainsKey("ID") ? dict["ID"] : "Unknown";
            _data._name = dict.ContainsKey("Name") ? dict["Name"] : "Unknown";
            _data._attackDamage = ParseFloat(dict, "AttackDamage");
            _data._attackInterval = ParseFloat(dict, "AttackInterval");
            _data._attackRange = ParseFloat(dict, "AttackRange");
            _data._maxHpBonus = ParseFloat(dict, "MaxHpBonus");
            _data._unlockCost = ParseInt(dict, "UnlockCost");
            _data._isDefault = ParseBool(dict, "IsDefault");
        }

        private float ParseFloat(Dictionary<string, string> dict, string key)
        {
            return (dict.ContainsKey(key) && float.TryParse(dict[key], out float value)) ? value : 0f;
        }

        private int ParseInt(Dictionary<string, string> dict, string key)
        {
            return (dict.ContainsKey(key) && int.TryParse(dict[key], out int value)) ? value : 0;
        }

        private bool ParseBool(Dictionary<string, string> dict, string key)
        {
            return dict.ContainsKey(key) && bool.TryParse(dict[key], out bool value) && value;
        }
    }
}
