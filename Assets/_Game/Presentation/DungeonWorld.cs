using System;
using System.Collections.Generic;
using AffixZero.Core;
using UnityEngine;

namespace AffixZero.Presentation
{
    public enum DungeonLayoutId { Bastion = 0, Galleries = 1, Crucible = 2 }

    // Three connected layouts. Each authored obstacle set drives movement, LOS, scenery and minimap.
    public sealed class DungeonWorld : IDisposable
    {
        public const int Width = 72, Height = 32;
        public static readonly Vector2 Origin = new Vector2(-36, -16);
        public static readonly Vector2 Entrance = new Vector2(-31.5f, -11.5f);
        public static readonly Rect Bounds = new Rect(Origin, new Vector2(Width, Height));
        public readonly DungeonNavigation Navigation;
        private readonly GameObject scenery, identityRoot;
        private readonly Sprite floorSprite, obstacleSprite, identitySprite;
        private readonly Texture2D identityTexture;
        private readonly List<Rect> obstacles = new List<Rect>();
        private readonly List<Material> materials = new List<Material>();
        private static readonly Vector2[] BastionLandmarks =
        {
            new Vector2(-24,-7), new Vector2(-15,7), new Vector2(-4,-5), new Vector2(5,6),
            new Vector2(15,-7), new Vector2(25,5), new Vector2(-1,0), new Vector2(18,1)
        };
        private static readonly Vector2[] GalleryLandmarks =
        {
            new Vector2(-22,-7),new Vector2(-22,-1),new Vector2(-22,7),
            new Vector2(-8,-10),new Vector2(-8,-4),new Vector2(-8,5),new Vector2(-8,11),
            new Vector2(7,-8),new Vector2(7,0),new Vector2(7,8),
            new Vector2(21,-6),new Vector2(21,3),new Vector2(21,9)
        };
        private static readonly Vector2[] CrucibleLandmarks =
        {
            new Vector2(-12,-6),new Vector2(-6,-10),new Vector2(6,-10),new Vector2(12,-6),
            new Vector2(12,6),new Vector2(6,10),new Vector2(-6,10),new Vector2(-12,6),
            new Vector2(-5,-3),new Vector2(5,-3),new Vector2(-5,3),new Vector2(5,3)
        };
        private static readonly Vector2[] BastionSpawns =
        {
            new Vector2(-28,-9),new Vector2(-25,-4),new Vector2(-29,3),new Vector2(-23,9),
            new Vector2(-18,-10),new Vector2(-16,-2),new Vector2(-18,5),new Vector2(-11,10),
            new Vector2(-9,-8),new Vector2(-7,-1),new Vector2(-8,8),new Vector2(-2,11),
            new Vector2(2,-10),new Vector2(4,-3),new Vector2(2,8),new Vector2(9,11),
            new Vector2(11,-8),new Vector2(13,0),new Vector2(11,7),new Vector2(19,10),
            new Vector2(21,-10),new Vector2(25,-4),new Vector2(23,4),new Vector2(30,9)
        };
        private static readonly Vector2[] GallerySpawns =
        {
            new Vector2(-29,-10),new Vector2(-27,-4),new Vector2(-29,4),new Vector2(-27,10),
            new Vector2(-17,-11),new Vector2(-16,-5),new Vector2(-17,2),new Vector2(-15,10),
            new Vector2(-3,-10),new Vector2(-2,-4),new Vector2(-3,4),new Vector2(-1,11),
            new Vector2(11,-11),new Vector2(12,-4),new Vector2(11,4),new Vector2(13,11),
            new Vector2(25,-11),new Vector2(27,-5),new Vector2(25,2),new Vector2(28,10),
            new Vector2(-13,0),new Vector2(2,0),new Vector2(16,-1),new Vector2(31,3)
        };
        private static readonly Vector2[] CrucibleSpawns =
        {
            new Vector2(-29,-10),new Vector2(-28,0),new Vector2(-29,10),new Vector2(-20,-7),
            new Vector2(-20,7),new Vector2(-13,-11),new Vector2(-13,11),new Vector2(-8,-6),
            new Vector2(-8,6),new Vector2(-2,-11),new Vector2(-2,-6),new Vector2(-2,0),
            new Vector2(-2,7),new Vector2(3,-10),new Vector2(3,-5),new Vector2(3,1),
            new Vector2(3,8),new Vector2(10,-11),new Vector2(10,0),new Vector2(10,11),
            new Vector2(19,-7),new Vector2(19,7),new Vector2(28,-9),new Vector2(29,8)
        };
        private readonly Vector2[] landmarks;
        public IReadOnlyList<Vector2> SpawnPoints { get; }
        public IReadOnlyList<Vector2> ObstacleCenters => landmarks;
        public DungeonLayoutId LayoutId { get; }
        public string LayoutName { get; }
        public string VisualIdentityName { get; }
        public string VisualIdentitySignature { get; }
        public Color AccentColor { get; }
        public int IdentityDecorationCount { get; private set; }
        public int IdentityColliderCount => identityRoot == null ? 0 : identityRoot.GetComponentsInChildren<Collider2D>(true).Length;
        public int DetourQueries { get; private set; }

        public DungeonWorld(DungeonLayoutId layout = DungeonLayoutId.Bastion)
        {
            if (!Enum.IsDefined(typeof(DungeonLayoutId), layout)) throw new ArgumentOutOfRangeException(nameof(layout));
            LayoutId = layout;
            if (layout == DungeonLayoutId.Bastion)
            { landmarks = BastionLandmarks; SpawnPoints = Array.AsReadOnly(BastionSpawns); LayoutName = "EMBER BASTION";
                VisualIdentityName="FORGE CITADEL";VisualIdentitySignature="ember-trenches/crenellated-hearth";AccentColor = new Color(.96f,.48f,.18f,1); }
            else if (layout == DungeonLayoutId.Galleries)
            { landmarks = GalleryLandmarks; SpawnPoints = Array.AsReadOnly(GallerySpawns); LayoutName = "SPLIT GALLERIES";
                VisualIdentityName="SUNKEN ARCHIVE";VisualIdentitySignature="azure-canals/bridge-plinths";AccentColor = new Color(.30f,.72f,1f,1); }
            else
            { landmarks = CrucibleLandmarks; SpawnPoints = Array.AsReadOnly(CrucibleSpawns); LayoutName = "RITUAL CRUCIBLE";
                VisualIdentityName="ECLIPSE SANCTUM";VisualIdentitySignature="ritual-rings/radial-altars";AccentColor = new Color(1f,.30f,.68f,1); }
            var blocked = new List<GridCell>();
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
                        blocked.Add(new GridCell(x, y));
            foreach (Vector2 center in landmarks)
            {
                GridCell low = Cell(center - Vector2.one);
                for (int x = low.X; x < low.X + 2; x++)
                    for (int y = low.Y; y < low.Y + 2; y++) blocked.Add(new GridCell(x, y));
            }
            Navigation = new DungeonNavigation(Width, Height, blocked);
            foreach (Vector2 point in SpawnPoints)
                if (!Navigation.IsWalkable(Cell(point)) || Navigation.FindPath(Cell(Entrance), Cell(point)).Count == 0)
                    throw new InvalidOperationException("Dungeon layout has an unreachable spawn: " + layout + " at " + point);
            foreach (GridCell cell in blocked)
                obstacles.Add(new Rect(Origin.x + cell.X - .15f, Origin.y + cell.Y - .15f, 1.3f, 1.3f));

            Texture2D room = Resources.Load<Texture2D>("AffixOriginal/TempleRoom-v2");
            Texture2D rubble = Resources.Load<Texture2D>("AffixOriginal/TempleObstacle-v2");
            if (room == null || rubble == null) throw new InvalidOperationException("Original temple world art is missing.");
            Rect crop = new Rect(256, 192, 1024, 640);
            floorSprite = Sprite.Create(room, crop, new Vector2(.5f, .5f), crop.width / 24f);
            obstacleSprite = Sprite.Create(rubble, new Rect(0, 0, rubble.width, rubble.height), new Vector2(.5f, .5f), rubble.width / 2.4f);
            identityTexture = new Texture2D(1,1,TextureFormat.RGBA32,false){name="Procedural dungeon identity pixel",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
            identityTexture.SetPixel(0,0,Color.white);identityTexture.Apply(false,true);
            identitySprite = Sprite.Create(identityTexture,new Rect(0,0,1,1),new Vector2(.5f,.5f),1);
            scenery = new GameObject("Connected Temple Dungeon");
            identityRoot = new GameObject("Visual identity - "+VisualIdentityName);identityRoot.transform.SetParent(scenery.transform,false);
            Color floorTint = layout == DungeonLayoutId.Bastion ? new Color(1f,.84f,.68f,1) :
                layout == DungeonLayoutId.Galleries ? new Color(.72f,.88f,1f,1) : new Color(.94f,.72f,1f,1);
            for (int row = 0; row < 2; row++)
                for (int column = 0; column < 3; column++)
                {
                    var tile = new GameObject("Temple floor " + column + ":" + row, typeof(SpriteRenderer));
                    tile.transform.SetParent(scenery.transform, false);
                    tile.transform.position = new Vector3(-24 + column * 24, -8 + row * 16, 0);
                    tile.transform.localScale = new Vector3(1, 1.4f, 1);
                    var renderer = tile.GetComponent<SpriteRenderer>();
                    renderer.sprite = floorSprite; renderer.sortingOrder = -10000;
                    renderer.color = ((column + row) & 1) == 0 ? floorTint : Color.Lerp(floorTint, Color.gray, .13f);
                }
            for (int i = 0; i < landmarks.Length; i++)
            {
                Vector2 point = landmarks[i];
                var obstacle = new GameObject("Navigation landmark " + i, typeof(SpriteRenderer), typeof(BoxCollider2D));
                obstacle.transform.SetParent(scenery.transform, false);
                obstacle.transform.position = point;
                var renderer = obstacle.GetComponent<SpriteRenderer>();
                renderer.sprite = obstacleSprite; renderer.sortingOrder = -(int)(point.y * 100) + 40; renderer.flipX = (i & 1) == 1;
                renderer.color = Color.Lerp(Color.white, AccentColor, .22f);
                obstacle.GetComponent<BoxCollider2D>().size = new Vector2(2, 2);
            }
            BuildVisualIdentity(layout);
            BuildFloorMotifs(layout);
        }

        private void BuildVisualIdentity(DungeonLayoutId layout)
        {
            if(layout==DungeonLayoutId.Bastion)BuildBastionIdentity();
            else if(layout==DungeonLayoutId.Galleries)BuildGalleryIdentity();
            else BuildCrucibleIdentity();
            if(IdentityDecorationCount<12||IdentityColliderCount!=0)
                throw new InvalidOperationException("Procedural visual identity is incomplete or changed collision: "+layout);
        }

        private void BuildBastionIdentity()
        {
            Color coal=new Color(.12f,.055f,.035f,.30f),ember=new Color(1f,.30f,.07f,.48f),brass=new Color(1f,.68f,.22f,.38f);
            AddRect("North heat trench",new Vector2(0,11.7f),new Vector2(61,.72f),coal);
            AddRect("South heat trench",new Vector2(0,-11.7f),new Vector2(61,.72f),coal);
            AddLine("North molten seam",new[]{new Vector2(-30,11.7f),new Vector2(30,11.7f)},ember,.10f);
            AddLine("South molten seam",new[]{new Vector2(-30,-11.7f),new Vector2(30,-11.7f)},ember,.10f);
            for(int i=0;i<9;i++)
            {
                float x=-28+i*7;
                AddRect("North battlement "+i,new Vector2(x,13.5f),new Vector2(2.6f,.7f),new Color(.20f,.12f,.08f,.46f));
                AddRect("South battlement "+i,new Vector2(x,-13.5f),new Vector2(2.6f,.7f),new Color(.20f,.12f,.08f,.46f));
            }
            AddRect("Forge hearth",Vector2.zero,new Vector2(6.4f,2.5f),new Color(.16f,.08f,.05f,.24f));
            AddLine("Forge hearth frame",new[]{new Vector2(-3.2f,-1.25f),new Vector2(3.2f,-1.25f),new Vector2(3.2f,1.25f),new Vector2(-3.2f,1.25f)},brass,.12f,true);
            AddLine("Forge strike",new[]{new Vector2(-2.2f,0),new Vector2(2.2f,0)},ember,.18f);
            AddDiamond("Forge crest",Vector2.zero,1.05f,brass);
            Vector2 entranceSeal=new Vector2(-25.5f,-8.5f);
            AddRect("Entrance forge seal",entranceSeal,new Vector2(5.2f,2.8f),new Color(.13f,.06f,.035f,.20f));
            AddLine("Entrance forge frame",new[]{entranceSeal+new Vector2(-2.6f,-1.4f),entranceSeal+new Vector2(2.6f,-1.4f),
                entranceSeal+new Vector2(2.6f,1.4f),entranceSeal+new Vector2(-2.6f,1.4f)},brass,.085f,true);
            AddDiamond("Entrance forge crest",entranceSeal,1.15f,ember);
            AddLine("Entrance forge spark",new[]{entranceSeal+Vector2.left*1.8f,entranceSeal+Vector2.right*1.8f},brass,.07f);
        }

        private void BuildGalleryIdentity()
        {
            Color water=new Color(.04f,.20f,.30f,.28f),edge=new Color(.24f,.82f,1f,.46f),bridge=new Color(.58f,.78f,.86f,.28f);
            float[] channels={-14f,0f,14f};
            for(int i=0;i<channels.Length;i++)
            {
                float x=channels[i];AddRect("Archive canal "+i,new Vector2(x,0),new Vector2(1.15f,27),water);
                AddLine("Canal west edge "+i,new[]{new Vector2(x-.58f,-13.5f),new Vector2(x-.58f,13.5f)},edge,.07f);
                AddLine("Canal east edge "+i,new[]{new Vector2(x+.58f,-13.5f),new Vector2(x+.58f,13.5f)},edge,.07f);
                for(int bridgeIndex=0;bridgeIndex<3;bridgeIndex++)
                    AddRect("Canal bridge "+i+":"+bridgeIndex,new Vector2(x,-8+bridgeIndex*8),new Vector2(2.2f,.65f),bridge);
            }
            for(int i=0;i<6;i++)
            {
                float x=-27+i*10.8f;
                AddLine("Archive plinth "+i,new[]{new Vector2(x-1.4f,11.2f),new Vector2(x+1.4f,11.2f),new Vector2(x+1.4f,13.2f),new Vector2(x-1.4f,13.2f)},edge,.09f,true);
                AddDiamond("Archive lens "+i,new Vector2(x,12.2f),.42f,new Color(.55f,.92f,1f,.60f));
            }
        }

        private void BuildCrucibleIdentity()
        {
            Color voidFill=new Color(.18f,.03f,.16f,.18f),rite=new Color(.92f,.20f,.58f,.29f),gold=new Color(.90f,.56f,.22f,.28f);
            AddRing("Outer ritual court",Vector2.zero,10.8f,rite,.075f);
            AddRing("Inner ritual court",Vector2.zero,5.8f,gold,.065f);
            AddRing("Eclipse well",Vector2.zero,2.1f,rite,.095f);
            AddRect("Eclipse well shade",Vector2.zero,new Vector2(3.1f,3.1f),voidFill,45);
            for(int i=0;i<8;i++)
            {
                float angle=i*Mathf.PI/4f;Vector2 direction=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
                AddLine("Ritual spoke "+i,new[]{direction*2.3f,direction*10.5f},i%2==0?rite:gold,.045f);
            }
            Vector2[] altars={new Vector2(0,12),new Vector2(0,-12),new Vector2(14,0),new Vector2(-14,0)};
            for(int i=0;i<altars.Length;i++)
            {
                AddDiamond("Radial altar "+i,altars[i],1.25f,gold);
                AddRing("Altar halo "+i,altars[i],.62f,rite,.065f);
            }
        }

        private void AddRect(string name,Vector2 center,Vector2 size,Color color,float rotation=0)
        {
            var host=new GameObject(name,typeof(SpriteRenderer));host.transform.SetParent(identityRoot.transform,false);
            host.transform.position=center;host.transform.localScale=new Vector3(size.x,size.y,1);host.transform.rotation=Quaternion.Euler(0,0,rotation);
            SpriteRenderer renderer=host.GetComponent<SpriteRenderer>();renderer.sprite=identitySprite;renderer.color=color;renderer.sortingOrder=-9700;
            IdentityDecorationCount++;
        }

        private void AddDiamond(string name,Vector2 center,float radius,Color color)
        {
            AddLine(name,new[]{center+Vector2.up*radius,center+Vector2.right*radius,center+Vector2.down*radius,center+Vector2.left*radius},color,.10f,true);
        }

        private void AddRing(string name,Vector2 center,float radius,Color color,float width)
        {
            var points=new Vector2[48];for(int i=0;i<points.Length;i++){float angle=Mathf.PI*2*i/points.Length;points[i]=center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;}
            AddLine(name,points,color,width,true);
        }

        private void AddLine(string name,IReadOnlyList<Vector2> points,Color color,float width,bool loop=false)
        {
            Shader shader=Shader.Find("Sprites/Default");if(shader==null)throw new InvalidOperationException("Built-in sprite shader missing for dungeon identity.");
            var host=new GameObject(name,typeof(LineRenderer));host.transform.SetParent(identityRoot.transform,false);
            LineRenderer line=host.GetComponent<LineRenderer>();var material=new Material(shader);materials.Add(material);
            line.sharedMaterial=material;line.useWorldSpace=true;line.positionCount=points.Count;line.loop=loop;
            line.startWidth=line.endWidth=width;line.startColor=line.endColor=color;line.sortingOrder=-9600;
            for(int i=0;i<points.Count;i++)line.SetPosition(i,points[i]);IdentityDecorationCount++;
        }

        private void BuildFloorMotifs(DungeonLayoutId layout)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) return;
            Vector2[][] paths = layout == DungeonLayoutId.Bastion ? new[]
            {
                new[]{new Vector2(-31,-12),new Vector2(-18,-12),new Vector2(-6,-2),new Vector2(8,-2),new Vector2(20,9),new Vector2(31,9)},
                new[]{new Vector2(-28,10),new Vector2(-10,10),new Vector2(2,2),new Vector2(16,2),new Vector2(29,-10)}
            } : layout == DungeonLayoutId.Galleries ? new[]
            {
                new[]{new Vector2(-31,-12),new Vector2(-25,-5),new Vector2(-13,-5),new Vector2(-3,3),new Vector2(13,3),new Vector2(27,10)},
                new[]{new Vector2(-29,10),new Vector2(-17,4),new Vector2(-2,-5),new Vector2(14,-5),new Vector2(30,-11)}
            } : new[]
            {
                new[]{new Vector2(-31,-12),new Vector2(-18,-4),new Vector2(-8,-8),new Vector2(0,-5),new Vector2(8,-8),new Vector2(18,-4),new Vector2(31,-12)},
                new[]{new Vector2(-25,10),new Vector2(-12,5),new Vector2(-7,0),new Vector2(0,6),new Vector2(7,0),new Vector2(12,5),new Vector2(25,10)}
            };
            for (int i = 0; i < paths.Length; i++)
            {
                var host = new GameObject("Layout inlay " + i, typeof(LineRenderer)); host.transform.SetParent(scenery.transform, false);
                var line = host.GetComponent<LineRenderer>(); var material = new Material(shader); materials.Add(material);
                line.sharedMaterial = material; line.useWorldSpace = true; line.positionCount = paths[i].Length;
                line.startWidth = line.endWidth = .055f; line.sortingOrder = -9000;
                Color color = AccentColor; color.a = .17f; line.startColor = line.endColor = color;
                for (int p = 0; p < paths[i].Length; p++) line.SetPosition(p, paths[i][p]);
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
            int index = Math.Abs((actorIndex * 7 + step * 11) % SpawnPoints.Count);
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
            Vector2 nextCenter = Center(path[0]);
            Vector2 step = nextCenter - ownCenter;
            // Align only the axis perpendicular to the first cardinal step. Re-centering both axes
            // after every partial step pulls the actor backward and can oscillate at center +/- 0.12.
            if (Mathf.Abs(step.x) > Mathf.Abs(step.y))
            {
                if (Mathf.Abs(from.y - ownCenter.y) > .04f) return new Vector2(from.x, ownCenter.y);
            }
            else if (Mathf.Abs(from.x - ownCenter.x) > .04f) return new Vector2(ownCenter.x, from.y);
            return nextCenter;
        }
        public void Dispose()
        {
            if (scenery != null) UnityEngine.Object.Destroy(scenery);
            if (floorSprite != null) UnityEngine.Object.Destroy(floorSprite);
            if (obstacleSprite != null) UnityEngine.Object.Destroy(obstacleSprite);
            if (identitySprite != null) UnityEngine.Object.Destroy(identitySprite);
            if (identityTexture != null) UnityEngine.Object.Destroy(identityTexture);
            foreach (Material material in materials) if (material != null) UnityEngine.Object.Destroy(material);
        }
    }
}
