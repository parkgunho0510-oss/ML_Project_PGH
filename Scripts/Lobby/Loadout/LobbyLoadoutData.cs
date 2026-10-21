using System;
using System.Collections.Generic;

namespace Wooduduk.Lobby
{
    [Serializable]
    public class LobbyLoadoutData
    {
        public AxeDataSO selectedAxe;
        public List<LobbyTraitDataSO> selectedTraits = new List<LobbyTraitDataSO>();

        public LobbyLoadoutData()
        {
        }

        public LobbyLoadoutData(AxeDataSO axe, List<LobbyTraitDataSO> traits)
        {
            selectedAxe = axe;
            selectedTraits = new List<LobbyTraitDataSO>(traits);
        }
    }
}
