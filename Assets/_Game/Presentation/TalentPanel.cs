using System;
using AffixZero.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace AffixZero.Presentation
{
    // Six real, persistent progression nodes. Selection never spends a point by itself.
    public sealed class TalentPanel
    {
        public sealed class View { public int Points,Spent,Fury,Precision,Keystone,Vitality,Cleave,Haste,TotalDamage,TotalHealth; }
        public readonly VisualElement Root;
        public TalentId SelectedTalent { get; private set; }=TalentId.Fury;
        private readonly Func<View> read; private readonly Action<TalentId> invest; private readonly Action reset;
        private readonly VisualElement[] nodes=new VisualElement[6]; private readonly Label[] ranks=new Label[6]; private readonly Texture2D[] icons=new Texture2D[6];
        private readonly Label points,spent,detailName,detailRank,detailEffect,prerequisite,investCaption,summary;
        private readonly Image detailIcon; private readonly VisualElement investButton,resetButton;
        private static readonly TalentId[] Ids={TalentId.Fury,TalentId.Precision,TalentId.Keystone,TalentId.Vitality,TalentId.Cleave,TalentId.Haste};
        private static readonly Color Ink=new Color32(17,19,22,255),Surface=new Color32(26,28,31,255),Highest=new Color32(51,53,56,255),Edge=new Color32(91,64,64,255),Crimson=new Color32(196,30,58,255),Gold=new Color32(233,195,73,255),Cream=new Color32(226,226,230,255),Muted=new Color32(227,190,189,255),Sky=new Color32(151,203,255,255);

        public TalentPanel(VisualElement parent,Func<View> read,Action<TalentId> invest,Action reset,Action close)
        {
            this.read=read??throw new ArgumentNullException(nameof(read));this.invest=invest??throw new ArgumentNullException(nameof(invest));this.reset=reset??throw new ArgumentNullException(nameof(reset));
            if(parent==null||close==null)throw new ArgumentNullException();
            icons[0]=Resources.Load<Texture2D>("AffixGenerated/PowerRune");icons[1]=Resources.Load<Texture2D>("AffixGenerated/PrecisionRune");icons[2]=Resources.Load<Texture2D>("AffixGenerated/VeteranRune");icons[3]=icons[2];icons[4]=icons[0];icons[5]=icons[1];
            Root=Box(parent,"talent-screen",16,64,1248,516,Ink);Root.pickingMode=PickingMode.Position;
            var header=Box(Root,"talent-header",8,8,1232,56,Surface);Box(header,"talent-header-accent",0,0,4,56,Crimson);
            Text(header,"특성 스킬트리",18,5,355,29,23,Cream);Text(header,"OFFENSE · SURVIVAL · AUTO SKILL",19,35,355,15,10,Muted);
            points=Text(header,"",493,16,180,30,16,Gold);spent=Text(header,"",681,18,183,26,13,Cream);
            resetButton=Button(header,"talent-screen-reset","특성 초기화",878,8,194,40,Reset,Highest);Button(header,"talent-screen-close","닫기 [K / ESC]",1084,8,136,40,close,Highest);
            var tree=Box(Root,"talent-tree",8,76,780,432,Surface);Text(tree,"전투 성장 · 최대 75단계",18,10,400,28,18,Cream);Text(tree,"모든 노드는 실제 전투 수치에 적용됩니다.",370,14,390,22,11,Muted).style.unityTextAlign=TextAnchor.MiddleRight;
            string[] names={"분노","정밀","숙련자의 일격","생명력","휩쓸기","가속"};
            for(int i=0;i<Ids.Length;i++){float x=22+(i%3)*250,y=56+(i/3)*166;nodes[i]=Node(tree,"talent-node-"+Ids[i].ToString().ToLowerInvariant(),Ids[i],names[i],icons[i],x,y,out ranks[i]);}
            Text(tree,"공격 · 생존 · 범위 · 자동 스킬 재사용",22,377,470,24,12,Sky);Text(tree,"초반 250 XP / PT · 심화 비용 증가",500,377,258,24,11,Muted).style.unityTextAlign=TextAnchor.MiddleRight;
            var detail=Box(Root,"talent-detail",800,76,440,432,Surface);Box(detail,"talent-detail-accent",0,0,3,432,new Color32(255,179,180,255));
            var iconFrame=Box(detail,"talent-detail-icon",20,20,76,76,Crimson);detailIcon=Icon(iconFrame,null,14,14,48,48);
            Text(detail,"선택한 특성 / PASSIVE",112,17,307,19,11,Gold);detailName=Text(detail,"",112,41,307,34,23,Cream);detailRank=Text(detail,"",112,78,307,21,13,Muted);
            var effect=Box(detail,"talent-effect-panel",20,120,400,112,Ink);Text(effect,"실제 적용 효과",16,12,368,22,12,Gold);detailEffect=Text(effect,"",16,43,368,57,15,Cream);detailEffect.style.whiteSpace=WhiteSpace.Normal;
            prerequisite=Text(detail,"",22,252,394,76,13,Sky);prerequisite.style.whiteSpace=WhiteSpace.Normal;
            investButton=Button(detail,"talent-invest","특성 포인트 투자",20,344,400,52,Invest,Gold);investCaption=investButton.Q<Label>();investCaption.style.color=Ink;investCaption.style.fontSize=16;
            summary=Text(detail,"",22,402,396,20,11,Muted);Root.style.display=DisplayStyle.None;
        }

        public void Refresh(bool visible)
        {
            Root.style.display=visible?DisplayStyle.Flex:DisplayStyle.None;if(!visible)return;View v=read();if(v==null){Root.style.display=DisplayStyle.None;return;}
            points.text="잔여 포인트  "+v.Points;spent.text="투자 "+v.Spent+" / "+HeroProgression.TotalTalentCapacity;
            for(int i=0;i<Ids.Length;i++){var d=HeroProgression.GetTalentDefinition(Ids[i]);int r=Rank(v,Ids[i]);ranks[i].text=r+" / "+d.MaxRank;ShowNode(nodes[i],Ids[i],r>0,Unlocked(v,Ids[i]));}
            var definition=HeroProgression.GetTalentDefinition(SelectedTalent);int rank=Rank(v,SelectedTalent);bool unlocked=Unlocked(v,SelectedTalent);
            detailName.text=definition.Name;detailIcon.image=icons[Array.IndexOf(Ids,SelectedTalent)];detailRank.text="현재 단계  "+rank+" / "+definition.MaxRank;detailEffect.text=definition.Description+"\n"+CurrentEffect(v,SelectedTalent);
            prerequisite.text=definition.Prerequisite.HasValue?"선행 조건: "+HeroProgression.GetTalentDefinition(definition.Prerequisite.Value).Name+" "+definition.RequiredRank+"단계":"선행 조건 없음";
            SetEnabled(investButton,v.Points>0&&unlocked&&rank<definition.MaxRank);investCaption.text=rank>=definition.MaxRank?"최대 단계":!unlocked?"선행 조건 필요":v.Points<1?"포인트 부족":"특성 포인트 투자 · 1 PT";SetEnabled(resetButton,v.Spent>0);
            summary.text="총 공격 "+v.TotalDamage+" · 최대 체력 "+v.TotalHealth+" · 누적 투자 "+v.Spent;
        }
        private static string CurrentEffect(View v,TalentId id){int r=Rank(v,id);switch(id){case TalentId.Fury:return "현재 공격 +"+(r*3);case TalentId.Precision:return "현재 공격 +"+(r*4);case TalentId.Keystone:return "현재 공격 +"+(r*6);case TalentId.Vitality:return "현재 체력 +"+(r*10);case TalentId.Cleave:return "현재 범위 +"+(r*.08f).ToString("0.00");default:return "현재 재사용 시간 -"+(r*2)+"%";}}
        private void Select(TalentId id){SelectedTalent=id;Refresh(true);} private void Invest(){View v=read();var d=HeroProgression.GetTalentDefinition(SelectedTalent);if(v!=null&&v.Points>0&&Unlocked(v,SelectedTalent)&&Rank(v,SelectedTalent)<d.MaxRank)invest(SelectedTalent);Refresh(true);} private void Reset(){View v=read();if(v!=null&&v.Spent>0)reset();Refresh(true);}
        private static int Rank(View v,TalentId id){switch(id){case TalentId.Fury:return v.Fury;case TalentId.Precision:return v.Precision;case TalentId.Keystone:return v.Keystone;case TalentId.Vitality:return v.Vitality;case TalentId.Cleave:return v.Cleave;default:return v.Haste;}}
        private static bool Unlocked(View v,TalentId id){var d=HeroProgression.GetTalentDefinition(id);return !d.Prerequisite.HasValue||Rank(v,d.Prerequisite.Value)>=d.RequiredRank;}
        private void ShowNode(VisualElement n,TalentId id,bool acquired,bool unlocked){Border(n,SelectedTalent==id?Gold:acquired?Crimson:Edge,SelectedTalent==id?2:1);n.style.backgroundColor=acquired?(Color)new Color32(60,26,35,255):Ink;n.style.opacity=unlocked?1:.72f;}
        private VisualElement Node(VisualElement p,string name,TalentId id,string caption,Texture2D texture,float x,float y,out Label rank){var n=Button(p,name,"",x,y,220,142,()=>Select(id),Ink);var well=Box(n,name+"-icon",77,13,66,66,Highest);Icon(well,texture,9,9,48,48);Text(n,caption,8,87,204,27,16,Cream).style.unityTextAlign=TextAnchor.MiddleCenter;rank=Text(n,"",164,6,49,18,10,Gold);rank.style.unityTextAlign=TextAnchor.MiddleCenter;return n;}
        private static void SetEnabled(VisualElement e,bool v){e.SetEnabled(v);e.style.opacity=v?1:.45f;} private static Image Icon(VisualElement p,Texture2D t,float x,float y,float w,float h){var i=new Image{image=t,scaleMode=ScaleMode.ScaleToFit,pickingMode=PickingMode.Ignore};Place(i,x,y,w,h);p.Add(i);return i;}
        private static VisualElement Button(VisualElement p,string n,string c,float x,float y,float w,float h,Action a,Color color){var e=Box(p,n,x,y,w,h,color);Border(e,Edge,1);e.pickingMode=PickingMode.Position;e.focusable=true;Text(e,c,0,0,w,h,13,Cream).style.unityTextAlign=TextAnchor.MiddleCenter;e.RegisterCallback<ClickEvent>(_=>a());e.RegisterCallback<NavigationSubmitEvent>(evt=>{a();evt.StopPropagation();});return e;}
        private static void Place(VisualElement e,float x,float y,float w,float h){e.style.position=Position.Absolute;e.style.left=x;e.style.top=y;e.style.width=w;e.style.height=h;} private static VisualElement Box(VisualElement p,string n,float x,float y,float w,float h,Color c){var e=new VisualElement{name=n,pickingMode=PickingMode.Ignore};Place(e,x,y,w,h);e.style.backgroundColor=c;p.Add(e);return e;}
        private static Label Text(VisualElement p,string t,float x,float y,float w,float h,int size,Color c){var l=new Label(t){pickingMode=PickingMode.Ignore};Place(l,x,y,w,h);l.style.fontSize=size;l.style.color=c;l.style.marginLeft=l.style.marginRight=l.style.marginTop=l.style.marginBottom=0;l.style.paddingLeft=l.style.paddingRight=l.style.paddingTop=l.style.paddingBottom=0;p.Add(l);return l;} private static void Border(VisualElement e,Color c,float w){e.style.borderTopColor=e.style.borderRightColor=e.style.borderBottomColor=e.style.borderLeftColor=c;e.style.borderTopWidth=e.style.borderRightWidth=e.style.borderBottomWidth=e.style.borderLeftWidth=w;}
    }
}
