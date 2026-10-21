using System.Collections.Generic;

namespace Wooduduk.Network.Match
{
    /// <summary>
    /// 대기열 시스템
    /// 순차적으로 처리됨
    /// </summary>
    public class LobbyQueue
    {
        /// <summary>
        /// 대기열 (FIFO 구조: 먼저 들어온 사람이 먼저 처리됨)
        /// </summary>
        private readonly Queue<(int clientId, string uid)> _queue = new();

        /// <summary>
        /// 플레이어를 대기열에 추가
        /// 매칭 시스템으로 넘기기 전에 들어옴
        /// </summary>
        public void Enqueue(int clientId, string uid)
        {
            _queue.Enqueue((clientId, uid));
        }

        /// <summary>
        /// 대기열에서 한 명 꺼내기
        /// </summary>
        public bool TryDequeue(out (int clientId, string uid) data)
        {
            if (_queue.Count > 0)
            {
                data = _queue.Dequeue();
                return true;
            }

            data = default;
            return false;
        }

        /// <summary>
        /// 현재 로비 대기 인원 수
        /// → 매칭 상태 UI, 서버 상태 확인용
        /// </summary>
        public int Count => _queue.Count;
    }
}