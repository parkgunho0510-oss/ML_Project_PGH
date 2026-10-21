using System.Collections.Generic;
using UnityEngine;
using Wooduduk.AI.EnemyAI.Training;
using static Unity.VisualScripting.Member;

namespace Wooduduk.Core
{
    public class ExitPoint : MonoBehaviour
    {
        // 맵에 있는 모든 출구를 담아둘 전역 리스트
        public static readonly List<ExitPoint> AllExits = new();

        private void OnEnable() => AllExits.Add(this);
        private void OnDisable() => AllExits.Remove(this);

        private void OnTriggerEnter(Collider other)
        {
            var agent = other.GetComponentInChildren<FarmingAgent>();
            if (agent != null)
            {
                agent.OnExitReached();
            }

        }
    }
}