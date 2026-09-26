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

        private void OnEnable()
        {
            hero = AssetDatabase.LoadAssetAtPath<ActorAnimationSet>(ReviewedArtSetup.DataRoot + "Hero.asset");
            enemy = AssetDatabase.LoadAssetAtPath<ActorAnimationSet>(ReviewedArtSetup.DataRoot + "Enemy.asset");
            ground = ReviewedArtSetup.EnvironmentSpriteNamed("Grass");
        }

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
            if (GUILayout.Button("Import reviewed Ninja Adventure art and create encounter")) ReviewedArtSetup.Build();
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
            CreateReviewedEncounter(hero, enemy, ground);
        }

        public static void CreateReviewedEncounter(ActorAnimationSet hero, ActorAnimationSet enemy, Sprite ground)
        {
            if (Application.unityVersion != Version) throw new InvalidOperationException("Unexpected editor version.");
            if (hero == null || enemy == null || ground == null) throw new InvalidOperationException("Reviewed assets are required.");
            string problem = hero.ValidateSet() ?? enemy.ValidateSet();
            if (problem != null) throw new InvalidOperationException(problem);
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
            // MSAA samples outside atlas rectangles at subpixel tile boundaries.
            // Point-filtered pixel sprites must not borrow pixels from neighboring tiles.
            camera.allowMSAA = false;
            camera.transparencySortMode = UnityEngine.TransparencySortMode.CustomAxis;
            camera.transparencySortAxis = Vector3.up;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.84f, 0.93f, 0.79f);
            Transform world = new GameObject("Field").transform;
            for (int y = -6; y < 6; y++)
            for (int x = -12; x < 12; x++)
            {
                string name = y == 1 ? "PathTop" : y == 0 ? "Dirt" : y == -1 ? "PathBottom"
                    : (x * x + y * y) % 11 == 0 ? "GrassDetail" : (x - y + 30) % 5 == 0 ? "GrassTufts" : "Grass";
                Sprite sprite = ReviewedArtSetup.EnvironmentSpriteNamed(name) ?? ground;
                Place(sprite, name, new Vector2(x + .5f, y + .5f), world, -10000);
            }
            PlaceDecoration("Pond", new Vector2(4.4f, -2.7f), world, -9500);
            PlaceDecoration("Tree", new Vector2(-5.8f, 1.7f), world);
            PlaceDecoration("Tree", new Vector2(-3.8f, 2.25f), world);
            PlaceDecoration("Tree", new Vector2(5.7f, 1.55f), world);
            PlaceDecoration("Tree", new Vector2(6.7f, -1.6f), world);
            PlaceDecoration("Shrub", new Vector2(-5.0f, -2.2f), world);
            PlaceDecoration("Shrub", new Vector2(3.7f, 2.0f), world);
            PlaceDecoration("Flowers", new Vector2(-3.8f, -2.5f), world);
            PlaceDecoration("Flowers", new Vector2(5.0f, 2.0f), world);
            PlaceDecoration("Daisies", new Vector2(1.3f, 2.4f), world);
            PlaceDecoration("Daisies", new Vector2(-1.8f, -2.2f), world);
            PlaceDecoration("Rock", new Vector2(2.65f, -2.3f), world);
            MeleeActor player = MakeActor("Hero", hero, 1, 120, 30, new Vector3(-2.5f, 0, 0));
            MeleeActor opponent = MakeActor("Melee Enemy", enemy, 2, 80, 10, new Vector3(2.5f, 0, 0));
            opponent.GetComponent<SpriteRenderer>().color = new Color(1, .7f, .72f);
            opponent.GetComponent<SpriteRenderer>().flipX = true;
            player.SetTarget(opponent);
            opponent.SetTarget(player);
            new GameObject("Encounter", typeof(FirstEncounter)).GetComponent<FirstEncounter>().Configure(player, opponent);
            if (!EditorSceneManager.SaveScene(scene, path)) throw new IOException("Could not save first encounter.");
            if (!EditorBuildSettings.scenes.Any(s => s.path == path))
                EditorBuildSettings.scenes = EditorBuildSettings.scenes.Concat(new[] { new EditorBuildSettingsScene(path, true) }).ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("FirstEncounter created using selected assets. Unity Play and visual acceptance are still required.");
        }

        private static void PlaceDecoration(string name, Vector2 position, Transform parent, int? order = null)
        {
            Sprite sprite = ReviewedArtSetup.EnvironmentSpriteNamed(name);
            if (sprite != null) Place(sprite, name, position, parent, order ?? -(int)(position.y * 100));
        }

        private static SpriteRenderer Place(Sprite sprite, string name, Vector2 position, Transform parent, int order)
        {
            var view = new GameObject(name, typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
            view.transform.SetParent(parent);
            view.transform.position = position;
            view.sprite = sprite;
            view.sortingOrder = order;
            return view;
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
