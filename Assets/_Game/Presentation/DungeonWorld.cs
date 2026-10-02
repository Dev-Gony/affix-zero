using System;
using System.Collections.Generic;
using AffixZero.Core;
using UnityEngine;

namespace AffixZero.Presentation
{
    // One connected dungeon. The same authored cells drive movement, LOS, scenery and the minimap.
    public sealed class DungeonWorld : IDisposable
    {
        public const int Width = 72, Height = 32;
        public static readonly Vector2 Origin = new Vector2(-36, -16);
        public static readonly Vector2 Entrance = new Vector2(-31.5f, -11.5f);
        public static readonly Rect Bounds = new Rect(Origin, new Vector2(Width, Height));
        public readonly DungeonNavigation Navigation;
        private readonly GameObject scenery;
        private readonly Sprite floorSprite, obstacleSprite;
        private readonly List<Rect> obstacles = new List<Rect>();
        private static readonly Vector2[] Landmarks =
        {
            new Vector2(-24,-7), new Vector2(-15,7), new Vector2(-4,-5), new Vector2(5,6),
            new Vector2(15,-7), new Vector2(25,5), new Vector2(-1,0), new Vector2(18,1)
        };
        public static readonly Vector2[] SpawnPoints =
        {
            new Vector2(-28,-9),new Vector2(-25,-4),new Vector2(-29,3),new Vector2(-23,9),
            new Vector2(-18,-10),new Vector2(-16,-2),new Vector2(-18,5),new Vector2(-11,10),
            new Vector2(-9,-8),new Vector2(-7,-1),new Vector2(-8,8),new Vector2(-2,11),
            new Vector2(2,-10),new Vector2(4,-3),new Vector2(2,8),new Vector2(9,11),
            new Vector2(11,-8),new Vector2(13,0),new Vector2(11,7),new Vector2(19,10),
            new Vector2(21,-10),new Vector2(25,-4),new Vector2(23,4),new Vector2(30,9)
        };
        public int DetourQueries { get; private set; }

        public DungeonWorld()
        {
            var blocked = new List<GridCell>();
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
                        blocked.Add(new GridCell(x, y));
            foreach (Vector2 center in Landmarks)
            {
                GridCell low = Cell(center - Vector2.one);
                for (int x = low.X; x < low.X + 2; x++)
                    for (int y = low.Y; y < low.Y + 2; y++) blocked.Add(new GridCell(x, y));
            }
            Navigation = new DungeonNavigation(Width, Height, blocked);
            foreach (GridCell cell in blocked)
                obstacles.Add(new Rect(Origin.x + cell.X - .15f, Origin.y + cell.Y - .15f, 1.3f, 1.3f));

            Texture2D room = Resources.Load<Texture2D>("AffixOriginal/TempleRoom-v2");
            Texture2D rubble = Resources.Load<Texture2D>("AffixOriginal/TempleObstacle-v2");
            if (room == null || rubble == null) throw new InvalidOperationException("Original temple world art is missing.");
            Rect crop = new Rect(256, 192, 1024, 640);
            floorSprite = Sprite.Create(room, crop, new Vector2(.5f, .5f), crop.width / 24f);
            obstacleSprite = Sprite.Create(rubble, new Rect(0, 0, rubble.width, rubble.height), new Vector2(.5f, .5f), rubble.width / 2.4f);
            scenery = new GameObject("Connected Temple Dungeon");
            for (int row = 0; row < 2; row++)
                for (int column = 0; column < 3; column++)
                {
                    var tile = new GameObject("Temple floor " + column + ":" + row, typeof(SpriteRenderer));
                    tile.transform.SetParent(scenery.transform, false);
                    tile.transform.position = new Vector3(-24 + column * 24, -8 + row * 16, 0);
                    tile.transform.localScale = new Vector3(1, 1.4f, 1);
                    var renderer = tile.GetComponent<SpriteRenderer>();
                    renderer.sprite = floorSprite; renderer.sortingOrder = -10000;
                    renderer.color = ((column + row) & 1) == 0 ? Color.white : new Color(.88f, .94f, .96f, 1);
                }
            for (int i = 0; i < Landmarks.Length; i++)
            {
                Vector2 point = Landmarks[i];
                var obstacle = new GameObject("Navigation landmark " + i, typeof(SpriteRenderer), typeof(BoxCollider2D));
                obstacle.transform.SetParent(scenery.transform, false);
                obstacle.transform.position = point;
                var renderer = obstacle.GetComponent<SpriteRenderer>();
                renderer.sprite = obstacleSprite; renderer.sortingOrder = -(int)(point.y * 100) + 40; renderer.flipX = (i & 1) == 1;
                obstacle.GetComponent<BoxCollider2D>().size = new Vector2(2, 2);
            }
        }

        public GridCell Cell(Vector2 point) => new GridCell(Mathf.FloorToInt(point.x - Origin.x), Mathf.FloorToInt(point.y - Origin.y));
        public Vector2 Center(GridCell cell) => Origin + new Vector2(cell.X + .5f, cell.Y + .5f);
        public bool IsWalkable(Vector2 point) => Navigation.IsWalkable(Cell(point));
        public Vector2 SafePoint(Vector2 desired)
        {
            if (Fits(desired)) return desired;
            GridCell around = Cell(desired); Vector2 nearest = Entrance; float score = float.MaxValue;
            for (int radius = 1; radius <= 6; radius++)
                for (int y = -radius; y <= radius; y++)
                    for (int x = -radius; x <= radius; x++)
                    {
                        Vector2 candidate = Center(new GridCell(around.X + x, around.Y + y));
                        if (!Fits(candidate)) continue;
                        float candidateScore = (candidate - desired).sqrMagnitude;
                        if (candidateScore < score) { score = candidateScore; nearest = candidate; }
                    }
            return nearest;
        }
        public Vector2 PatrolPoint(int actorIndex, int step)
        {
            int index = Math.Abs((actorIndex * 7 + step * 11) % SpawnPoints.Length);
            Vector2 offset = new Vector2(((step * 13 + actorIndex) % 5 - 2) * .55f, ((step * 17 + actorIndex) % 5 - 2) * .55f);
            return SafePoint(SpawnPoints[index] + offset);
        }
        private bool Fits(Vector2 point) => IsWalkable(point) && IsWalkable(point + Vector2.right * .2f) &&
            IsWalkable(point - Vector2.right * .2f) && IsWalkable(point + Vector2.up * .2f) && IsWalkable(point - Vector2.up * .2f);
        public bool LineOfSight(Vector2 from, Vector2 to)
        {
            if (!Fits(from) || !Fits(to)) return false;
            foreach (Rect box in obstacles)
            {
                float near = 0, far = 1; Vector2 delta = to - from;
                if (ClipAxis(from.x, delta.x, box.xMin, box.xMax, ref near, ref far) &&
                    ClipAxis(from.y, delta.y, box.yMin, box.yMax, ref near, ref far)) return false;
            }
            return true;
        }
        private static bool ClipAxis(float start, float delta, float min, float max, ref float near, ref float far)
        {
            if (Mathf.Abs(delta) < .00001f) return start >= min && start <= max;
            float a = (min - start) / delta, b = (max - start) / delta;
            near = Mathf.Max(near, Mathf.Min(a, b)); far = Mathf.Min(far, Mathf.Max(a, b)); return near <= far;
        }
        public Vector2 NextWaypoint(Vector2 from, Vector2 destination)
        {
            if (!IsWalkable(from) || !IsWalkable(destination)) return from;
            if (LineOfSight(from, destination)) return destination;
            IReadOnlyList<GridCell> path = Navigation.FindPath(Cell(from), Cell(destination));
            if (path.Count == 0) return from;
            DetourQueries++;
            Vector2 ownCenter = Center(Cell(from));
            // Center the actor before taking the first cardinal BFS step. This avoids cutting an
            // inflated landmark corner and makes the visible obstacle, LOS and traversed cells agree.
            if(Vector2.Distance(from,ownCenter)>.12f)return ownCenter;
            return Center(path[0]);
        }
        public void Dispose()
        {
            if (scenery != null) UnityEngine.Object.Destroy(scenery);
            if (floorSprite != null) UnityEngine.Object.Destroy(floorSprite);
            if (obstacleSprite != null) UnityEngine.Object.Destroy(obstacleSprite);
        }
    }
}
