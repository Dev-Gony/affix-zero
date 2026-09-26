using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace AffixZero.Presentation
{
    // Stitch 11: equipment list, central upgrade preview, right-hand item details.
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
        private static readonly Color Ink = new Color32(17,19,22,255), Surface = new Color32(26,28,31,255), Highest = new Color32(51,53,56,255), Edge = new Color32(91,64,64,255);
        private static readonly Color Crimson = new Color32(196,30,58,255), Gold = new Color32(233,195,73,255), Cream = new Color32(226,226,230,255), Muted = new Color32(227,190,189,255), Sky = new Color32(151,203,255,255);

        public ForgePanel(VisualElement parent, Func<View> read, Action upgrade, Action equipment, Action close)
        {
            this.read = read;
            Root = Box(parent,"forge-screen",16,64,1248,516,Ink);
            Root.pickingMode = PickingMode.Position;
            var header=Box(Root,"forge-header",8,8,1232,56,Surface);
            Box(header,"forge-header-accent",0,0,4,56,Crimson);
            Text(header,"대장간 강화",18,5,355,29,23,Cream);
            Text(header,"FORGE  /  장착 무기 확정 강화",19,35,355,15,10,Muted);
            var activeTab=Box(header,"forge-active-tab",427,9,226,38,Crimson);
            Text(activeTab,"장비 강화  + ENHANCE",10,8,206,22,13,Cream).style.unityTextAlign=TextAnchor.MiddleCenter;
            Text(header,"실패 · 장비 파괴 없음",750,18,306,24,12,Gold).style.unityTextAlign=TextAnchor.MiddleRight;
            Button(header,"forge-close","닫기  [ESC]",1084,8,136,40,close,Highest);

            var list = Box(Root,"forge-equipment-list",8,76,244,432,Surface);
            Text(list,"강화 대상 장비",16,13,212,28,18,Cream);
            Box(list,"forge-list-divider",16,49,212,1,Edge);
            var item = Box(list,"forge-equipped-item",12,66,220,111,Highest);
            Box(item,"forge-selected-accent",0,0,3,111,Crimson);
            var iconWell=Box(item,"forge-selected-icon-well",12,20,52,52,Ink);
            selectedIcon = Icon(iconWell,6,6,40,40);
            selected = Text(item,"",75,21,134,76,14,Cream); selected.style.whiteSpace = WhiteSpace.Normal;
            Text(list,"현재 장착한 무기를 강화합니다.\n다른 무기는 장비 화면에서\n장착한 뒤 강화하세요.",16,201,212,76,12,Muted).style.whiteSpace = WhiteSpace.Normal;
            Button(list,"forge-equipment","장비 선택으로 이동",16,294,212,40,equipment,Highest);
            var hint=Box(list,"forge-preservation-note",12,360,220,60,Ink);
            Text(hint,"강화 단계는 무기에 남습니다.\n교체해도 유지됩니다.",12,12,196,40,12,Gold).style.whiteSpace = WhiteSpace.Normal;

            var altar = Box(Root,"forge-preview",264,76,620,432,Surface);
            Box(altar,"forge-altar-accent",16,18,4,18,Crimson);
            Text(altar,"강화의 모루",29,11,300,29,18,Cream);
            outcome = Text(altar,"",399,17,201,22,12,Gold); outcome.style.unityTextAlign=TextAnchor.MiddleRight;
            var frame = Box(altar,"forge-item-frame",234,54,152,130,Ink); Border(frame,Edge);
            Box(frame,"forge-frame-top",10,9,132,2,Gold);
            preview = Icon(frame,40,22,72,72);
            var rankPlate=Box(frame,"forge-rank-plate",19,101,114,22,Crimson);
            rank = Text(rankPlate,"",0,1,114,21,13,Cream); rank.style.unityTextAlign=TextAnchor.MiddleCenter;
            name = Text(altar,"",18,193,584,35,23,Cream); name.name="forge-item-name"; name.style.unityTextAlign=TextAnchor.MiddleCenter;
            Text(altar,"강화 전 → 강화 후",20,235,580,21,11,Muted);
            var weaponBox=Box(altar,"forge-weapon-preview",20,264,282,63,Ink);
            Text(weaponBox,"무기 피해",12,7,258,18,11,Muted);
            weapon=Text(weaponBox,"",12,29,258,29,23,Gold); weapon.name="forge-weapon-damage";
            var attackBox=Box(altar,"forge-attack-preview",314,264,286,63,Ink);
            Text(attackBox,"영웅 공격력",12,7,262,18,11,Muted);
            attack=Text(attackBox,"",12,29,262,29,23,Sky); attack.name="forge-attack-damage";
            cost=Text(altar,"",20,337,580,24,14,Gold); cost.name="forge-cost";
            enhance=Button(altar,"forge-enhance","강화 실행",20,372,580,44,upgrade,Crimson);
            enhance.Q<Label>().style.fontSize=17;enhance.Q<Label>().style.unityFontStyleAndWeight=FontStyle.Bold;

            var info=Box(Root,"forge-details",896,76,344,432,Surface);
            Text(info,"무기 속성",16,13,312,28,18,Cream);
            var propertyCard=Box(info,"forge-property-card",16,57,312,105,Ink);
            details=Text(propertyCard,"",14,12,284,83,14,Cream); details.style.whiteSpace=WhiteSpace.Normal;
            Box(info,"forge-divider",16,179,312,1,Edge);
            Text(info,"강화 규칙",16,195,312,25,16,Gold);
            Text(info,"단계마다 무기 피해 +2\n최대 강화 +3\n소모 골드  8 → 16 → 24\n실패와 장비 파괴 없음",16,234,312,91,13,Muted).style.whiteSpace=WhiteSpace.Normal;
            var walletCard=Box(info,"forge-wallet-card",16,333,312,39,Highest);
            wallet=Text(walletCard,"",12,8,288,24,16,Gold); wallet.name="forge-wallet";
            notice=Text(info,"",16,384,312,36,11,Sky); notice.name="forge-notice"; notice.style.whiteSpace=WhiteSpace.Normal;
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
        {var e=new Label(value){pickingMode=PickingMode.Ignore};Place(e,x,y,w,h);e.style.fontSize=size;e.style.color=color;if(size>=17)e.style.unityFontStyleAndWeight=FontStyle.Bold;e.style.marginLeft=e.style.marginRight=e.style.marginTop=e.style.marginBottom=0;e.style.paddingLeft=e.style.paddingRight=e.style.paddingTop=e.style.paddingBottom=0;parent.Add(e);return e;}
        private static void Border(VisualElement e,Color color){e.style.borderTopColor=e.style.borderRightColor=e.style.borderBottomColor=e.style.borderLeftColor=color;e.style.borderTopWidth=e.style.borderRightWidth=e.style.borderBottomWidth=e.style.borderLeftWidth=1;}
    }
}
