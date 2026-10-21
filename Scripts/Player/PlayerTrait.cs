using System.Collections.Generic;
using Wooduduk.Core;
using Wooduduk.Data;
using Wooduduk.Data.Core;

namespace Wooduduk.Player
{
    public class PlayerTrait : ITrait
    {
        private readonly TraitData _data;
        private readonly List<TraitEffectData> _activeEffects = new();

        public string TraitID => _data._id;
        public string DisplayName => _data._name;

        public PlayerTrait(TraitData data, IEnumerable<TraitEffectData> allEffects, int unlockedLevel)
        {
            _data = data;
            foreach (var e in allEffects)
            {
                if (e != null && e._traitId == data._id && e._level <= unlockedLevel)
                    _activeEffects.Add(e);
            }
        }
        public void Apply(IPlayerStats stats, object source)
        {
            if (stats == null) return;
            foreach (var e in _activeEffects)
            {
                if (TryMap(e._stat, out StatId id))
                {
                    stats.AddModifier(id, new StatModifier { percent = e._value, source = source });
                }
            }
        }
        public void Remove(IPlayerStats stats, object source)
        {
            if (stats == null) return;
            foreach (var e in _activeEffects)
            {
                if (TryMap(e._stat, out StatId id))
                {
                    stats.RemoveModifier(id, new StatModifier { percent = e._value, source = source});
                }
            }
        }
        private static bool TryMap(StatType t, out StatId id)
        {
            switch (t)
            {
                case StatType.MAXHP:            id = StatId.MAX_HEALTH; return true;
                case StatType.ATTACKDAMAGE:     id = StatId.SWING_DAMAGE; return true;
                case StatType.ATTACKRANGE:      id = StatId.SWING_RANGE; return true;
                case StatType.ATTACKINTERVAL:   id = StatId.SWING_SPEED; return true; 
                case StatType.MOVESPEED:        id = StatId.MOVE_SPEED; return true;
                case StatType.DEFENSE:          id = StatId.DEFENSE; return true;
                default:                        id = default; return false;
            }
        }

    }
}
