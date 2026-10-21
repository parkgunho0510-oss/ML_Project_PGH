#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Wooduduk.Network;
using Wooduduk.Network.Match;

public class ServerMonitorWindow : EditorWindow
{
    private Vector2 _scroll;
    private string _pendingRemoveMatchId;
    private readonly Dictionary<string, bool> _matchFoldouts = new();
    private readonly Dictionary<string, bool> _playerFoldouts = new();

    [MenuItem("Tools/서버 모니터")]
    public static void Open()
    {
        // 기존에 열려있는 창이 있으면 가져오고, 없으면 새로 생성
        var window = GetWindow<ServerMonitorWindow>(false, "서버 모니터", true);
        window.Show();
    }

    private void Update()
    {
        if (Application.isPlaying)
            Repaint();
    }

    private void OnGUI()
    {
        GUILayout.Space(5);

        EditorGUILayout.LabelField(
            "게임 서버 모니터",
            EditorStyles.boldLabel);

        GUILayout.Space(10);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox(
                "플레이 모드에서만 사용 가능합니다.",
                MessageType.Info);
            return;
        }

        var server = GameServerManager.Instance;

        if (server == null)
        {
            EditorGUILayout.HelpBox(
                "GameServerManager를 찾을 수 없습니다.",
                MessageType.Warning);
            return;
        }

        DrawServerInfo(server);

        GUILayout.Space(5);

        DrawDebugButtons(server);

        GUILayout.Space(10);

        DrawMatches(server);

        if (!string.IsNullOrEmpty(_pendingRemoveMatchId))
        {
            server.DebugEndMatch(_pendingRemoveMatchId);
            _pendingRemoveMatchId = null;
        }
    }


    private void DrawServerInfo(GameServerManager server)
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField(
            "서버 상태",
            EditorStyles.boldLabel);

        EditorGUILayout.LabelField(
            "실행 여부",
            server.IsServerRunning.ToString());

        EditorGUILayout.LabelField(
            "Tick Rate",
            server.TickRate.ToString("F3"));

        if (server.MatchRegistry == null)
        {
            EditorGUILayout.HelpBox(
                "MatchRegistry 생성 대기중",
                MessageType.Info);

            EditorGUILayout.EndVertical();
            return;
        }

        int totalPlayers = 0;
        int totalBots = 0;
        int totalTrees = 0;

        foreach (var match in server.MatchRegistry.AllMatches.Values)
        {
            foreach (var player in match.Players.Values)
            {
                if (player._isBot)
                    totalBots++;
                else
                    totalPlayers++;
            }

            totalTrees += match.Trees.Count;
        }

        EditorGUILayout.LabelField(
            "세션 수",
            server.MatchRegistry.AllMatches.Count.ToString());

        EditorGUILayout.LabelField(
            "전체 플레이어",
            totalPlayers.ToString());

        EditorGUILayout.LabelField(
            "전체 봇",
            totalBots.ToString());

        EditorGUILayout.LabelField(
            "전체 나무",
            totalTrees.ToString());

        EditorGUILayout.EndVertical();
    }

    private void DrawDebugButtons(GameServerManager server)
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField(
            "디버그",
            EditorStyles.boldLabel);

        bool canUse = server.IsServerRunning;

        if (!canUse)
        {
            EditorGUILayout.HelpBox(
                "서버가 실행 중이 아닙니다.",
                MessageType.Warning);
        }

        GUI.enabled = canUse;

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("매치 생성", GUILayout.Height(30)))
        {
            server.DebugCreateMatch();
        }

        if (GUILayout.Button("전체 매치 삭제", GUILayout.Height(30)))
        {
            server.DebugRemoveAllMatches();
        }

        EditorGUILayout.EndHorizontal();

        GUI.enabled = true;

        EditorGUILayout.EndVertical();
    }

    private void DrawMatches(GameServerManager server)
    {
        if (server.MatchRegistry == null)
            return;

        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        var sessions =
            new List<MatchSession>(
                server.MatchRegistry.AllMatches.Values);

        foreach (var session in sessions)
        {
            DrawMatch(server, session);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawMatch(GameServerManager server, MatchSession session)
    {
        string matchId = session.State._matchId;

        if (!_matchFoldouts.ContainsKey(matchId))
            _matchFoldouts.Add(matchId, true);

        EditorGUILayout.BeginVertical("box");

        _matchFoldouts[matchId] =
            EditorGUILayout.Foldout(
                _matchFoldouts[matchId],
                $"매치 : {matchId}",
                true);

        if (_matchFoldouts[matchId])
        {
            DrawMatchInfo(session);

            GUILayout.Space(5);

            DrawMatchButtons(server, session);

            GUILayout.Space(5);

            DrawHumanPlayers(session);

            GUILayout.Space(5);

            DrawBots(server, session);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawMatchButtons(GameServerManager server, MatchSession session)
    {
        GUI.enabled = server.IsServerRunning;

        EditorGUILayout.BeginVertical("box");

        // ===== BOT =====
        EditorGUILayout.LabelField("봇 제어", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("봇 +1"))
            server.DebugAddBot(session.State._matchId);

        if (GUILayout.Button("봇 -1"))
            server.DebugRemoveBot(session.State._matchId);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("봇 +10"))
            server.DebugAddBots(session.State._matchId, 10);

        if (GUILayout.Button("봇 전체 제거"))
            server.DebugKillAllBots(session.State._matchId);

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        // ===== TREE =====
        EditorGUILayout.LabelField("나무 제어", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("나무 +1"))
            server.DebugSpawnTree(session.State._matchId);

        if (GUILayout.Button("나무 -1"))
            server.DebugRemoveTrees(session.State._matchId, 1);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("나무 +10"))
            server.DebugSpawnTrees(session.State._matchId, 10);

        if (GUILayout.Button("나무 -10"))
            server.DebugRemoveTrees(session.State._matchId, 10);

        if (GUILayout.Button("나무 +100"))
            server.DebugSpawnTrees(session.State._matchId, 100);

        if (GUILayout.Button("나무 -100"))
            server.DebugRemoveTrees(session.State._matchId, 100);

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        // ===== SESSION =====
        EditorGUILayout.LabelField("세션", EditorStyles.boldLabel);

        if (GUILayout.Button("세션 종료"))
        {
            _pendingRemoveMatchId = session.State._matchId;
        }

        EditorGUILayout.EndVertical();

        GUI.enabled = true;
    }

    private void DrawMatchInfo(MatchSession session)
    {
        EditorGUILayout.LabelField(
            "경과 시간",
            session.State._elapsedTime.ToString("F1"));

        EditorGUILayout.LabelField(
            "생존 플레이어",
            session.State._alivePlayerCount.ToString());

        EditorGUILayout.LabelField(
            "인간 플레이어",
            GetHumanCount(session).ToString());

        EditorGUILayout.LabelField(
            "봇",
            GetBotCount(session).ToString());

        EditorGUILayout.LabelField(
            "나무",
            session.Trees.Count.ToString());

        EditorGUILayout.LabelField(
            "존 반경",
            session.FireZone._currentRadius.ToString("F1"));

        EditorGUILayout.LabelField(
            "탈출 오픈",
            session.State._extractionOpened.ToString());

        EditorGUILayout.LabelField(
            "매치 종료",
            session.State._isEnded.ToString());
    }

    private void DrawHumanPlayers(MatchSession session)
    {
        EditorGUILayout.LabelField(
             $"플레이어 ({GetHumanCount(session)})",
            EditorStyles.boldLabel);

        foreach (var player in session.Players.Values)
        {
            if (player._isBot)
                continue;

            string key = player._userId;
            string shortId = player._userId.Length > 6 ? player._userId[..8] : player._userId;

            if (!_playerFoldouts.ContainsKey(key))
                _playerFoldouts[key] = false;

            EditorGUILayout.BeginHorizontal("helpbox");

            _playerFoldouts[key] = EditorGUILayout.Foldout(_playerFoldouts[key], $"{player._userNick} ({shortId})", true);

            if (_playerFoldouts[key])
            {
                GUILayout.Label(player._userNick, GUILayout.Width(100));
                GUILayout.Label($"HP {player._currentHp:F0}/{player._currentMaxHp:F0}", GUILayout.Width(100));
                GUILayout.Label($"Damage {player._currentAttackDamage:F0}", GUILayout.Width(100));
                GUILayout.Label($"Speed {player._currentAttackInterval:F0}", GUILayout.Width(100));
                GUILayout.Label($"Range {player._currentAttackRange:F0}", GUILayout.Width(100));
                GUILayout.Label($"Weapon {player._equippedWeaponId}", GUILayout.Width(100));
                GUILayout.Label($"Trait {player._equippedTraitId}", GUILayout.Width(100));
                GUILayout.Label($"Wood {player._collectedWood:F0}", GUILayout.Width(100));
                GUILayout.Label($"Speed {player._currentMoveSpeed:F1}", GUILayout.Width(100));
                GUILayout.Label($"Pos {player._position.x:F1}, {player._position.z:F1}", GUILayout.Width(100));
                GUILayout.Label(player._isDead ? "사망" : "생존", GUILayout.Width(100));
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawBots(GameServerManager server, MatchSession session)
    {
        EditorGUILayout.LabelField($"봇 ({GetBotCount(session)})", EditorStyles.boldLabel);

        int index = 1; // 봇 순번 표시용

        // 제거할 봇 ID를 저장할 임시 변수
        string botIdToRemove = null;

        foreach (var player in session.Players.Values)
        {
            if (!player._isBot) continue;

            EditorGUILayout.BeginHorizontal("helpbox");

            // 봇 순번 및 이름 표시
            GUILayout.Label($"BOT {index++}", GUILayout.Width(60));
            GUILayout.Label(player._userId.Substring(0, 8) + "...", GUILayout.Width(100)); // ID 일부만 표시

            // 상태 정보들
            GUILayout.Label($"HP {player._currentHp:F0}/{player._currentMaxHp:F0}", GUILayout.Width(100));
            GUILayout.Label($"Wood {player._collectedWood:F0}", GUILayout.Width(100));
            GUILayout.Label($"Pos {player._position.x:F0},{player._position.z:F0}", GUILayout.Width(100));
            GUILayout.Label(player._isDead ? "사망" : "생존", GUILayout.Width(100));
            // 개별 삭제 버튼
            GUI.color = Color.red;
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                botIdToRemove = player._userId;
            }
            GUI.color = Color.white;

            EditorGUILayout.EndHorizontal();
        }

        // 루프 밖에서 삭제 처리 (루프 중 컬렉션 수정 방지)
        if (botIdToRemove != null)
        {
            session.RemovePlayer(botIdToRemove, -1);
        }
    }

    private int GetHumanCount(MatchSession session)
    {
        int count = 0;

        foreach (var player in session.Players.Values)
        {
            if (!player._isBot)
                count++;
        }

        return count;
    }

    private int GetBotCount(MatchSession session)
    {
        int count = 0;

        foreach (var player in session.Players.Values)
        {
            if (player._isBot)
                count++;
        }

        return count;
    }
}

#endif