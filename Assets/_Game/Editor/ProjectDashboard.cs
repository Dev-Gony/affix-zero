using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using AffixZero.Presentation;

namespace AffixZero.Editor
{
    public sealed class ProjectDashboard : EditorWindow
    {
        private const string Version = "6000.3.24f1";
        private ActorAnimationSet hero;
        private ActorAnimationSet enemy;
        private Sprite ground;
        private bool licenseReviewed;
        private Vector2 scroll;

        [MenuItem("AFFIX/Project Dashboard")]
        public static void Open() => GetWindow<ProjectDashboard>("AFFIX Project");

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField("Unity restart / U1 preparation", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("No old Godot code or artwork is used. A playable scene requires approved, real frame assets. This dashboard is a setup tool, not the game.", MessageType.Info);
            EditorGUILayout.LabelField("Expected editor", Version);
            EditorGUILayout.LabelField("Running editor", Application.unityVersion);
            EditorGUILayout.SelectableLabel(EditorApplication.applicationPath, GUILayout.Height(34));
            if (Application.unityVersion != Version)
                EditorGUILayout.HelpBox("Editor version differs. Do not silently upgrade project settings; record the version first.", MessageType.Warning);
            if (GUILayout.Button("Export setup report")) ExportReport();
            if (GUILayout.Button("Open current documentation")) EditorUtility.RevealInFinder(Path.GetFullPath("docs/README.md"));
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("First encounter assets", EditorStyles.boldLabel);
            hero = (ActorAnimationSet)EditorGUILayout.ObjectField("Hero animation set", hero, typeof(ActorAnimationSet), false);
            enemy = (ActorAnimationSet)EditorGUILayout.ObjectField("Enemy animation set", enemy, typeof(ActorAnimationSet), false);
            ground = (Sprite)EditorGUILayout.ObjectField("Licensed ground tile", ground, typeof(Sprite), false);
            licenseReviewed = EditorGUILayout.ToggleLeft("Source license / public redistribution and frame poses reviewed", licenseReviewed);
            string issue = ReadinessIssue();
            EditorGUILayout.HelpBox(issue ?? "Data checks passed. This does not certify art quality or gameplay.", issue == null ? MessageType.Info : MessageType.Warning);
            using (new EditorGUI.DisabledScope(issue != null || EditorApplication.isPlaying))
            {
                if (GUILayout.Button("Create first encounter scene (no overwrite)")) CreateEncounter();
            }
            EditorGUILayout.EndScrollView();
        }

        private string ReadinessIssue()
        {
            if (Application.unityVersion != Version) return "Use the project editor version or approve a recorded version change first.";
            if (hero == null || enemy == null || ground == null) return "Waiting for licensed hero, enemy and ground assets. No substitute art will be generated.";
            string issue = hero.ValidateSet() ?? enemy.ValidateSet();
            if (issue != null) return issue;
            if (ground.texture == null) return "Ground texture is missing.";
            if (ground.bounds.size.x <= 0 || ground.bounds.size.y <= 0) return "Ground sprite dimensions are invalid.";
            if (!licenseReviewed) return "Complete source/license and visual frame review first.";
            return null;
        }

        private void CreateEncounter()
        {
            string issue = ReadinessIssue();
            if (issue != null) { Debug.LogError(issue); return; }
            const string path = "Assets/_Game/Scenes/FirstEncounter.unity";
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(path)) { EditorSceneManager.OpenScene(path); return; }
            if (!AssetDatabase.IsValidFolder("Assets/_Game/Scenes")) AssetDatabase.CreateFolder("Assets/_Game", "Scenes");
            EditorSettings.serializationMode = SerializationMode.ForceText;
            EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Camera camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0, 0, -10);
            camera.orthographic = true;
            camera.orthographicSize = 4.2f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.84f, 0.93f, 0.79f);
            Transform world = new GameObject("Field").transform;
            for (int y = -4; y < 4; y++)
            for (int x = -7; x < 7; x++)
            {
                SpriteRenderer tile = new GameObject("Ground", typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
                tile.transform.SetParent(world);
                tile.transform.position = new Vector3(x + 0.5f, y + 0.5f, 0);
                tile.transform.localScale = new Vector3(1 / ground.bounds.size.x, 1 / ground.bounds.size.y, 1);
                tile.sprite = ground;
                tile.sortingOrder = -10000;
            }
            MeleeActor player = MakeActor("Hero", hero, 1, 120, 30, new Vector3(-2.5f, 0, 0));
            MeleeActor opponent = MakeActor("Melee Enemy", enemy, 2, 80, 10, new Vector3(2.5f, 0, 0));
            player.SetTarget(opponent);
            opponent.SetTarget(player);
            new GameObject("Encounter", typeof(FirstEncounter)).GetComponent<FirstEncounter>().Configure(player, opponent);
            if (!EditorSceneManager.SaveScene(scene, path)) throw new IOException("Could not save first encounter.");
            if (!EditorBuildSettings.scenes.Any(s => s.path == path))
                EditorBuildSettings.scenes = EditorBuildSettings.scenes.Concat(new[] { new EditorBuildSettingsScene(path, true) }).ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("FirstEncounter created using selected assets. Unity Play and visual acceptance are still required.");
        }

        private static MeleeActor MakeActor(string name, ActorAnimationSet set, int id, int hp, int power, Vector3 position)
        {
            var go = new GameObject(name, typeof(SpriteRenderer), typeof(MeleeActor));
            go.transform.position = position;
            go.GetComponent<SpriteRenderer>().sprite = set.Frame(ActorClip.Idle, 0);
            var actor = go.GetComponent<MeleeActor>();
            actor.Configure(set, id, hp, power);
            return actor;
        }

        [Serializable]
        private sealed class SetupReport
        {
            public string schema = "affix-unity-setup-v1";
            public string recordedUtc;
            public string expectedEditor;
            public string actualEditor;
            public string editorPath;
            public string projectPath;
            public string[] animationSets;
            public string[] animationDataProblems;
            public string sourceBranch = "restart/unity-6";
            public string unityEditorImport = "DASHBOARD_LOADED";
            public string gameplay = "NOT_TESTED_BY_THIS_REPORT";
            public string storageDeletion = "NOT_PERFORMED";
        }
        public static void ExportReport()
        {
            string[] paths = AssetDatabase.FindAssets("t:ActorAnimationSet").Select(AssetDatabase.GUIDToAssetPath).ToArray();
            var report = new SetupReport {
                recordedUtc = DateTime.UtcNow.ToString("O"), expectedEditor = Version,
                actualEditor = Application.unityVersion, editorPath = EditorApplication.applicationPath,
                projectPath = Path.GetFullPath("."), animationSets = paths,
                animationDataProblems = paths.Select(p => p + ": " + (AssetDatabase.LoadAssetAtPath<ActorAnimationSet>(p).ValidateSet() ?? "DATA_OK_VISUAL_REVIEW_PENDING")).ToArray()
            };
            Directory.CreateDirectory("Build/Reports");
            string output = Path.GetFullPath("Build/Reports/unity-setup-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff") + ".json");
            File.WriteAllText(output, JsonUtility.ToJson(report, true));
            EditorUtility.RevealInFinder(output);
            Debug.Log("Setup report saved: " + output + ". Review local paths before sharing.");
        }
    }
}
