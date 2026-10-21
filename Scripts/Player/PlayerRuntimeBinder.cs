using Wooduduk.Data.Runtime;
using UnityEngine;

namespace Wooduduk.Player
{
    // 스ㅡ폰 시 단일 진입점
    // 세션 / 스폰 코드는 이 메서드 호출하면 됨
    public class PlayerRuntimeBinder : MonoBehaviour
    {
        private PlayerStats _stats;
        private WoodBag _bag;
        public PlayerRuntimeData Runtime { get; private set;  }

        private void Awake()
        {
            _stats = GetComponent<PlayerStats>();
            _bag = GetComponent<WoodBag>();
        }
        public void Bind(PlayerRuntimeData runtime)
        {
            if (runtime == null)
            {
                Debug.LogWarning("runtime null");
                return;
            }
            // 서버 권위
            var nb = GetComponent<FishNet.Object.NetworkBehaviour>();
            if (nb != null && !nb.IsServerInitialized)
            {
                return;
            }
            Runtime = runtime;
            _stats?.Initialize(new PlayerRuntimeBaseStats(runtime));
        }
    }
}
