using System.Linq;
using UnityEngine;
using Wooduduk.Network;

namespace Wooduduk.Core
{
    public class DebugRuntimeUI : MonoBehaviour
    {
        private string GetMatchId()
        {
            var match = GameServerManager.Instance?
                .MatchRegistry?
                .AllMatches?
                .Values
                .FirstOrDefault();

            return match?.State._matchId;
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
                return;

            string matchId = GetMatchId();

            if (string.IsNullOrEmpty(matchId))
                return;

            float width = 200f;
            float height = 250f;

            GUILayout.BeginArea(
                new Rect(
                    Screen.width - width - 10f,
                    10f,
                    width,
                    height),
                GUI.skin.box);

            GUILayout.Label("Debug");

            if (GUILayout.Button("봇 +1"))
                GameServerManager.Instance.DebugAddBot(matchId);

            if (GUILayout.Button("봇 -1"))
                GameServerManager.Instance.DebugRemoveBot(matchId);

            GUILayout.Space(10);

            if (GUILayout.Button("나무 +10"))
                GameServerManager.Instance.DebugSpawnTrees(matchId, 10);

            if (GUILayout.Button("나무 -10"))
                GameServerManager.Instance.DebugRemoveTrees(matchId, 10);

            GUILayout.Space(5);

            if (GUILayout.Button("나무 +100"))
                GameServerManager.Instance.DebugSpawnTrees(matchId, 100);

            if (GUILayout.Button("나무 -100"))
                GameServerManager.Instance.DebugRemoveTrees(matchId, 100);

            GUILayout.EndArea();
        }
    }
}