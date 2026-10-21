
using System;
using UnityEngine;

namespace Wooduduk.Lobby
{
    [Serializable]
    public class StatModifier
    {
        [SerializeField] private StatType statType;
        [SerializeField] private int value;

        public StatType StatType => statType;
        public int Value => value;
    }
}

