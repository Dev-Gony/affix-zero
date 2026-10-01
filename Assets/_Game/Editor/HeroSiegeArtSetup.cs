using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AffixZero.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AffixZero.Editor
{
    // Imports the original, project-owned generated combat set and keeps the existing gameplay scene intact.
    public static class HeroSiegeArtSetup
    {
        public const string ScenePath = "Assets/_Game/Scenes/HeroSiegeEncounter.unity";
        public const string DataRoot = "Assets/_Game/Data/HeroSiege";
        public const string ArtRoot = "Assets/Art/OriginalTemple/Resources/AffixOriginal";
        public const string HeroAtlasPath = ArtRoot + "/HeroAtlas-v1.png";
        public const string EnemyAtlasPath = ArtRoot + "/EnemyAtlas-v1.png";
        public const string ImpactAtlasPath = ArtRoot + "/ImpactFxAtlas-v1.png";
        public const string BackgroundPath = ArtRoot + "/TempleRoom-v2.png";
        public const string ObstaclePath = ArtRoot + "/TempleObstacle-v2.png";
        private const string ProvenancePath = "docs/assets/original-temple-combat-set.md";
        private const string PreviousScene = "Assets/_Game/Scenes/FirstEncounter.unity";
        private const float WorldWidth = 32;
        private static readonly string[] DirectionNames = { "Down", "Left", "Right", "Up" };

        [MenuItem("AFFIX/Setup/Build Hero Siege Art Review Scene")]
        public static void Build()
        {
            ValidateSources(); // No blank scene or partial scene is created on missing/mismatched sources.
            AssetDatabase.Refresh();
            EnsureFolder(DataRoot);
            EnsureFolder("Assets/_Game/Scenes");
            ActorAnimationSet heroSet = ImportActor(HeroAtlasPath, "Vanguard", "Hero");
            ActorAnimationSet enemySet = ImportActor(EnemyAtlasPath, "Raider", "Enemy");
            ImportImpactFx();
            ImportObstacle();
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
                Scene existing = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                ApplyExistingScene(heroSet, enemySet, room);
                if (!EditorSceneManager.SaveScene(existing)) throw new IOException("Could not update the existing encounter scene art.");
                RegisterBuildScene();
                Debug.Log("Existing HeroSiegeEncounter gameplay was preserved while the original temple combat art was reapplied.");
                return;
            }
            CreateScene(heroSet, enemySet, room);
            RegisterBuildScene();
            AssetDatabase.SaveAssets();
            Debug.Log("HeroSiegeEncounter created from the original generated four-direction temple combat set. Play and visual acceptance remain unverified.");
        }

        private static void ValidateSources()
        {
            if (Application.unityVersion != "6000.3.24f1")
                throw new InvalidOperationException("Use the approved Unity 6000.3.24f1 editor.");
            if (!File.Exists(ProvenancePath)) throw new FileNotFoundException("Generated art provenance is missing.", ProvenancePath);
            RequirePngSize(HeroAtlasPath, 1024, 1024);
            RequirePngSize(EnemyAtlasPath, 1024, 1024);
            RequirePngSize(ImpactAtlasPath, 1024, 1024);
            RequirePngSize(BackgroundPath, 1536, 1024);
            RequirePngSize(ObstaclePath, 1024, 768);
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

        private static ActorAnimationSet ImportActor(string path, string actor, string assetName)
        {
            TextureImporter importer = TextureAt(path);
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 80;
            var slices = new List<SpriteMetaData>();
            for (int direction = 0; direction < 4; direction++)
            {
                int locomotionRow = direction * 2;
                int combatRow = locomotionRow + 1;
                AddSlices(slices, actor, DirectionNames[direction], "Idle", locomotionRow, 0, 2);
                AddSlices(slices, actor, DirectionNames[direction], "Walk", locomotionRow, 2, 4);
                AddSlices(slices, actor, DirectionNames[direction], "Hit", locomotionRow, 6, 2);
                AddSlices(slices, actor, DirectionNames[direction], "Attack", combatRow, 0, 5);
                AddSlices(slices, actor, DirectionNames[direction], "Death", combatRow, 5, 3);
            }
#pragma warning disable 0618
            importer.spritesheet = slices.ToArray();
#pragma warning restore 0618
            importer.SaveAndReimport();
            var loaded = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
                .ToDictionary(sprite => sprite.name, StringComparer.Ordinal);
            if (loaded.Count != 64) throw new InvalidDataException("Expected 64 imported directional frames: " + path);
            string assetPath = DataRoot + "/" + assetName + ".asset";
            var set = AssetDatabase.LoadAssetAtPath<ActorAnimationSet>(assetPath);
            if (set == null)
            {
                set = ScriptableObject.CreateInstance<ActorAnimationSet>();
                AssetDatabase.CreateAsset(set, assetPath);
            }
            var serialized = new SerializedObject(set);
            AssignFrames(serialized, "idle", Clip(loaded, actor, "Idle", 2));
            AssignFrames(serialized, "walk", Clip(loaded, actor, "Walk", 4));
            AssignFrames(serialized, "attack", Clip(loaded, actor, "Attack", 5));
            AssignFrames(serialized, "hit", Clip(loaded, actor, "Hit", 2));
            AssignFrames(serialized, "death", Clip(loaded, actor, "Death", 3));
            AssignFrames(serialized, "attackWeapon", Array.Empty<Sprite>()); // Body sheets already contain the weapon.
            serialized.FindProperty("fourDirections").boolValue = true;
            serialized.FindProperty("idleFramesPerDirection").intValue = 2;
            serialized.FindProperty("walkFramesPerDirection").intValue = 4;
            serialized.FindProperty("attackFramesPerDirection").intValue = 5;
            serialized.FindProperty("hitFramesPerDirection").intValue = 2;
            serialized.FindProperty("deathFramesPerDirection").intValue = 3;
            serialized.FindProperty("attackFps").floatValue = 12;
            serialized.FindProperty("movementFps").floatValue = 8;
            serialized.FindProperty("reactionFps").floatValue = 16;
            serialized.FindProperty("deathFps").floatValue = 8;
            serialized.FindProperty("impactFrame").intValue = 2;
            serialized.FindProperty("sourceFacesRight").boolValue = true;
            serialized.FindProperty("sourceLicenseRecord").stringValue = ProvenancePath;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(set);
            return set;
        }

        private static void AddSlices(List<SpriteMetaData> slices, string actor, string direction,
            string clip, int topRow, int startColumn, int count)
        {
            for (int frame = 0; frame < count; frame++)
                slices.Add(new SpriteMetaData {
                    name = actor + "_" + direction + "_" + clip + "_" + frame.ToString("00"),
                    rect = new Rect((startColumn + frame) * 128, 1024 - (topRow + 1) * 128, 128, 128),
                    alignment = (int)SpriteAlignment.Custom,
                    pivot = new Vector2(.5f, .14f)
                });
        }

        private static Sprite[] Clip(Dictionary<string, Sprite> sprites, string actor, string clip, int count)
        {
            var result = new List<Sprite>(count * 4);
            foreach (string direction in DirectionNames)
            for (int frame = 0; frame < count; frame++)
                result.Add(sprites[actor + "_" + direction + "_" + clip + "_" + frame.ToString("00")]);
            return result.ToArray();
        }

        private static void ImportImpactFx()
        {
            TextureImporter importer = TextureAt(ImpactAtlasPath);
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 160;
            var slices = new List<SpriteMetaData>();
            for (int frame = 0; frame < 8; frame++)
            {
                int topRow = frame / 4;
                int column = frame % 4;
                slices.Add(new SpriteMetaData {
                    name = "Impact_" + frame.ToString("00"),
                    rect = new Rect(column * 256, 1024 - (topRow + 1) * 512, 256, 512),
                    alignment = (int)SpriteAlignment.Custom,
                    pivot = new Vector2(.5f, .5f)
                });
            }
#pragma warning disable 0618
            importer.spritesheet = slices.ToArray();
#pragma warning restore 0618
            importer.SaveAndReimport();
        }

        private static void ImportObstacle()
        {
            TextureImporter importer = TextureAt(ObstaclePath);
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 1024 / 2.4f;
            importer.spritePivot = new Vector2(.5f, .5f);
            importer.SaveAndReimport();
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

        private static void ApplyExistingScene(ActorAnimationSet heroSet, ActorAnimationSet enemySet, Sprite room)
        {
            foreach (MeleeActor actor in UnityEngine.Object.FindObjectsByType<MeleeActor>(FindObjectsSortMode.None))
            {
                ActorAnimationSet set = actor.ActorId == 1 ? heroSet : enemySet;
                actor.Configure(set, actor.ActorId, actor.MaxHp, actor.Damage);
                actor.GetComponent<SpriteRenderer>().sprite = set.Frame(ActorClip.Idle, 0, ActorFacing.Down);
                EditorUtility.SetDirty(actor);
            }
            SpriteRenderer background = UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)
                .FirstOrDefault(renderer => renderer.sortingOrder <= -10000);
            if (background == null) throw new InvalidDataException("Existing encounter background renderer was not found.");
            background.sprite = room;
            background.name = "Temple Room - original generated v2";
            EditorUtility.SetDirty(background);
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
            background.name = "Temple Room - original generated v2";
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
