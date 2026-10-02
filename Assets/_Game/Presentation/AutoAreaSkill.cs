using System;
using System.Collections;
using System.Collections.Generic;
using AffixZero.Core;
using UnityEngine;

namespace AffixZero.Presentation
{
    // Independent automatic skill. Only real living targets within the collision-aware radius receive damage.
    public sealed class AutoAreaSkill : MonoBehaviour
    {
        public const float BaseRadius=3.2f;
        private MeleeActor hero;
        private IReadOnlyList<MeleeActor> targets;
        private AutoHuntDirector hunt;
        private LineRenderer path;
        private Material pathMaterial;
        private float pathUntil;
        private int quakeGeneration;
        public int CastCount {get;private set;}
        public int ChainCasts {get;private set;}
        public int PierceCasts {get;private set;}
        public int QuakeCasts {get;private set;}
        public int QuakeOuterHits {get;private set;}
        public int RadialCasts {get;private set;}
        public int TrajectoryHits {get;private set;}
        public int DuplicateCandidatesRejected {get;private set;}
        public int LastUniqueTargets {get;private set;}
        public AreaSkillTrajectory LastTrajectory {get;private set;}
        public bool IsTrajectoryVisible=>path!=null&&path.enabled;
        public float CooldownRemaining {get;private set;}=2.6f;
        public float CooldownDuration=>hero==null?4.5f:4.5f/hero.AttackSpeedMultiplier;
        public void Configure(MeleeActor actor,IReadOnlyList<MeleeActor> enemies,AutoHuntDirector director)
        {hero=actor;targets=enemies;hunt=director;}
        public void ResetForRun(){quakeGeneration++;CooldownRemaining=hunt==null?2.6f:hunt.AreaSkillArmingDelay;}
        private void Update()
        {
            if(path!=null&&path.enabled&&Time.unscaledTime>=pathUntil)path.enabled=false;
            if(hero==null||hero.IsDead||hunt==null||!hunt.Running||Time.deltaTime<=0)return;
            CooldownRemaining=Mathf.Max(0,CooldownRemaining-Time.deltaTime);
            if(CooldownRemaining>0||hunt.Phase!=HuntPhase.Fighting)return;
            int power=(int)Math.Min(int.MaxValue,Math.Round(hero.Damage*hunt.AreaSkillDamageMultiplier,MidpointRounding.AwayFromZero));
            AreaSkillTrajectory trajectory=hunt.AreaTrajectory;
            int hits=0;bool cast;
            if(trajectory==AreaSkillTrajectory.Chain){hits=CastChain(power);cast=hits>0;}
            else if(trajectory==AreaSkillTrajectory.Pierce){hits=CastPierce(power);cast=hits>0;}
            else if(trajectory==AreaSkillTrajectory.Quake)cast=CastQuake(power,out hits);
            else {hits=CastRadial(power);cast=hits>0;}
            if(!cast)return;
            LastTrajectory=trajectory;if(trajectory!=AreaSkillTrajectory.Radial)TrajectoryHits+=hits;CastCount++;CooldownRemaining=CooldownDuration;
            if(trajectory==AreaSkillTrajectory.Chain)ChainCasts++;
            else if(trajectory==AreaSkillTrajectory.Pierce)PierceCasts++;
            else if(trajectory==AreaSkillTrajectory.Quake)QuakeCasts++;
            else RadialCasts++;
        }

        private int CastRadial(int power)
        {
            float radius=hunt.AreaSkillRadius;int nearby=0;
            foreach(MeleeActor target in targets)if(EligibleFromHero(target,radius))nearby++;
            if(nearby<hunt.AreaSkillMinimumTargets)return 0;
            LastUniqueTargets=nearby;
            return hero.CastAreaStrike(targets,radius,power);
        }

        private bool CastQuake(int power,out int hits)
        {
            float radius=hunt.AreaSkillRadius,innerRadius=radius*.55f;var inner=new List<MeleeActor>();int eligible=0;
            foreach(MeleeActor target in targets)
            {
                if(!EligibleFromHero(target,radius))continue;eligible++;
                if(Vector2.Distance(hero.transform.position,target.transform.position)<=innerRadius)inner.Add(target);
            }
            if(eligible<hunt.AreaSkillMinimumTargets){hits=0;return false;}
            var powers=new List<int>();for(int i=0;i<inner.Count;i++)powers.Add(power);
            hits=hero.CastSkillSequence(inner,powers,innerRadius,out int duplicates);DuplicateCandidatesRejected+=duplicates;
            LastUniqueTargets=eligible;RenderRing(innerRadius,new Color(1f,.72f,.16f,.95f));
            StartCoroutine(ResolveQuakeOuter(power,new HashSet<MeleeActor>(inner),innerRadius,radius,quakeGeneration));return true;
        }

        private IEnumerator ResolveQuakeOuter(int power,HashSet<MeleeActor> inner,float innerRadius,float radius,int generation)
        {
            yield return new WaitForSeconds(.18f);
            if(generation!=quakeGeneration||hero==null||hero.IsDead||hunt==null||!hunt.Running||hunt.Phase!=HuntPhase.Fighting)yield break;
            var outer=new List<MeleeActor>();
            foreach(MeleeActor target in targets)
            {
                if(target==null||inner.Contains(target)||!EligibleFromHero(target,radius))continue;
                if(Vector2.Distance(hero.transform.position,target.transform.position)>innerRadius)outer.Add(target);
            }
            var powers=new List<int>();int outerPower=Math.Max(1,(int)Math.Round(power*.65f,MidpointRounding.AwayFromZero));
            for(int i=0;i<outer.Count;i++)powers.Add(outerPower);
            int hits=hero.CastSkillSequence(outer,powers,radius,out int duplicates);DuplicateCandidatesRejected+=duplicates;
            QuakeOuterHits+=hits;TrajectoryHits+=hits;if(hits>0)RenderRing(radius,new Color(1f,.32f,.08f,.95f));
        }

        private int CastChain(int power)
        {
            var chosen=new List<MeleeActor>();var powers=new List<int>();var points=new List<Vector2>();
            float initialRange=hunt.AreaSkillRadius;
            MeleeActor first=EligibleFromHero(hero.CurrentTarget,initialRange)?hero.CurrentTarget:Nearest(hero.transform.position,initialRange,null);
            if(first==null)return 0;
            chosen.Add(first);points.Add(hero.transform.position);points.Add((Vector2)first.transform.position);
            Vector2 source=first.transform.position;
            while(chosen.Count<hunt.AreaSkillMaxTargets)
            {
                MeleeActor next=Nearest(source,hunt.AreaSkillJumpRange,chosen);
                if(next==null)break;chosen.Add(next);source=next.transform.position;points.Add(source);
            }
            if(chosen.Count<hunt.AreaSkillMinimumTargets)return 0;
            float decay=hunt.FuryEvolutionTier>1?.82f:.72f;
            for(int i=0;i<chosen.Count;i++)powers.Add(Math.Max(1,(int)Math.Round(power*Math.Pow(decay,i),MidpointRounding.AwayFromZero)));
            int hits=hero.CastSkillSequence(chosen,powers,hunt.AreaSkillJumpRange,out int duplicates);
            DuplicateCandidatesRejected+=duplicates;LastUniqueTargets=chosen.Count;
            if(hits>0)RenderPath(points,new Color(.38f,.82f,1f,.95f),.12f);
            return hits;
        }

        private int CastPierce(int power)
        {
            float reach=hunt.AreaSkillPierceReach,width=hunt.AreaSkillPierceWidth;
            MeleeActor aim=EligibleFromHero(hero.CurrentTarget,reach)?hero.CurrentTarget:Nearest(hero.transform.position,reach,null);
            if(aim==null)return 0;
            Vector2 origin=hero.transform.position,delta=(Vector2)aim.transform.position-origin;
            if(delta.sqrMagnitude<.0001f)return 0;
            Vector2 direction=delta.normalized,end=origin+direction*reach;
            var chosen=new List<MeleeActor>();
            foreach(MeleeActor target in targets)
            {
                if(target==null||!target.isActiveAndEnabled||target.IsDead)continue;
                Vector2 offset=(Vector2)target.transform.position-origin;float along=Vector2.Dot(offset,direction);
                if(along<0||along>reach||Mathf.Abs(offset.x*direction.y-offset.y*direction.x)>width||
                    !hunt.World.LineOfSight(origin,target.transform.position))continue;
                chosen.Add(target);
            }
            chosen.Sort((a,b)=>Vector2.Dot((Vector2)a.transform.position-origin,direction).CompareTo(
                Vector2.Dot((Vector2)b.transform.position-origin,direction)));
            if(chosen.Count>hunt.AreaSkillMaxTargets)chosen.RemoveRange(hunt.AreaSkillMaxTargets,chosen.Count-hunt.AreaSkillMaxTargets);
            if(chosen.Count<hunt.AreaSkillMinimumTargets)return 0;
            var powers=new List<int>();int lanePower=Math.Max(1,(int)Math.Round(power*.86f,MidpointRounding.AwayFromZero));
            for(int i=0;i<chosen.Count;i++)powers.Add(lanePower);
            int hits=hero.CastSkillSequence(chosen,powers,width,out int duplicates);
            DuplicateCandidatesRejected+=duplicates;LastUniqueTargets=chosen.Count;
            if(hits>0)RenderPath(new List<Vector2>{origin,end},new Color(1f,.32f,.72f,.95f),width*.65f);
            return hits;
        }

        private bool EligibleFromHero(MeleeActor target,float range)=>target!=null&&target.isActiveAndEnabled&&!target.IsDead&&
            Vector2.Distance(hero.transform.position,target.transform.position)<=range&&hunt.World.LineOfSight(hero.transform.position,target.transform.position);
        private MeleeActor Nearest(Vector2 source,float range,IReadOnlyList<MeleeActor> excluded)
        {
            MeleeActor best=null;float bestDistance=range*range;
            foreach(MeleeActor target in targets)
            {
                if(target==null||!target.isActiveAndEnabled||target.IsDead||Contains(excluded,target)||
                    !hunt.World.LineOfSight(source,target.transform.position))continue;
                float distance=((Vector2)target.transform.position-source).sqrMagnitude;
                if(distance<=bestDistance){bestDistance=distance;best=target;}
            }
            return best;
        }
        private static bool Contains(IReadOnlyList<MeleeActor> values,MeleeActor value)
        {if(values==null)return false;for(int i=0;i<values.Count;i++)if(values[i]==value)return true;return false;}

        private void RenderPath(IReadOnlyList<Vector2> points,Color color,float width)
        {
            EnsurePath();path.loop=false;path.positionCount=points.Count;path.startColor=path.endColor=color;
            path.startWidth=path.endWidth=width;
            for(int i=0;i<points.Count;i++)path.SetPosition(i,points[i]+Vector2.up*.45f);
            path.enabled=true;pathUntil=Time.unscaledTime+.32f;
        }
        private void RenderRing(float radius,Color color)
        {
            EnsurePath();path.loop=true;path.positionCount=41;path.startColor=path.endColor=color;path.startWidth=path.endWidth=.11f;
            Vector2 origin=hero.transform.position;
            for(int i=0;i<41;i++){float angle=Mathf.PI*2*i/41;path.SetPosition(i,origin+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius);}
            path.enabled=true;pathUntil=Time.unscaledTime+.25f;
        }
        private void EnsurePath()
        {
            if(path!=null)return;Shader shader=Shader.Find("Sprites/Default");
            if(shader==null)throw new InvalidOperationException("Built-in sprite shader missing for player skill trajectory.");
            pathMaterial=new Material(shader);path=gameObject.AddComponent<LineRenderer>();path.sharedMaterial=pathMaterial;
            path.useWorldSpace=true;path.sortingOrder=1996;path.numCapVertices=3;path.numCornerVertices=3;path.enabled=false;
        }
        private void OnDestroy(){if(pathMaterial!=null)Destroy(pathMaterial);}
    }
}
