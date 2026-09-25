using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using AffixZero.Presentation;

namespace AffixZero.Editor
{
    public static class FoundationSetup
    {
        private const string ScenePath = "Assets/_Game/Scenes/FirstField.unity";

        [MenuItem("AFFIX/Setup/Create First Field Scene")]
        public static void CreateScene()
        {
            if (!Application.unityVersion.StartsWith("6000.3."))
                Debug.LogWarning("This workspace targets Unity 6.3 LTS (6000.3.24f1).");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(ScenePath))
            {
                EditorSceneManager.OpenScene(ScenePath);
                return; // Never overwrite an existing scene.
            }
            Directory.CreateDirectory("Assets/_Game/Scenes");
            AssetDatabase.Refresh();
            EditorSettings.serializationMode = SerializationMode.ForceText;
            EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0, 0, -10);
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.79f, 0.88f, 0.72f);
            new GameObject("Actors");
            new GameObject("World");
            new GameObject("HUD");
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new IOException("Could not save the foundation scene.");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("Unity foundation created. This is NOT a playable game. Import approved licensed animation assets next.");
        }

        [MenuItem("AFFIX/Validate Selected Animation Set")]
        public static void ValidateAnimation()
        {
            var set = Selection.activeObject as ActorAnimationSet;
            if (set == null) { Debug.LogWarning("Select an ActorAnimationSet asset first."); return; }
            string problem = set.ValidateSet();
            if (problem == null) Debug.Log("Animation data checks passed. Visual approval remains separate.", set);
            else Debug.LogError(problem, set);
        }
    }
}
