using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using AffixZero.Presentation;

namespace AffixZero.Editor
{
    // Only the specific, reviewed CC0 source files documented in docs/assets are accepted.
    public static class ReviewedArtSetup
    {
        public const string Root = "Assets/Art/NinjaAdventure/";
        public const string DataRoot = "Assets/_Game/Data/";
        public const string Provenance = "docs/assets/ninja-adventure.md";

        [Serializable] public sealed class EnvironmentManifest { public EnvironmentSprite[] sprites; }
        [Serializable] public sealed class EnvironmentSprite
        {
            public string name, path;
            public int x, y, width, height;
            public float pivotX = 0.5f, pivotY = 0.5f;
        }

        [MenuItem("AFFIX/Setup/Import Reviewed Art and Create Encounter")]
        public static void Build()
        {
            if (Application.unityVersion != "6000.3.24f1")
                throw new InvalidOperationException("Use the approved Unity 6000.3.24f1 editor.");
            if (!File.Exists(Provenance) || !File.Exists(Root + "LICENSE.txt"))
                throw new FileNotFoundException("Reviewed CC0 provenance and source license are required.");
            AssetDatabase.Refresh();
            Directory.CreateDirectory(DataRoot);
            AssetDatabase.Refresh();
            Sprite[] idle = ImportActor("Idle", 4);
            Sprite[] walk = ImportActor("Walk", 4);
            Sprite[] attack = ImportActor("Attack", 4);
            Sprite[] hit = ImportActor("Hit", 2);
            Sprite[] death = ImportGrid(Root + "Characters/NinjaGreen/Dead.png", 32, 0, 2, new Vector2(.5f, .25f));
            Sprite[] katana = ImportGrid(Root + "Weapons/Katana.png", 64, 3, 4, new Vector2(.5f, .375f));
            Sprite[] axe = ImportGrid(Root + "Weapons/Axe.png", 64, 3, 4, new Vector2(.5f, .375f));
            var hero = MakeSet("Hero", idle, walk, attack, hit, death, katana);
            var enemy = MakeSet("Enemy", idle, walk, attack, hit, death, axe);
            var environment = JsonUtility.FromJson<EnvironmentManifest>(File.ReadAllText("docs/assets/ninja-environment.json"));
            if (environment?.sprites == null || environment.sprites.Length == 0)
                throw new InvalidDataException("Reviewed environment rectangles are missing.");
            foreach (var group in environment.sprites.GroupBy(s => s.path))
            {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(group.Key);
                if (texture == null) throw new FileNotFoundException(group.Key);
                Import(group.Key, group.Select(s => new SpriteMetaData {
                    name = s.name, rect = new Rect(s.x, texture.height - s.y - s.height, s.width, s.height),
                    alignment = (int)SpriteAlignment.Custom, pivot = new Vector2(s.pivotX, s.pivotY)
                }).ToArray());
            }
            Sprite ground = EnvironmentSpriteNamed("Grass");
            ProjectDashboard.CreateReviewedEncounter(hero, enemy, ground);
            AssetDatabase.SaveAssets();
            Debug.Log("Reviewed source sprites and encounter configured. Play and visual acceptance remain separate checks.");
        }

        public static Sprite EnvironmentSpriteNamed(string name)
        {
            var path = "docs/assets/ninja-environment.json";
            if (!File.Exists(path)) return null;
            var spec = JsonUtility.FromJson<EnvironmentManifest>(File.ReadAllText(path)).sprites.FirstOrDefault(s => s.name == name);
            return spec == null ? null : AssetDatabase.LoadAllAssetsAtPath(spec.path).OfType<Sprite>().SingleOrDefault(s => s.name == name);
        }

        private static Sprite[] ImportActor(string name, int count) =>
            ImportGrid(Root + "Characters/NinjaGreen/" + name + ".png", 32, 3, count, new Vector2(.5f, .25f));

        private static Sprite[] ImportGrid(string path, int cell, int column, int count, Vector2 pivot)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null) throw new FileNotFoundException(path);
            int height = texture.height;
            if (texture.width < (column + 1) * cell || height != count * cell)
                throw new InvalidDataException("Unexpected source dimensions: " + path);
            string prefix = Path.GetFileNameWithoutExtension(path);
            Import(path, Enumerable.Range(0, count).Select(i => new SpriteMetaData {
                name = prefix + "_" + i, rect = new Rect(column * cell, height - (i + 1) * cell, cell, cell),
                alignment = (int)SpriteAlignment.Custom, pivot = pivot
            }).ToArray());
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name, StringComparer.Ordinal).ToArray();
        }

        private static void Import(string path, SpriteMetaData[] slices)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidDataException("Not a texture: " + path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 16;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            // Supported by the pinned editor; source textures stay intact and slice IDs are retained on reimport.
#pragma warning disable 0618
            importer.spritesheet = slices;
#pragma warning restore 0618
            importer.SaveAndReimport();
        }

        private static ActorAnimationSet MakeSet(string name, Sprite[] idle, Sprite[] walk, Sprite[] attack,
            Sprite[] hit, Sprite[] death, Sprite[] weapon)
        {
            string path = DataRoot + name + ".asset";
            var set = AssetDatabase.LoadAssetAtPath<ActorAnimationSet>(path);
            if (set == null) { set = ScriptableObject.CreateInstance<ActorAnimationSet>(); AssetDatabase.CreateAsset(set, path); }
            var data = new SerializedObject(set);
            SetFrames(data, "idle", idle); SetFrames(data, "walk", walk); SetFrames(data, "attack", attack);
            SetFrames(data, "hit", hit); SetFrames(data, "death", death); SetFrames(data, "attackWeapon", weapon);
            data.FindProperty("attackFps").floatValue = name == "Hero" ? 8 : 10;
            data.FindProperty("movementFps").floatValue = 8;
            data.FindProperty("reactionFps").floatValue = 10;
            data.FindProperty("impactFrame").intValue = 1;
            data.FindProperty("sourceFacesRight").boolValue = true;
            data.FindProperty("sourceLicenseRecord").stringValue = Provenance;
            data.ApplyModifiedPropertiesWithoutUndo();
            string problem = set.ValidateSet();
            if (problem != null) throw new InvalidDataException(name + ": " + problem);
            return set;
        }

        private static void SetFrames(SerializedObject data, string field, Sprite[] sprites)
        {
            var property = data.FindProperty(field);
            if (property == null) throw new InvalidDataException("Animation schema does not contain " + field);
            property.arraySize = sprites.Length;
            for (int i = 0; i < sprites.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
        }
    }
}
