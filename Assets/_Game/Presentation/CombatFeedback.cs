using UnityEngine;
using System;
using System.Collections.Generic;

namespace AffixZero.Presentation
{
    // Original procedural slash/ring geometry; actor sprite animation remains the licensed source animation.
    public sealed class CombatFeedback : MonoBehaviour
    {
        private sealed class Pulse
        {
            public LineRenderer line;
            public Vector2 origin,end;
            public float age,duration,radius,angle;
            public bool ring,beam,active;
            public Color color;
        }
        private readonly Pulse[] pulses=new Pulse[20];
        private MeleeActor hero;
        private IReadOnlyList<MeleeActor> actors;
        private Material material;
        private AudioSource audioSource;
        private AudioClip hitClip,criticalClip,killClip;
        private Camera cameraView;
        private Vector3 previousShake;
        private float shake;
        private int cursor;
        private float effectsScale=1f;
        public bool ReducedEffects=>effectsScale<1f;

        public void Configure(MeleeActor actor,IReadOnlyList<MeleeActor> combatants)
        {
            hero=actor;actors=combatants;hero.StrikeResolved+=OnStrike;cameraView=Camera.main;
            Shader shader=Shader.Find("Sprites/Default");
            if(shader==null){Debug.LogError("Built-in sprite shader missing for combat feedback.",this);return;}
            material=new Material(shader);
            audioSource=gameObject.AddComponent<AudioSource>();audioSource.playOnAwake=false;audioSource.spatialBlend=0;
            audioSource.volume=.1f;hitClip=CreateTone("AFFIX hit",120,.055f,.32f);criticalClip=CreateTone("AFFIX critical",190,.085f,.48f);killClip=CreateTone("AFFIX kill",82,.13f,.58f);
            for(int i=0;i<pulses.Length;i++)
            {
                var host=new GameObject("Combat Slash "+i,typeof(LineRenderer));host.transform.SetParent(transform,false);
                var line=host.GetComponent<LineRenderer>();line.sharedMaterial=material;line.useWorldSpace=true;
                line.sortingOrder=2000;line.numCapVertices=3;line.numCornerVertices=2;line.enabled=false;
                pulses[i]=new Pulse{line=line};
            }
            foreach(MeleeActor combatant in combatants)if(combatant!=null){combatant.AttackStarted+=OnAttackStarted;combatant.Damaged+=OnDamaged;}
        }
        public void SetReducedEffects(bool reduced){effectsScale=reduced?.35f:1f;if(audioSource!=null)audioSource.volume=reduced?.045f:.1f;}
        private void OnAttackStarted(MeleeActor attacker,MeleeActor target)
        {
            if(attacker==null||target!=hero||material==null)return;
            Vector2 origin=(Vector2)attacker.transform.position+Vector2.up*.32f;
            Vector2 direction=((Vector2)target.transform.position-(Vector2)attacker.transform.position).normalized;
            float angle=Mathf.Atan2(direction.y,direction.x);
            Emit(origin,Vector2.zero,Mathf.Max(.8f,attacker.AttackReach),angle,false,false,new Color(1f,.18f,.12f,.85f),.24f);
        }
        private void OnDamaged(MeleeActor defender,AffixZero.Core.HitReceipt receipt)
        {
            if(!receipt.Accepted||audioSource==null)return;
            AudioClip clip=receipt.Killed?killClip:defender.LastHitCritical?criticalClip:hitClip;
            audioSource.pitch=receipt.Killed?.88f:defender.LastHitCritical?1.12f:1f+(defender.ActorId%3-.5f)*.025f;
            audioSource.PlayOneShot(clip);
        }
        private void OnStrike(MeleeActor attacker,int hits,int kills,bool area)
        {
            if(hits<=0||material==null)return;
            Vector2 origin=(Vector2)hero.transform.position+Vector2.up*.4f;
            float angle=Mathf.Atan2(hero.LastStrikeDirection.y,hero.LastStrikeDirection.x);
            if(area)
            {
                Emit(origin,Vector2.zero,AutoAreaSkill.Radius,0,true,false,new Color(1,.8f,.3f),.34f);
                Emit(origin,Vector2.zero,AutoAreaSkill.Radius*.78f,0,true,false,new Color(1,.35f,.18f),.24f);
                Emit(origin,Vector2.zero,AutoAreaSkill.Radius*.45f,0,true,false,Color.white,.16f);
            }
            else if(hero.IsRanged)
            {
                Vector2 end=hero.LastStrikePoint+Vector2.up*.4f;
                Emit(origin,end,0,0,false,true,new Color(.5f,.85f,1),.18f);
                Emit(end,Vector2.zero,.5f,0,true,false,new Color(.55f,.8f,1),.2f);
            }
            else
            {
                bool axe=hero.CleaveRadius>2.1f;
                Emit(origin,Vector2.zero,hero.CleaveRadius,angle,false,false,axe?new Color(1,.55f,.18f):new Color(1,.9f,.65f),axe?.26f:.2f);
                Emit(origin,Vector2.zero,hero.CleaveRadius*(axe?.88f:.82f),angle,false,false,axe?new Color(1,.2f,.08f):new Color(1,.45f,.2f),axe?.21f:.16f);
            }
            shake=Mathf.Max(shake,(area?.22f:kills>1?.14f:.07f)*effectsScale);
        }
        private void Emit(Vector2 origin,Vector2 end,float radius,float angle,bool ring,bool beam,Color color,float duration)
        {
            Pulse pulse=pulses[cursor++%pulses.Length];
            pulse.origin=origin;pulse.end=end;pulse.radius=radius;pulse.angle=angle;pulse.ring=ring;pulse.beam=beam;
            pulse.color=color;pulse.duration=duration;pulse.age=0;pulse.active=true;pulse.line.enabled=true;
        }
        private void LateUpdate()
        {
            foreach(Pulse pulse in pulses)
            {
                if(pulse==null||!pulse.active)continue;
                pulse.age+=Time.deltaTime;
                float t=Mathf.Clamp01(pulse.age/pulse.duration);
                if(t>=1){pulse.line.enabled=false;pulse.active=false;continue;}
                Color color=pulse.color;color.a=1-t;pulse.line.startColor=pulse.line.endColor=color;
                pulse.line.startWidth=(pulse.ring?.12f:.2f)*(1-t)+.015f;pulse.line.endWidth=pulse.line.startWidth*.4f;
                if(pulse.beam)
                {pulse.line.positionCount=2;pulse.line.SetPosition(0,pulse.origin);pulse.line.SetPosition(1,pulse.end);continue;}
                int count=pulse.ring?(ReducedEffects?25:49):(ReducedEffects?17:29);pulse.line.positionCount=count;
                float extent=pulse.ring?Mathf.PI*2:Mathf.PI*1.1f;
                float radius=pulse.radius*Mathf.Lerp(pulse.ring?.45f:.7f,1,t);
                for(int i=0;i<count;i++)
                {
                    float a=pulse.angle-extent/2+extent*i/(count-1);
                    pulse.line.SetPosition(i,pulse.origin+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius);
                }
            }
            if(cameraView!=null)
            {
                cameraView.transform.position-=previousShake;
                shake=Mathf.Max(0,shake-Time.deltaTime);
                float magnitude=Mathf.Min(.075f,shake*.34f);
                previousShake=Time.timeScale>0?new Vector3(Mathf.Sin(Time.time*83),Mathf.Cos(Time.time*67),0)*magnitude:Vector3.zero;
                cameraView.transform.position+=previousShake;
            }
        }
        private void OnDestroy()
        {
            if(hero!=null)hero.StrikeResolved-=OnStrike;
            if(actors!=null)foreach(MeleeActor actor in actors)if(actor!=null){actor.AttackStarted-=OnAttackStarted;actor.Damaged-=OnDamaged;}
            if(cameraView!=null)cameraView.transform.position-=previousShake;
            if(material!=null)Destroy(material);
            if(hitClip!=null)Destroy(hitClip);if(criticalClip!=null)Destroy(criticalClip);if(killClip!=null)Destroy(killClip);
        }
        private static AudioClip CreateTone(string name,float frequency,float duration,float amplitude)
        {
            const int rate=22050;int count=Mathf.Max(64,Mathf.CeilToInt(rate*duration));var samples=new float[count];
            for(int i=0;i<count;i++)
            {
                float t=i/(float)rate,envelope=1-i/(float)count;
                float noise=Mathf.Sin(i*12.9898f)*.08f;
                samples[i]=(Mathf.Sin(2*Mathf.PI*frequency*t)+Mathf.Sin(2*Mathf.PI*frequency*2.03f*t)*.32f+noise)*amplitude*envelope*envelope;
            }
            AudioClip clip=AudioClip.Create(name,count,1,rate,false);clip.SetData(samples,0);return clip;
        }
    }
}
