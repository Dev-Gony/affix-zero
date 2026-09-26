using System;
using AffixZero.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace AffixZero.Presentation
{
    // Dedicated native talent view. Selecting a node never spends a point.
    public sealed class TalentPanel
    {
        public sealed class View
        {
            public int Points, Spent, Fury, Precision, Keystone, TotalDamage;
        }
        public readonly VisualElement Root;
        public TalentId SelectedTalent { get; private set; } = TalentId.Fury;
        private readonly Func<View> read;
        private readonly Action<TalentId> invest;
        private readonly Action reset;
        private readonly VisualElement furyNode, precisionNode, keystoneNode, investButton, resetButton, firstLink, secondLink;
        private readonly Label points, spent, furyRank, precisionRank, keystoneRank, detailName, detailRank, detailEffect, prerequisite, investCaption, summaryDamage, summaryBonus, summaryPoints;
        private readonly Image detailIcon;
        private readonly Texture2D furyIcon, precisionIcon, keystoneIcon;
        private static readonly Color Ink = new Color32(17,19,22,255), Surface = new Color32(26,28,31,255), Highest = new Color32(51,53,56,255), Edge = new Color32(91,64,64,255);
        private static readonly Color Crimson = new Color32(196,30,58,255), Gold = new Color32(233,195,73,255), Cream = new Color32(226,226,230,255), Muted = new Color32(227,190,189,255), Sky = new Color32(151,203,255,255);

        public TalentPanel(VisualElement parent,Func<View> read,Action<TalentId> invest,Action reset,Action close)
        {
            this.read=read??throw new ArgumentNullException(nameof(read));
            this.invest=invest??throw new ArgumentNullException(nameof(invest));
            this.reset=reset??throw new ArgumentNullException(nameof(reset));
            if(parent==null)throw new ArgumentNullException(nameof(parent));
            if(close==null)throw new ArgumentNullException(nameof(close));
            furyIcon=Resources.Load<Texture2D>("AffixGenerated/PowerRune");
            precisionIcon=Resources.Load<Texture2D>("AffixGenerated/PrecisionRune");
            keystoneIcon=Resources.Load<Texture2D>("AffixGenerated/VeteranRune");
            Root=Box(parent,"talent-screen",16,64,1248,516,Ink);Root.pickingMode=PickingMode.Position;
            var header=Box(Root,"talent-header",8,8,1232,56,Surface);
            Box(header,"talent-header-accent",0,0,4,56,Crimson);
            Text(header,"특성 스킬트리",18,5,355,29,23,Cream);
            Text(header,"TALENT RUNES  /  무기 숙련",19,35,355,15,10,Muted);
            var available=Box(header,"talent-available-card",493,8,180,40,Ink);
            points=Text(available,"",12,8,156,26,16,Gold);points.name="talent-screen-points";
            var invested=Box(header,"talent-invested-card",681,8,183,40,Ink);
            spent=Text(invested,"",12,10,159,22,13,Cream);
            resetButton=Button(header,"talent-screen-reset","특성 초기화",878,8,194,40,Reset,Highest);
            Button(header,"talent-screen-close","닫기 [K / ESC]",1084,8,136,40,close,Highest);

            var tree=Box(Root,"talent-tree",8,76,780,376,Surface);
            var branch=Box(tree,"talent-branch-header",12,12,756,42,new Color32(60,26,35,255));
            Box(branch,"talent-branch-accent",0,0,3,42,Crimson);
            Text(branch,"무기 숙련",14,7,295,29,18,Cream);
            Text(branch,"선택 → 효과 확인 → 포인트 투자",379,11,361,24,12,Muted).style.unityTextAlign=TextAnchor.MiddleRight;
            firstLink=Box(tree,"fury-precision-link",248,158,50,2,Edge);
            secondLink=Box(tree,"precision-keystone-link",486,158,50,2,Edge);
            Text(tree,"→",260,140,28,32,22,Gold).style.unityTextAlign=TextAnchor.MiddleCenter;
            Text(tree,"→",498,140,28,32,22,Gold).style.unityTextAlign=TextAnchor.MiddleCenter;
            furyNode=Node(tree,"talent-node-fury",TalentId.Fury,"분노",furyIcon,60,90,out furyRank);
            precisionNode=Node(tree,"talent-node-precision",TalentId.Precision,"정밀",precisionIcon,298,90,out precisionRank);
            keystoneNode=Node(tree,"talent-node-keystone",TalentId.Keystone,"숙련자의 일격",keystoneIcon,536,90,out keystoneRank);
            Text(tree,"공격력 +3 / 단계",42,248,224,25,15,Gold).style.unityTextAlign=TextAnchor.MiddleCenter;
            Text(tree,"최대 2단계",42,278,224,22,12,Muted).style.unityTextAlign=TextAnchor.MiddleCenter;
            Text(tree,"공격력 +4",280,248,224,25,15,Sky).style.unityTextAlign=TextAnchor.MiddleCenter;
            Text(tree,"분노 2단계 필요",280,278,224,22,12,Muted).style.unityTextAlign=TextAnchor.MiddleCenter;
            Text(tree,"공격력 +6",518,248,224,25,15,Gold).style.unityTextAlign=TextAnchor.MiddleCenter;
            Text(tree,"정밀 1단계 필요",518,278,224,22,12,Muted).style.unityTextAlign=TextAnchor.MiddleCenter;
            Box(tree,"talent-tree-divider",20,322,740,1,Edge);
            Text(tree,"잠긴 특성도 선택해서 선행 조건을 확인할 수 있습니다.",22,339,736,21,12,Muted);

            var detail=Box(Root,"talent-detail",800,76,440,432,Surface);
            Box(detail,"talent-detail-accent",0,0,3,432,new Color32(255,179,180,255));
            var iconFrame=Box(detail,"talent-detail-icon",20,20,76,76,Crimson);
            detailIcon=Icon(iconFrame,null,14,14,48,48);
            Text(detail,"선택한 특성  /  PASSIVE",112,17,307,19,11,Gold);
            detailName=Text(detail,"",112,41,307,34,23,Cream);detailName.name="talent-detail-name";
            detailRank=Text(detail,"",112,78,307,21,13,Muted);
            var effect=Box(detail,"talent-effect-panel",20,120,400,112,Ink);
            Text(effect,"특성 효과",16,12,368,22,12,Gold);
            detailEffect=Text(effect,"",16,43,368,57,16,Cream);detailEffect.name="talent-detail-effect";detailEffect.style.whiteSpace=WhiteSpace.Normal;
            Text(detail,"투자 조건",22,252,394,22,12,Gold);
            prerequisite=Text(detail,"",22,283,394,57,14,Sky);prerequisite.style.whiteSpace=WhiteSpace.Normal;
            investButton=Button(detail,"talent-invest","특성 포인트 투자",20,360,400,52,Invest,Gold);
            investCaption=investButton.Q<Label>();investCaption.style.color=Ink;investCaption.style.fontSize=16;investCaption.style.unityFontStyleAndWeight=FontStyle.Bold;
            var summary=Box(Root,"talent-summary",8,464,780,44,Surface);
            summaryDamage=Text(summary,"",16,9,208,26,17,Cream);
            summaryBonus=Text(summary,"",238,11,283,24,13,Gold);
            summaryPoints=Text(summary,"",535,12,229,22,11,Muted);
            Root.style.display=DisplayStyle.None;
        }
        public void Refresh(bool visible)
        {
            Root.style.display=visible?DisplayStyle.Flex:DisplayStyle.None;
            if(!visible)return;
            View view=read();if(view==null){Root.style.display=DisplayStyle.None;return;}
            points.text="잔여 포인트  "+view.Points;
            spent.text="투자 누적  "+view.Spent+" / 4";
            furyRank.text=view.Fury+" / 2";precisionRank.text=view.Precision+" / 1";keystoneRank.text=view.Keystone+" / 1";
            ShowNode(furyNode,TalentId.Fury,view.Fury>0,true);
            ShowNode(precisionNode,TalentId.Precision,view.Precision>0,view.Fury>=2);
            ShowNode(keystoneNode,TalentId.Keystone,view.Keystone>0,view.Precision>=1);
            firstLink.style.backgroundColor=view.Fury>=2?Gold:Edge;
            secondLink.style.backgroundColor=view.Precision>=1?Gold:Edge;
            int rank=Rank(view,SelectedTalent),maximum=SelectedTalent==TalentId.Fury?2:1;
            int bonus=SelectedTalent==TalentId.Fury?3:SelectedTalent==TalentId.Precision?4:6;
            bool unlocked=Unlocked(view,SelectedTalent);
            detailName.text=SelectedTalent==TalentId.Fury?"분노":SelectedTalent==TalentId.Precision?"정밀":"숙련자의 일격";
            detailIcon.image=SelectedTalent==TalentId.Fury?furyIcon:SelectedTalent==TalentId.Precision?precisionIcon:keystoneIcon;
            detailRank.text="현재 단계  "+rank+" / "+maximum;
            detailEffect.text="단계당 공격력 +"+bonus+"\n현재 이 특성의 공격력 증가  +"+(rank*bonus);
            string condition=SelectedTalent==TalentId.Fury?"선행 조건 없음":SelectedTalent==TalentId.Precision?"선행 조건: 분노 2단계":"선행 조건: 정밀 1단계";
            prerequisite.text=condition+"\n"+(rank>=maximum?"최대 단계에 도달했습니다.":!unlocked?"선행 특성에 먼저 투자하세요.":view.Points<1?"다음 승리에서 특성 포인트를 얻으세요.":"포인트 1로 공격력이 증가합니다.");
            SetEnabled(investButton,CanInvest(view,SelectedTalent));
            investCaption.text=rank>=maximum?"최대 단계":!unlocked?"선행 조건 필요":view.Points<1?"포인트 부족":"특성 포인트 투자  ·  1 PT";
            SetEnabled(resetButton,view.Spent>0);
            summaryDamage.text="총 공격력  "+view.TotalDamage;
            summaryBonus.text="특성으로 얻은 공격력  +"+(view.Fury*3+view.Precision*4+view.Keystone*6);
            summaryPoints.text="획득 포인트 "+(view.Points+view.Spent)+"  ·  남은 포인트 "+view.Points;
        }
        private void Select(TalentId talent){SelectedTalent=talent;Refresh(true);}
        private void Invest(){View view=read();if(view!=null&&CanInvest(view,SelectedTalent))invest(SelectedTalent);Refresh(true);}
        private void Reset(){View view=read();if(view!=null&&view.Spent>0)reset();Refresh(true);}
        private static int Rank(View view,TalentId talent)=>talent==TalentId.Fury?view.Fury:talent==TalentId.Precision?view.Precision:view.Keystone;
        private static bool Unlocked(View view,TalentId talent)=>talent==TalentId.Fury||(talent==TalentId.Precision?view.Fury>=2:view.Precision>=1);
        private static bool CanInvest(View view,TalentId talent)=>view.Points>0&&Unlocked(view,talent)&&Rank(view,talent)<(talent==TalentId.Fury?2:1);
        private void ShowNode(VisualElement node,TalentId id,bool acquired,bool unlocked)
        {
            Border(node,SelectedTalent==id?Gold:acquired?Crimson:Edge,SelectedTalent==id?2:1);
            node.style.backgroundColor=acquired?(Color)new Color32(60,26,35,255):Ink;
            // Locked nodes remain selectable so their prerequisites can be read.
            node.style.opacity=unlocked?1:0.78f;
        }
        private VisualElement Node(VisualElement parent,string name,TalentId id,string caption,Texture2D texture,float x,float y,out Label rank)
        {
            var node=Button(parent,name,"",x,y,188,142,()=>Select(id),Ink);
            var iconWell=Box(node,name+"-icon-well",61,16,66,66,Highest);
            Icon(iconWell,texture,9,9,48,48);
            Text(node,caption,6,91,176,29,17,Cream).style.unityTextAlign=TextAnchor.MiddleCenter;
            rank=Text(node,"",135,7,44,19,11,Gold);rank.style.unityTextAlign=TextAnchor.MiddleCenter;
            return node;
        }
        private static void SetEnabled(VisualElement element,bool value){element.SetEnabled(value);element.style.opacity=value?1:0.45f;}
        private static Image Icon(VisualElement parent,Texture2D texture,float x,float y,float width,float height)
        {var image=new Image{image=texture,scaleMode=ScaleMode.ScaleToFit,pickingMode=PickingMode.Ignore};Place(image,x,y,width,height);parent.Add(image);return image;}
        private static VisualElement Button(VisualElement parent,string name,string caption,float x,float y,float width,float height,Action action,Color color)
        {
            var element=Box(parent,name,x,y,width,height,color);Border(element,Edge,1);element.pickingMode=PickingMode.Position;element.focusable=true;
            Text(element,caption,0,0,width,height,13,Cream).style.unityTextAlign=TextAnchor.MiddleCenter;
            element.RegisterCallback<ClickEvent>(_=>action());
            element.RegisterCallback<NavigationSubmitEvent>(evt=>{action();evt.StopPropagation();});return element;
        }
        private static void Place(VisualElement element,float x,float y,float width,float height)
        {element.style.position=Position.Absolute;element.style.left=x;element.style.top=y;element.style.width=width;element.style.height=height;}
        private static VisualElement Box(VisualElement parent,string name,float x,float y,float width,float height,Color color)
        {var element=new VisualElement{name=name,pickingMode=PickingMode.Ignore};Place(element,x,y,width,height);element.style.backgroundColor=color;parent.Add(element);return element;}
        private static Label Text(VisualElement parent,string text,float x,float y,float width,float height,int size,Color color)
        {
            var label=new Label(text){pickingMode=PickingMode.Ignore};Place(label,x,y,width,height);label.style.fontSize=size;label.style.color=color;if(size>=17)label.style.unityFontStyleAndWeight=FontStyle.Bold;
            label.style.marginLeft=label.style.marginRight=label.style.marginTop=label.style.marginBottom=0;
            label.style.paddingLeft=label.style.paddingRight=label.style.paddingTop=label.style.paddingBottom=0;parent.Add(label);return label;
        }
        private static void Border(VisualElement element,Color color,float width)
        {element.style.borderTopColor=element.style.borderRightColor=element.style.borderBottomColor=element.style.borderLeftColor=color;element.style.borderTopWidth=element.style.borderRightWidth=element.style.borderBottomWidth=element.style.borderLeftWidth=width;}
    }
}
