using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace AffixZero.Presentation
{
    // Stitch 06: equipment list, central upgrade preview, right-hand item details.
    // Only the implemented deterministic weapon upgrade is presented as an action.
    public sealed class ForgePanel
    {
        public sealed class View
        {
            public string Name, Icon, Affix, Notice;
            public int Rank, WeaponDamage, TotalDamage, Gold, Cost;
            public bool CanEnhance;
        }

        public readonly VisualElement Root;
        private readonly Func<View> read;
        private readonly Label name, rank, weapon, attack, wallet, cost, outcome, details, notice, selected;
        private readonly Image preview, selectedIcon;
        private readonly VisualElement enhance;
        private string resource;
        private static readonly Color Ink = new Color32(10,12,15,255), Surface = new Color32(25,28,30,255), Edge = new Color32(65,66,70,255);
        private static readonly Color Crimson = new Color32(196,30,58,255), Gold = new Color32(233,195,73,255), Cream = new Color32(229,225,223,255), Muted = new Color32(159,163,170,255), Sky = new Color32(151,203,255,255);

        public ForgePanel(VisualElement parent, Func<View> read, Action upgrade, Action equipment, Action close)
        {
            this.read = read;
            Root = Box(parent,"forge-screen",18,60,1244,500,Ink);
            Root.pickingMode = PickingMode.Position; Border(Root,Edge);
            Text(Root,"대장간  /  FORGE",18,10,360,30,19,Cream);
            Text(Root,"장비 강화",428,17,170,22,13,Gold);
            Button(Root,"forge-close","닫기  [ESC]",1106,9,121,31,close,Surface);

            var list = Box(Root,"forge-equipment-list",12,49,238,439,Surface);
            Text(list,"강화 대상 장비",14,13,208,25,15,Cream);
            var item = Box(list,"forge-equipped-item",12,53,214,93,Ink); Border(item,Gold);
            selectedIcon = Icon(item,12,16,42,42);
            selected = Text(item,"",65,15,140,61,12,Cream); selected.style.whiteSpace = WhiteSpace.Normal;
            Text(list,"현재 장착한 무기를 강화합니다.\n다른 무기는 장비 화면에서\n장착한 뒤 강화하세요.",14,166,208,80,12,Muted).style.whiteSpace = WhiteSpace.Normal;
            Button(list,"forge-equipment","장비 선택으로 이동",14,260,208,35,equipment,Ink);
            Text(list,"강화 단계는 무기에 남습니다.\n교체해도 강화가 유지됩니다.",14,364,208,58,11,Gold).style.whiteSpace = WhiteSpace.Normal;

            var altar = Box(Root,"forge-preview",262,49,620,439,Surface);
            Text(altar,"ENHANCEMENT ALTAR",16,13,350,24,14,Cream);
            outcome = Text(altar,"",404,16,200,20,11,Gold); outcome.style.unityTextAlign=TextAnchor.MiddleRight;
            var frame = Box(altar,"forge-item-frame",249,51,122,108,Ink); Border(frame,Gold);
            preview = Icon(frame,29,15,64,64);
            rank = Text(frame,"",4,82,114,21,12,Gold); rank.style.unityTextAlign=TextAnchor.MiddleCenter;
            name = Text(altar,"",18,170,584,30,20,Cream); name.name="forge-item-name"; name.style.unityTextAlign=TextAnchor.MiddleCenter;
            Text(altar,"강화 후 능력치  /  PREVIEW",20,215,580,21,12,Muted);
            var weaponBox=Box(altar,"forge-weapon-preview",20,244,281,65,Ink);
            Text(weaponBox,"무기 피해",12,7,257,18,11,Muted);
            weapon=Text(weaponBox,"",12,30,257,27,20,Gold); weapon.name="forge-weapon-damage";
            var attackBox=Box(altar,"forge-attack-preview",313,244,287,65,Ink);
            Text(attackBox,"영웅 공격력",12,7,263,18,11,Muted);
            attack=Text(attackBox,"",12,30,263,27,20,Sky); attack.name="forge-attack-damage";
            cost=Text(altar,"",20,321,580,24,13,Gold); cost.name="forge-cost";
            enhance=Button(altar,"forge-enhance","강화 실행",20,359,580,54,upgrade,Crimson);

            var info=Box(Root,"forge-details",894,49,338,439,Surface);
            Text(info,"무기 속성",16,13,306,25,15,Cream);
            details=Text(info,"",16,54,306,98,13,Cream); details.style.whiteSpace=WhiteSpace.Normal;
            Box(info,"forge-divider",16,164,306,1,Edge);
            Text(info,"강화 규칙",16,180,306,25,14,Gold);
            Text(info,"단계마다 무기 피해 +2\n최대 강화 +3\n소모 골드  8 → 16 → 24\n실패와 장비 파괴 없음",16,219,306,96,12,Muted).style.whiteSpace=WhiteSpace.Normal;
            wallet=Text(info,"",16,329,306,24,14,Gold); wallet.name="forge-wallet";
            notice=Text(info,"",16,371,306,53,11,Sky); notice.name="forge-notice"; notice.style.whiteSpace=WhiteSpace.Normal;
            Root.style.display=DisplayStyle.None;
        }

        public void Refresh(bool visible)
        {
            Root.style.display=visible?DisplayStyle.Flex:DisplayStyle.None;
            if(!visible)return;
            View v=read(); if(v==null)return;
            bool max=v.Rank>=3;
            name.text=v.Name+"  +"+v.Rank;
            selected.text=v.Name+"\n+"+v.Rank+"  ·  장착 중";
            rank.text=max?"+3  MAX":"+"+v.Rank+" → +"+(v.Rank+1);
            weapon.text=v.WeaponDamage+(max?"  MAX":" → "+(v.WeaponDamage+2));
            attack.text=v.TotalDamage+(max?"  MAX":" → "+(v.TotalDamage+2));
            cost.text=max?"최대 강화 단계에 도달했습니다.":"소모 골드  "+v.Cost+" G";
            outcome.text=max?"최대 강화 완료":"확정 강화  /  100%";
            details.text=v.Affix+"\n강화 피해 +"+(v.Rank*2)+"\n전체 무기 피해 +"+v.WeaponDamage;
            wallet.text="보유 골드  "+v.Gold+" G";
            notice.text=v.Notice??"";
            enhance.SetEnabled(v.CanEnhance); enhance.style.opacity=v.CanEnhance?1:0.45f;
            enhance.Q<Label>().text=max?"최대 강화 완료":v.CanEnhance?"강화 실행  ·  "+v.Cost+" G":"골드 부족  ·  "+v.Cost+" G 필요";
            if(resource!=v.Icon) { resource=v.Icon; preview.image=selectedIcon.image=Resources.Load<Texture2D>(resource); }
        }

        private static Image Icon(VisualElement parent,float x,float y,float w,float h)
        {var image=new Image {scaleMode=ScaleMode.ScaleToFit,pickingMode=PickingMode.Ignore};Place(image,x,y,w,h);parent.Add(image);return image;}
        private static VisualElement Button(VisualElement parent,string id,string caption,float x,float y,float w,float h,Action action,Color color)
        {
            var button=Box(parent,id,x,y,w,h,color);button.pickingMode=PickingMode.Position;button.focusable=true;Border(button,Edge);
            Text(button,caption,0,0,w,h,13,Cream).style.unityTextAlign=TextAnchor.MiddleCenter;
            button.RegisterCallback<ClickEvent>(_=>action());button.RegisterCallback<NavigationSubmitEvent>(evt=>{action();evt.StopPropagation();});return button;
        }
        private static void Place(VisualElement e,float x,float y,float w,float h){e.style.position=Position.Absolute;e.style.left=x;e.style.top=y;e.style.width=w;e.style.height=h;}
        private static VisualElement Box(VisualElement parent,string id,float x,float y,float w,float h,Color color)
        {var e=new VisualElement{name=id,pickingMode=PickingMode.Ignore};Place(e,x,y,w,h);e.style.backgroundColor=color;parent.Add(e);return e;}
        private static Label Text(VisualElement parent,string value,float x,float y,float w,float h,int size,Color color)
        {var e=new Label(value){pickingMode=PickingMode.Ignore};Place(e,x,y,w,h);e.style.fontSize=size;e.style.color=color;e.style.marginLeft=e.style.marginRight=e.style.marginTop=e.style.marginBottom=0;e.style.paddingLeft=e.style.paddingRight=e.style.paddingTop=e.style.paddingBottom=0;parent.Add(e);return e;}
        private static void Border(VisualElement e,Color color){e.style.borderTopColor=e.style.borderRightColor=e.style.borderBottomColor=e.style.borderLeftColor=color;e.style.borderTopWidth=e.style.borderRightWidth=e.style.borderBottomWidth=e.style.borderLeftWidth=1;}
    }
}
