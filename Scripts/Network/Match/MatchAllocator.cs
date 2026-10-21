using Wooduduk.Data;
using Wooduduk.Data.Runtime;

namespace Wooduduk.Network.Match
{
    /// <summary>
    /// 매치 배정 시스템 (핵심)
    /// 로비에 있는 플레이어를 적절한 MatchSession에 넣어줌
    /// </summary>
    public class MatchAllocator
    {
        private readonly MatchRegistry _registry;

        public MatchAllocator(MatchRegistry registry)
        {
            _registry = registry;
        }

        /// <summary>
        /// 플레이어를 적절한 매치에 배정
        /// </summary>
        public MatchSession Allocate(int clientId, string uid, PlayerRuntimeData player, MatchConfigData config)
        {
            // 해당 매치의 최대 인원 제한
            int maxPlayers = config._playerCount;

            // 기존 세션 중 빈 자리 찾기
            foreach (var match in _registry.AllMatches.Values)
            {
                // 아직 인원이 덜 찬 세션이면 여기에 넣음
                if (match.Players.Count < maxPlayers)
                {
                    RegisterPlayer(match, clientId, uid, player);
                    return match;
                }
            }
            // 모든 세션이 꽉 찼으면 새 RAID 생성
            var newMatch = _registry.CreateMatch(config);

            RegisterPlayer(newMatch, clientId, uid, player);
            return newMatch;
        }

        private void RegisterPlayer(MatchSession match, int clientId, string uid, PlayerRuntimeData player)
        {
            match.AddPlayer(uid, player);
            match.ClientToUser[clientId] = uid;
            match.UserToClient[uid] = clientId;
        }

        /// <summary>
        /// clientId → session 역추적
        /// </summary>
        public MatchSession GetSessionByClient(int clientId)
        {
            foreach (var match in _registry.AllMatches.Values)
            {
                if (match.ClientToUser.ContainsKey(clientId))
                    return match;
            }
            return null;
        }
    }
}