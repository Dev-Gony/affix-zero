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
        private static readonly Color Ink = new Color32(10,12,15,255), Surface = new Color32(25,28,30,255), Edge = new Color32(65,66,70,255);
        private static readonly Color Crimson = new Color32(196,30,58,255), Gold = new Color32(233,195,73,255), Cream = new Color32(229,225,223,255), Muted = new Color32(159,163,170,255), Sky = new Color32(151,203,255,255);

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
            Root=Box(parent,"talent-screen",18,60,1244,500,Ink);Root.pickingMode=PickingMode.Position;Border(Root,Edge,1);
            Text(Root,"특성 스킬트리  /  TALENT RUNES",18,11,538,30,20,Cream);
            points=Text(Root,"",573,9,170,24,14,Gold);points.name="talent-screen-points";
            spent=Text(Root,"",755,9,160,24,12,Cream);
            Text(Root,"승리로 얻은 포인트를 투자하세요.",575,34,330,17,10,Muted);
            resetButton=Button(Root,"talent-screen-reset","특성 초기화",937,12,154,34,Reset,Surface);
            Button(Root,"talent-screen-close","닫기 [K / ESC]",1104,12,124,34,close,Surface);
            var tree=Box(Root,"talent-tree",12,65,742,364,Surface);
            Text(tree,"무기 숙련",18,10,282,26,17,Gold);
            Text(tree,"분노 → 정밀 → 숙련자의 일격",395,15,326,20,11,Muted).style.unityTextAlign=TextAnchor.MiddleRight;
            Box(tree,"tree-accent",18,41,704,1,Crimson);
            firstLink=Box(tree,"fury-precision-link",368,126,2,40,Edge);
            secondLink=Box(tree,"precision-keystone-link",368,222,2,40,Edge);
            Text(tree,"↓",358,132,24,22,16,Gold).style.unityTextAlign=TextAnchor.MiddleCenter;
            Text(tree,"↓",358,228,24,22,16,Gold).style.unityTextAlign=TextAnchor.MiddleCenter;
            furyNode=Node(tree,"talent-node-fury",TalentId.Fury,"분노",furyIcon,237,56,out furyRank);
            precisionNode=Node(tree,"talent-node-precision",TalentId.Precision,"정밀",precisionIcon,237,152,out precisionRank);
            keystoneNode=Node(tree,"talent-node-keystone",TalentId.Keystone,"숙련자의 일격",keystoneIcon,237,248,out keystoneRank);
            Text(tree,"I",197,76,28,26,15,Gold);Text(tree,"II",197,172,28,26,15,Gold);Text(tree,"III",191,268,34,26,15,Gold);
            Text(tree,"공격력 +3 / 단계",521,65,199,22,12,Muted);
            Text(tree,"최대 2단계",521,91,199,18,10,Muted);
            Text(tree,"공격력 +4",521,161,199,22,12,Muted);
            Text(tree,"분노 2단계 필요",521,187,199,18,10,Muted);
            Text(tree,"공격력 +6",521,257,199,22,12,Muted);
            Text(tree,"정밀 1단계 필요",521,283,199,18,10,Muted);
            Text(tree,"노드를 선택하면 오른쪽에서 효과와 선행 조건을 확인할 수 있습니다.",22,334,700,18,11,Muted);
            var detail=Box(Root,"talent-detail",766,65,466,364,Surface);Border(detail,new Color32(110,58,65,255),1);
            var iconFrame=Box(detail,"talent-detail-icon",18,19,70,70,Crimson);
            detailIcon=Icon(iconFrame,null,15,15,40,40);
            Text(detail,"선택한 특성",105,18,339,20,11,Gold);
            detailName=Text(detail,"",105,44,339,29,20,Cream);detailName.name="talent-detail-name";
            detailRank=Text(detail,"",105,76,339,20,11,Muted);
            var effect=Box(detail,"talent-effect-panel",18,115,428,102,Ink);
            Text(effect,"특성 효과",14,10,397,20,12,Gold);
            detailEffect=Text(effect,"",14,39,397,51,14,Cream);detailEffect.name="talent-detail-effect";detailEffect.style.whiteSpace=WhiteSpace.Normal;
            prerequisite=Text(detail,"",20,237,423,50,12,Sky);prerequisite.style.whiteSpace=WhiteSpace.Normal;
            investButton=Button(detail,"talent-invest","특성 포인트 투자",18,306,428,40,Invest,Gold);
            investCaption=investButton.Q<Label>();investCaption.style.color=Ink;
            var summary=Box(Root,"talent-summary",12,441,1220,47,Surface);
            summaryDamage=Text(summary,"",18,12,350,25,15,Cream);
            summaryBonus=Text(summary,"",397,12,388,25,14,Gold);
            summaryPoints=Text(summary,"",853,12,345,25,12,Muted);
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
            node.style.opacity=unlocked?1:0.6f;
        }
        private VisualElement Node(VisualElement parent,string name,TalentId id,string caption,Texture2D texture,float x,float y,out Label rank)
        {
            var node=Button(parent,name,"",x,y,264,72,()=>Select(id),Ink);
            Icon(node,texture,12,12,48,48);
            Text(node,caption,75,13,179,26,15,Cream);
            rank=Text(node,"",75,43,176,19,11,Gold);return node;
        }
        private static void SetEnabled(VisualElement element,bool value){element.SetEnabled(value);element.style.opacity=value?1:0.45f;}
        private static Image Icon(VisualElement parent,Texture2D texture,float x,float y,float width,float height)
        {var image=new Image{image=texture,scaleMode=ScaleMode.ScaleToFit,pickingMode=PickingMode.Ignore};Place(image,x,y,width,height);parent.Add(image);return image;}
        private static VisualElement Button(VisualElement parent,string name,string caption,float x,float y,float width,float height,Action action,Color color)
        {
            var element=Box(parent,name,x,y,width,height,color);Border(element,Edge,1);element.pickingMode=PickingMode.Position;element.focusable=true;
            Text(element,caption,0,0,width,height,12,Cream).style.unityTextAlign=TextAnchor.MiddleCenter;
            element.RegisterCallback<ClickEvent>(_=>action());
            element.RegisterCallback<NavigationSubmitEvent>(evt=>{action();evt.StopPropagation();});return element;
        }
        private static void Place(VisualElement element,float x,float y,float width,float height)
        {element.style.position=Position.Absolute;element.style.left=x;element.style.top=y;element.style.width=width;element.style.height=height;}
        private static VisualElement Box(VisualElement parent,string name,float x,float y,float width,float height,Color color)
        {var element=new VisualElement{name=name,pickingMode=PickingMode.Ignore};Place(element,x,y,width,height);element.style.backgroundColor=color;parent.Add(element);return element;}
        private static Label Text(VisualElement parent,string text,float x,float y,float width,float height,int size,Color color)
        {
            var label=new Label(text){pickingMode=PickingMode.Ignore};Place(label,x,y,width,height);label.style.fontSize=size;label.style.color=color;
            label.style.marginLeft=label.style.marginRight=label.style.marginTop=label.style.marginBottom=0;
            label.style.paddingLeft=label.style.paddingRight=label.style.paddingTop=label.style.paddingBottom=0;parent.Add(label);return label;
        }
        private static void Border(VisualElement element,Color color,float width)
        {element.style.borderTopColor=element.style.borderRightColor=element.style.borderBottomColor=element.style.borderLeftColor=color;element.style.borderTopWidth=element.style.borderRightWidth=element.style.borderBottomWidth=element.style.borderLeftWidth=width;}
    }
}
