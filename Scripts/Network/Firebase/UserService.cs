using System.Collections.Generic;
using Wooduduk.Data;

namespace Wooduduk.Network.Firebase
{
    public class UserService
    {
        // 신규 유저 생성 (게임 규칙)
        public UserData CreateUserData(string userId)
        {
            var user = new UserData
            {
                _userId = userId,
                _userNick = "NewPlayer",
                _rankPoint = 0,
                _totalWood = 0,
                _jamCount = 0,

                _ownedSkinIds = new List<string>(),
                _selectedSkinId = "",

                _traitLevels = new List<TraitProgress>(),
                _equippedTraitId = "",

                _ownedWeaponIds = new List<string> { "W001" },
                _equippedWeaponId = "W001"
            };

            return user;
        }
    }
}
