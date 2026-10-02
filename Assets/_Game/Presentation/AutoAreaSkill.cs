using System;
using System.Collections.Generic;
using UnityEngine;

namespace AffixZero.Presentation
{
    // Independent automatic skill. Only real living targets within the collision-aware radius receive damage.
    public sealed class AutoAreaSkill : MonoBehaviour
    {
        public const float Radius=3.2f;
        private MeleeActor hero;
        private IReadOnlyList<MeleeActor> targets;
        private AutoHuntDirector hunt;
        public int CastCount {get;private set;}
        public float CooldownRemaining {get;private set;}=2.6f;
        public float CooldownDuration=>hero==null?4.5f:4.5f/hero.AttackSpeedMultiplier;
        public void Configure(MeleeActor actor,IReadOnlyList<MeleeActor> enemies,AutoHuntDirector director)
        {hero=actor;targets=enemies;hunt=director;}
        public void ResetForRun(){CooldownRemaining=2.6f;}
        private void Update()
        {
            if(hero==null||hero.IsDead||hunt==null||!hunt.Running||Time.deltaTime<=0)return;
            CooldownRemaining=Mathf.Max(0,CooldownRemaining-Time.deltaTime);
            if(CooldownRemaining>0||hunt.Phase!=HuntPhase.Fighting)return;
            int nearby=0;
            foreach(var target in targets)
                if(target!=null&&target.isActiveAndEnabled&&!target.IsDead&&
                    Vector2.Distance(hero.transform.position,target.transform.position)<=Radius&&
                    hunt.World.LineOfSight(hero.transform.position,target.transform.position))nearby++;
            if(nearby<2)return;
            int power=(int)Math.Min(int.MaxValue,(long)hero.Damage+hero.Damage/2);
            if(hero.CastAreaStrike(targets,Radius,power)>0)
            {CastCount++;CooldownRemaining=CooldownDuration;}
        }
    }
}
