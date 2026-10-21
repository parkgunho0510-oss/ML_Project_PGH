using Unity.Cinemachine;
using FishNet.Object;
using UnityEngine;

namespace Wooduduk.Core
{
    internal class PlayerCamera : NetworkBehaviour
    {
        public override void OnStartClient()
        {
            base.OnStartClient();
            if (!IsOwner) return;

            var cam = FindFirstObjectByType<CinemachineCamera>();
            if (cam == null) return;

            cam.Follow = transform;
        }
    }
}
