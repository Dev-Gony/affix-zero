using System;
using System.Collections.Generic;
using AffixZero.Core;
using UnityEngine;

namespace AffixZero.Presentation
{
    // One authored courtyard with three patrol areas. The same grid drives movement and blockers.
    public sealed class DungeonWorld : IDisposable
    {
        public const int Width=28, Height=16;
        public static readonly Vector2 Origin=new Vector2(-14,-8);
        public readonly DungeonNavigation Navigation;
        private readonly GameObject scenery;
        private readonly Sprite obstacleSprite;
        private readonly List<Rect> obstacles=new List<Rect>();
        public int DetourQueries { get; private set; }

        public DungeonWorld()
        {
            var blocked=new List<GridCell>();
            // The four raised corner platforms and the two new rubble barriers are impassable.
            for(int x=0;x<Width;x++)for(int y=0;y<Height;y++)
                if((x<3||x>=Width-3)&&(y<2||y>=Height-2))blocked.Add(new GridCell(x,y));
            foreach(int left in new[]{9,17})for(int x=left;x<left+2;x++)for(int y=7;y<9;y++)blocked.Add(new GridCell(x,y));
            Navigation=new DungeonNavigation(Width,Height,blocked);
            foreach(var cell in blocked)obstacles.Add(new Rect(Origin.x+cell.X-.15f,Origin.y+cell.Y-.15f,1.3f,1.3f));
            var texture=Resources.Load<Texture2D>("AffixGenerated/TempleObstacle-v1");
            if(texture==null)throw new InvalidOperationException("Authored navigation obstacle art is missing.");
            obstacleSprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f),texture.width/2.4f);
            scenery=new GameObject("Courtyard Navigation Obstacles");
            foreach(float x in new[]{-4f,4f})
            {
                var obstacle=new GameObject("Impassable Stone Rubble",typeof(SpriteRenderer),typeof(BoxCollider2D));
                obstacle.transform.SetParent(scenery.transform,false);
                obstacle.transform.position=new Vector3(x,0,0);
                var view=obstacle.GetComponent<SpriteRenderer>();view.sprite=obstacleSprite;view.sortingOrder=40;
                obstacle.GetComponent<BoxCollider2D>().size=new Vector2(2,2);
            }
        }

        public GridCell Cell(Vector2 point)=>new GridCell(Mathf.FloorToInt(point.x-Origin.x),Mathf.FloorToInt(point.y-Origin.y));
        public Vector2 Center(GridCell cell)=>Origin+new Vector2(cell.X+.5f,cell.Y+.5f);
        public bool IsWalkable(Vector2 point)=>Navigation.IsWalkable(Cell(point));
        private bool Fits(Vector2 p)=>IsWalkable(p)&&IsWalkable(p+Vector2.right*.15f)&&IsWalkable(p-Vector2.right*.15f)&&IsWalkable(p+Vector2.up*.15f)&&IsWalkable(p-Vector2.up*.15f);
        public bool LineOfSight(Vector2 from,Vector2 to)
        {
            if(!Fits(from)||!Fits(to))return false;
            foreach(var box in obstacles)
            {
                float near=0,far=1;Vector2 delta=to-from;
                if(ClipAxis(from.x,delta.x,box.xMin,box.xMax,ref near,ref far)&&
                    ClipAxis(from.y,delta.y,box.yMin,box.yMax,ref near,ref far))return false;
            }
            return true;
        }
        private static bool ClipAxis(float start,float delta,float min,float max,ref float near,ref float far)
        {
            if(Mathf.Abs(delta)<.00001f)return start>=min&&start<=max;
            float a=(min-start)/delta,b=(max-start)/delta;
            near=Mathf.Max(near,Mathf.Min(a,b));far=Mathf.Min(far,Mathf.Max(a,b));return near<=far;
        }
        public Vector2 NextWaypoint(Vector2 from,Vector2 destination)
        {
            if(!IsWalkable(from)||!IsWalkable(destination))return from;
            if(LineOfSight(from,destination))return destination;
            var path=Navigation.FindPath(Cell(from),Cell(destination));
            if(path.Count==0)return from;
            DetourQueries++;
            for(int i=path.Count-1;i>=0;i--)
            {
                Vector2 candidate=Center(path[i]);
                if(LineOfSight(from,candidate))return candidate;
            }
            Vector2 ownCenter=Center(Cell(from));
            return LineOfSight(from,ownCenter)?ownCenter:from;
        }
        public void Dispose()
        {
            if(scenery!=null)UnityEngine.Object.Destroy(scenery);
            if(obstacleSprite!=null)UnityEngine.Object.Destroy(obstacleSprite);
        }
    }
}
