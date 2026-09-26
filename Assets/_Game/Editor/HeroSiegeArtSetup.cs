using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using AffixZero.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AffixZero.Editor
{
    // Sources stay local because the free pack permits game use, not raw redistribution.
    // Public scene/data references are regenerated from the exact audited local sources.
    public static class HeroSiegeArtSetup
    {
        public const string ScenePath = "Assets/_Game/Scenes/HeroSiegeEncounter.unity";
        public const string DataRoot = "Assets/_Game/Data/HeroSiege";
        public const string SourceRoot = "Assets/LocalLicensed/Zerie";
        public const string ManifestPath = "docs/assets/zerie-local-manifest.json";
        public const string BackgroundPath = "Assets/Art/Generated/Resources/AffixGenerated/TempleRoom-v1.png";
        private const string ProvenancePath = "docs/assets/HERO_SIEGE_CHARACTER_CANDIDATES.md";
        private const string PreviousScene = "Assets/_Game/Scenes/FirstEncounter.unity";
        private const float WorldWidth = 32;
        private static readonly string[] Clips = { "Idle", "Walk", "Attack01", "Hurt", "Death" };
        private static readonly int[] FrameCounts = { 6, 8, 6, 4, 4 };

        [Serializable] private sealed class SourceManifest { public SourceEntry[] files = Array.Empty<SourceEntry>(); }
        [Serializable] private sealed class SourceEntry { public string path = ""; public string sha256 = ""; }

        [MenuItem("AFFIX/Setup/Build Hero Siege Art Review Scene")]
        public static void Build()
        {
            ValidateSources(); // No blank scene or partial scene is created on missing/mismatched sources.
            AssetDatabase.Refresh();
            EnsureFolder(DataRoot);
            EnsureFolder("Assets/_Game/Scenes");
            ActorAnimationSet heroSet = ImportActor("Soldier", "Hero", new Vector2(.5f, .4f));
            ActorAnimationSet enemySet = ImportActor("Orc", "Enemy", new Vector2(.55f, .43f));
            Sprite room = ImportBackground();
            string issue = heroSet.ValidateSet() ?? enemySet.ValidateSet();
            if (issue != null || room == null)
                throw new InvalidDataException(issue ?? "Generated room sprite could not be loaded.");
            AssetDatabase.SaveAssets();

            if (Application.isBatchMode)
            {
                if (SceneManager.GetActiveScene().isDirty)
                    throw new InvalidOperationException("An unsaved active scene must be handled before scene generation.");
            }
            else if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            if (File.Exists(ScenePath))
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                RegisterBuildScene();
                Debug.Log("Existing HeroSiegeEncounter opened without overwriting its scene content.");
                return;
            }
            CreateScene(heroSet, enemySet, room);
            RegisterBuildScene();
            AssetDatabase.SaveAssets();
            Debug.Log("HeroSiegeEncounter created from audited local character sheets and a new generated room. This is a visual review scene; Play and user visual acceptance remain unverified.");
        }

        private static void ValidateSources()
        {
            if (Application.unityVersion != "6000.3.24f1")
                throw new InvalidOperationException("Use the approved Unity 6000.3.24f1 editor.");
            if (!File.Exists(ManifestPath) || !File.Exists(ProvenancePath))
                throw new FileNotFoundException("Local character audit manifest and provenance are required before import.");
            var manifest = JsonUtility.FromJson<SourceManifest>(File.ReadAllText(ManifestPath));
            if (manifest == null || manifest.files == null)
                throw new InvalidDataException("Expected a files array in " + ManifestPath);
            foreach (string actor in new[] { "Soldier", "Orc" })
            for (int i = 0; i < Clips.Length; i++)
            {
                string path = SourceRoot + "/" + actor + "/" + Clips[i] + ".png";
                if (!File.Exists(path))
                    throw new FileNotFoundException("Required private source is missing. Acquire the documented free pack locally; no substitute scene will be created.", path);
                SourceEntry[] entries = manifest.files.Where(e => e != null && e.path == path).ToArray();
                if (entries.Length != 1 || entries[0].sha256 == null || entries[0].sha256.Length != 64)
                    throw new InvalidDataException("Expected one exact SHA-256 audit entry for " + path);
                using (var hash = SHA256.Create())
                using (var stream = File.OpenRead(path))
                {
                    string actual = BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
                    if (!string.Equals(actual, entries[0].sha256, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidDataException("Source hash differs from the reviewed file: " + path);
                }
                RequirePngSize(path, FrameCounts[i] * 100, 100);
            }
            if (!File.Exists(BackgroundPath)) throw new FileNotFoundException("New generated room image is missing.", BackgroundPath);
            RequirePngSize(BackgroundPath, 1536, 1024);
        }

        private static void RequirePngSize(string path, int width, int height)
        {
            byte[] header = new byte[24];
            using (var stream = File.OpenRead(path))
            {
                if (stream.Read(header, 0, header.Length) != header.Length)
                    throw new InvalidDataException("Truncated PNG: " + path);
            }
            byte[] signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
            for (int i = 0; i < signature.Length; i++)
                if (header[i] != signature[i]) throw new InvalidDataException("Not a PNG: " + path);
            int actualWidth = ReadBigEndian(header, 16);
            int actualHeight = ReadBigEndian(header, 20);
            if (actualWidth != width || actualHeight != height)
                throw new InvalidDataException(path + ": expected " + width + "x" + height + ", got " + actualWidth + "x" + actualHeight);
        }

        private static int ReadBigEndian(byte[] bytes, int offset) =>
            (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];

        private static ActorAnimationSet ImportActor(string actor, string assetName, Vector2 pivot)
        {
            var frames = new List<Sprite[]>();
            for (int i = 0; i < Clips.Length; i++)
            {
                string path = SourceRoot + "/" + actor + "/" + Clips[i] + ".png";
                TextureImporter importer = TextureAt(path);
                importer.spriteImportMode = SpriteImportMode.Multiple;
                importer.spritePixelsPerUnit = 32;
                var slices = Enumerable.Range(0, FrameCounts[i]).Select(frame => new SpriteMetaData {
                    name = actor + "_" + Clips[i] + "_" + frame.ToString("00"),
                    rect = new Rect(frame * 100, 0, 100, 100),
                    alignment = (int)SpriteAlignment.Custom, pivot = pivot
                }).ToArray();
#pragma warning disable 0618
                importer.spritesheet = slices;
#pragma warning restore 0618
                importer.SaveAndReimport();
                Sprite[] loaded = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
                    .OrderBy(sprite => sprite.name, StringComparer.Ordinal).ToArray();
                if (loaded.Length != FrameCounts[i]) throw new InvalidDataException("Unexpected imported frame count: " + path);
                frames.Add(loaded);
            }
            string assetPath = DataRoot + "/" + assetName + ".asset";
            var set = AssetDatabase.LoadAssetAtPath<ActorAnimationSet>(assetPath);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<ActorAnimationSet>();
                AssetDatabase.CreateAsset(set, assetPath);
            }
            var serialized = new SerializedObject(set);
            string[] properties = { "idle", "walk", "attack", "hit", "death" };
            for (int i = 0; i < properties.Length; i++) AssignFrames(serialized, properties[i], frames[i]);
            AssignFrames(serialized, "attackWeapon", Array.Empty<Sprite>()); // Body sheets already contain the weapon.
            serialized.FindProperty("attackFps").floatValue = 10;
            serialized.FindProperty("movementFps").floatValue = 10;
            // Reviewed timing hypothesis: shorten Hurt's flash playback to avoid perpetual hit-stun.
            serialized.FindProperty("reactionFps").floatValue = 20;
            serialized.FindProperty("deathFps").floatValue = 10;
            serialized.FindProperty("impactFrame").intValue = 3;
            serialized.FindProperty("sourceFacesRight").boolValue = true;
            serialized.FindProperty("sourceLicenseRecord").stringValue = ProvenancePath;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(set);
            return set;
        }

        private static TextureImporter TextureAt(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidDataException("Texture importer missing: " + path);
            importer.textureType = TextureImporterType.Sprite;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 2048;
            return importer;
        }

        private static Sprite ImportBackground()
        {
            TextureImporter importer = TextureAt(BackgroundPath);
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 1536 / WorldWidth;
            importer.spritePivot = new Vector2(.5f, .5f);
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundPath);
        }

        private static void AssignFrames(SerializedObject owner, string field, Sprite[] sprites)
        {
            var array = owner.FindProperty(field);
            if (array == null) throw new InvalidDataException("Animation schema changed: " + field);
            array.arraySize = sprites.Length;
            for (int i = 0; i < sprites.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
        }

        private static void CreateScene(ActorAnimationSet heroSet, ActorAnimationSet enemySet, Sprite room)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0, 0, -10);
            camera.orthographic = true;
            camera.orthographicSize = 32f * 1024 / 1536 / 2;
            camera.allowMSAA = false;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.transparencySortMode = TransparencySortMode.CustomAxis;
            camera.transparencySortAxis = Vector3.up;
            var background = new GameObject("Temple Room — generated background", typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
            background.sprite = room;
            background.sortingOrder = -10000;
            // Explicit visual bounds markers, not a collision/navmesh implementation.
            Transform bounds = new GameObject("Room Visual Bounds").transform;
            bounds.SetParent(background.transform, false);
            var minimum = new GameObject("Minimum").transform;
            minimum.SetParent(bounds, false);
            minimum.localPosition = new Vector3(-WorldWidth / 2, -camera.orthographicSize, 0);
            var maximum = new GameObject("Maximum").transform;
            maximum.SetParent(bounds, false);
            maximum.localPosition = new Vector3(WorldWidth / 2, camera.orthographicSize, 0);
            MeleeActor hero = CreateActor("Soldier", heroSet, 1, 120, 30, new Vector3(-2, 0, 0));
            MeleeActor enemy = CreateActor("Orc", enemySet, 2, 80, 10, new Vector3(2, 0, 0));
            hero.SetTarget(enemy);
            enemy.SetTarget(hero);
            new GameObject("Encounter", typeof(FirstEncounter)).GetComponent<FirstEncounter>().Configure(hero, enemy);
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new IOException("Could not save the new art review scene.");
        }

        private static MeleeActor CreateActor(string name, ActorAnimationSet set, int id, int hp, int damage, Vector3 position)
        {
            var actor = new GameObject(name, typeof(SpriteRenderer), typeof(MeleeActor)).GetComponent<MeleeActor>();
            actor.transform.position = position;
            actor.GetComponent<SpriteRenderer>().sprite = set.Frame(ActorClip.Idle, 0);
            actor.Configure(set, id, hp, damage);
            return actor;
        }

        private static void RegisterBuildScene()
        {
            var scenes = new List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(ScenePath, true) };
            scenes.AddRange(EditorBuildSettings.scenes.Where(scene => scene.path != ScenePath)
                .Select(scene => new EditorBuildSettingsScene(scene.path, scene.path == PreviousScene ? false : scene.enabled)));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = path.Substring(0, path.LastIndexOf('/'));
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
