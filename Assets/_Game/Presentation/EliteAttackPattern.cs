using System;
using UnityEngine;

namespace AffixZero.Presentation
{
    // Three original layout-bound attacks. Geometry is procedural and every hit uses the same
    // CombatHealth receipt path as ordinary melee, so telegraphs and damage cannot diverge.
    [DefaultExecutionOrder(-20)]
    public sealed class EliteAttackPattern : MonoBehaviour
    {
        private MeleeActor actor,hero;
        private EliteEncounterStyle style;
        private bool guardian,active,telegraphing;
        private float cooldown,telegraph;
        private Vector2 origin,direction,end;
        private LineRenderer line;
        private Material material;
        public bool Active=>active;
        public bool IsTelegraphing=>active&&telegraphing;
        public EliteEncounterStyle Style=>active?style:EliteEncounterStyle.None;
        public int CastCount{get;private set;}
        public int HitCount{get;private set;}
        public event Action<EliteEncounterStyle,bool,bool> Resolved;

        public void Configure(MeleeActor source,MeleeActor target,EliteEncounterStyle value,bool isGuardian)
        {
            if(source==null||target==null||value==EliteEncounterStyle.None)throw new ArgumentNullException(nameof(source));
            actor=source;hero=target;style=value;guardian=isGuardian;active=true;telegraphing=false;
            cooldown=2.4f+(actor.ActorId%5)*.22f;actor.SetPatternLocked(false);EnsureRenderer();line.enabled=false;
        }

        public void Clear()
        {
            active=telegraphing=false;if(actor!=null)actor.SetPatternLocked(false);if(line!=null)line.enabled=false;
        }

        private void Update()
        {
            if(!active||actor==null||hero==null||actor.IsDead||hero.IsDead||Time.deltaTime<=0)
            {if(line!=null)line.enabled=false;return;}
            if(telegraphing)
            {
                telegraph-=Time.deltaTime;DrawTelegraph();
                if(telegraph<=0)Resolve();return;
            }
            cooldown=Mathf.Max(0,cooldown-Time.deltaTime);
            if(cooldown>0||actor.CurrentTarget!=hero||actor.IsAttacking)return;
            Vector2 delta=(Vector2)hero.transform.position-(Vector2)actor.transform.position;
            float limit=style==EliteEncounterStyle.EmberBulwark?(guardian?2.5f:2.15f):
                style==EliteEncounterStyle.GalleryStalker?(guardian?3.8f:3.35f):(guardian?5.8f:4.8f);
            if(delta.sqrMagnitude>.04f&&delta.sqrMagnitude<=limit*limit&&actor.HasLineOfSight(hero.transform.position))
                Begin(delta.normalized,limit);
        }

        private void Begin(Vector2 aim,float limit)
        {
            telegraphing=true;actor.SetPatternLocked(true);origin=actor.transform.position;direction=aim;
            end=style==EliteEncounterStyle.EmberBulwark?origin:origin+direction*limit;
            telegraph=style==EliteEncounterStyle.GalleryStalker?.34f:guardian?.48f:.58f;
            EnsureRenderer();line.enabled=true;DrawTelegraph();
        }

        private void Resolve()
        {
            telegraphing=false;actor.SetPatternLocked(false);line.enabled=false;CastCount++;
            bool hit=false;
            if(style==EliteEncounterStyle.EmberBulwark)
            {
                float radius=guardian?2.5f:2.15f;
                if(Vector2.Distance(origin,hero.transform.position)<=radius&&actor.HasLineOfSight(hero.transform.position))
                    hit=actor.ResolvePatternHit(hero,Power(.50f),radius,true).Accepted;
            }
            else if(style==EliteEncounterStyle.GalleryStalker)
            {
                Vector2 target=end-direction*.45f;
                actor.TryPatternMove(target);
                float radius=guardian?1.05f:.88f;
                if(Vector2.Distance(actor.transform.position,hero.transform.position)<=radius&&actor.HasLineOfSight(hero.transform.position))
                    hit=actor.ResolvePatternHit(hero,Power(.60f),radius,false).Accepted;
            }
            else
            {
                float width=guardian?.62f:.46f;
                if(DistanceToSegment(hero.transform.position,origin,end)<=width&&actor.HasLineOfSight(hero.transform.position))
                    hit=actor.ResolvePatternHit(hero,Power(.45f),width,true).Accepted;
            }
            if(hit)HitCount++;Resolved?.Invoke(style,guardian,hit);
            cooldown=style==EliteEncounterStyle.EmberBulwark?(guardian?4.5f:5.2f):
                style==EliteEncounterStyle.GalleryStalker?(guardian?3.2f:3.8f):(guardian?4.0f:4.7f);
        }

        private int Power(float fraction)=>Mathf.Max(1,Mathf.RoundToInt(actor.Damage*fraction));

        private void DrawTelegraph()
        {
            EnsureRenderer();
            Color color=style==EliteEncounterStyle.EmberBulwark?new Color(1f,.32f,.08f,.9f):
                style==EliteEncounterStyle.GalleryStalker?new Color(.2f,.72f,1f,.9f):new Color(1f,.2f,.68f,.9f);
            color.a=.55f+.35f*Mathf.Abs(Mathf.Sin(Time.time*15));line.startColor=line.endColor=color;
            line.startWidth=line.endWidth=style==EliteEncounterStyle.RitualReaver?(guardian?.24f:.17f):.09f;
            if(style==EliteEncounterStyle.EmberBulwark)
            {
                int count=41;float radius=guardian?2.5f:2.15f;line.loop=true;line.positionCount=count;
                for(int i=0;i<count;i++){float a=Mathf.PI*2*i/count;line.SetPosition(i,origin+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius);}
            }
            else
            {
                line.loop=false;line.positionCount=2;line.SetPosition(0,origin+Vector2.up*.35f);line.SetPosition(1,end+Vector2.up*.35f);
            }
        }

        private void EnsureRenderer()
        {
            if(line!=null)return;Shader shader=Shader.Find("Sprites/Default");
            if(shader==null)throw new InvalidOperationException("Built-in sprite shader missing for elite attack pattern.");
            material=new Material(shader);line=gameObject.AddComponent<LineRenderer>();line.sharedMaterial=material;
            line.useWorldSpace=true;line.sortingOrder=1995;line.numCapVertices=3;line.numCornerVertices=3;line.enabled=false;
        }

        private static float DistanceToSegment(Vector2 point,Vector2 a,Vector2 b)
        {
            Vector2 span=b-a;if(span.sqrMagnitude<.000001f)return Vector2.Distance(point,a);
            float t=Mathf.Clamp01(Vector2.Dot(point-a,span)/span.sqrMagnitude);
            return Vector2.Distance(point,a+span*t);
        }

        private void OnDisable(){if(actor!=null)actor.SetPatternLocked(false);if(line!=null)line.enabled=false;telegraphing=false;}
        private void OnDestroy(){if(material!=null)Destroy(material);}
    }
}
