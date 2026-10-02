using UnityEngine;

namespace AffixZero.Presentation
{
    // A second autonomous skill: low-health recovery with a visible, queryable cooldown.
    public sealed class AutoRecoverySkill : MonoBehaviour
    {
        private MeleeActor hero;
        private AutoHuntDirector hunt;
        public int CastCount { get; private set; }
        public float CooldownRemaining { get; private set; }
        public float CooldownDuration => hero==null?14f:14f/hero.AttackSpeedMultiplier;
        public void Configure(MeleeActor actor,AutoHuntDirector director){hero=actor;hunt=director;}
        public void ResetForRun(){CooldownRemaining=8f;}
        private void Update()
        {
            if(hero==null||hero.IsDead||hunt==null||!hunt.Running||Time.deltaTime<=0)return;
            CooldownRemaining=Mathf.Max(0,CooldownRemaining-Time.deltaTime);
            if(CooldownRemaining>0||hero.Hp*100>hero.MaxHp*45)return;
            int healed=hero.Heal(Mathf.Max(12,hero.MaxHp/5));
            if(healed>0){CastCount++;CooldownRemaining=CooldownDuration;}
        }
    }
}
