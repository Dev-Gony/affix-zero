using System;
using AffixZero.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace AffixZero.Presentation
{
    // Stitch 11: equipment list, central upgrade preview, right-hand item details.
    // Preview strings come from the actual selected-slot build comparison.
    public sealed class ForgePanel
    {
        public sealed class View
        {
            public string Name, Icon, Affix, Notice;
            public int Rank, WeaponDamage, TotalDamage, Gold, Cost;
            public bool CanEnhance;
            public EquipmentSlot Slot;
            public bool HasItem;
            public string PrimaryLabel,PrimaryValue,NextPrimaryValue,SecondaryLabel,SecondaryValue,NextSecondaryValue;
        }

        public readonly VisualElement Root;
        private readonly Func<View> read;
        private readonly Label name, rank, weapon, attack, wallet, cost, outcome, details, notice, selected, primaryLabel, secondaryLabel, rules;
        private readonly Image preview, selectedIcon;
        private readonly VisualElement enhance;
        private readonly VisualElement[] slotButtons=new VisualElement[3];
        private string resource;
        private static readonly Color Ink = new Color32(17,19,22,255), Surface = new Color32(26,28,31,255), Highest = new Color32(51,53,56,255), Edge = new Color32(91,64,64,255);
        private static readonly Color Crimson = new Color32(196,30,58,255), Gold = new Color32(233,195,73,255), Cream = new Color32(226,226,230,255), Muted = new Color32(227,190,189,255), Sky = new Color32(151,203,255,255);

        public ForgePanel(VisualElement parent, Func<View> read, Action upgrade, Action equipment, Action close,Action<EquipmentSlot> selectSlot=null)
        {
            this.read = read;
            Root = Box(parent,"forge-screen",16,64,1248,516,Ink);
            Root.pickingMode = PickingMode.Position;
            var header=Box(Root,"forge-header",8,8,1232,56,Surface);
            Box(header,"forge-header-accent",0,0,4,56,Crimson);
            Text(header,"대장간 강화",18,5,355,29,23,Cream);
            Text(header,"장착 장비 확정 강화",19,35,355,15,10,Muted);
            var activeTab=Box(header,"forge-active-tab",427,9,226,38,Crimson);
            Text(activeTab,"장비 강화",10,8,206,22,13,Cream).style.unityTextAlign=TextAnchor.MiddleCenter;
            Text(header,"실패 · 장비 파괴 없음",750,18,306,24,12,Gold).style.unityTextAlign=TextAnchor.MiddleRight;
            Button(header,"forge-close","닫기  [ESC]",1084,8,136,40,close,Highest);

            var list = Box(Root,"forge-equipment-list",8,76,244,432,Surface);
            Text(list,"강화 대상 장비",16,13,212,28,18,Cream);
            Box(list,"forge-list-divider",16,49,212,1,Edge);
            string[] slotIds={"forge-slot-weapon","forge-slot-armor","forge-slot-relic"};
            string[] slotNames={"무기","방어구","유물"};
            for(int i=0;i<3;i++)
            {
                EquipmentSlot slot=(EquipmentSlot)i;
                slotButtons[i]=Button(list,slotIds[i],slotNames[i],12,62+i*37,220,31,
                    ()=>{selectSlot?.Invoke(slot);Refresh(true);},Highest);
                slotButtons[i].SetEnabled(selectSlot!=null);
            }
            var item = Box(list,"forge-equipped-item",12,184,220,100,Highest);
            Box(item,"forge-selected-accent",0,0,3,100,Crimson);
            var iconWell=Box(item,"forge-selected-icon-well",12,20,52,52,Ink);
            selectedIcon = Icon(iconWell,6,6,40,40);
            selected = Text(item,"",75,15,134,78,14,Cream); selected.style.whiteSpace = WhiteSpace.Normal;
            Text(list,"부위를 선택해 장착 장비를 강화합니다.\n다른 장비는 가방에서 먼저 장착하세요.",16,298,212,49,12,Muted).style.whiteSpace = WhiteSpace.Normal;
            Button(list,"forge-equipment","장비 선택으로 이동",16,352,212,34,equipment,Highest);
            var hint=Box(list,"forge-preservation-note",12,397,220,25,Ink);
            Text(hint,"강화 단계는 교체 후에도 유지",10,3,200,20,11,Gold);

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
            primaryLabel=Text(weaponBox,"",12,7,258,18,11,Muted);primaryLabel.name="forge-primary-label";
            weapon=Text(weaponBox,"",12,29,258,29,23,Gold); weapon.name="forge-weapon-damage";
            var attackBox=Box(altar,"forge-attack-preview",314,264,286,63,Ink);
            secondaryLabel=Text(attackBox,"",12,7,262,18,11,Muted);secondaryLabel.name="forge-secondary-label";
            attack=Text(attackBox,"",12,29,262,29,23,Sky); attack.name="forge-attack-damage";
            cost=Text(altar,"",20,337,580,24,14,Gold); cost.name="forge-cost";
            enhance=Button(altar,"forge-enhance","강화 실행",20,372,580,44,upgrade,Crimson);
            enhance.Q<Label>().style.fontSize=17;enhance.Q<Label>().style.unityFontStyleAndWeight=FontStyle.Bold;

            var info=Box(Root,"forge-details",896,76,344,432,Surface);
            Text(info,"장비 속성",16,13,312,28,18,Cream);
            var propertyCard=Box(info,"forge-property-card",16,57,312,105,Ink);
            details=Text(propertyCard,"",14,12,284,83,14,Cream); details.style.whiteSpace=WhiteSpace.Normal;
            Box(info,"forge-divider",16,179,312,1,Edge);
            Text(info,"강화 규칙",16,195,312,25,16,Gold);
            rules=Text(info,"",16,234,312,91,13,Muted);rules.style.whiteSpace=WhiteSpace.Normal;
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
            bool max=v.HasItem&&v.Rank>=WeaponItem.MaxEnhancementRank;
            name.text=v.HasItem?v.Name+"  +"+v.Rank:"이 부위에 장착한 장비가 없습니다";
            selected.text=v.HasItem?v.Name+"\n+"+v.Rank+"  ·  장착 중":"미장착\n가방에서 장비 선택";
            rank.text=!v.HasItem?"—":max?"+"+WeaponItem.MaxEnhancementRank+"  최대":"+"+v.Rank+" → +"+(v.Rank+1);
            primaryLabel.text=v.PrimaryLabel??"장비 능력치";
            secondaryLabel.text=v.SecondaryLabel??"영웅 능력치";
            weapon.text=!v.HasItem?"—":(v.PrimaryValue??"—")+(max?"  최대":" → "+(v.NextPrimaryValue??"—"));
            attack.text=!v.HasItem?"—":(v.SecondaryValue??"—")+(max?"  최대":" → "+(v.NextSecondaryValue??"—"));
            cost.text=!v.HasItem?"장비를 먼저 장착하세요.":max?"최대 강화 단계에 도달했습니다.":"소모 골드  "+v.Cost;
            outcome.text=!v.HasItem?"강화 대상 없음":max?"최대 강화 완료":"확정 강화  /  100%";
            details.text=v.HasItem?v.Affix??"":"장비 화면에서 해당 부위의 장비를 장착하면\n실제 속성과 강화 결과를 확인할 수 있습니다.";
            rules.text=(v.Slot==EquipmentSlot.Armor?"단계마다 방어력 +1 · 최대 체력 +5":"단계마다 피해 +2")+
                "\n최대 강화 +"+WeaponItem.MaxEnhancementRank+"\n단계가 높을수록 골드 비용 증가\n실패와 장비 파괴 없음";
            wallet.text="보유 골드  "+v.Gold;
            notice.text=v.HasItem?v.Notice??"":"미장착 부위는 강화할 수 없습니다.";
            bool available=v.HasItem&&!max&&v.CanEnhance;
            enhance.SetEnabled(available); enhance.style.opacity=available?1:0.45f;
            enhance.Q<Label>().text=!v.HasItem?"장비를 먼저 장착하세요":max?"최대 강화 완료":available?"강화 실행  ·  "+v.Cost+" 골드":"골드 부족  ·  "+v.Cost+" 골드 필요";
            string nextResource=v.HasItem?v.Icon:null;
            if(resource!=nextResource) { resource=nextResource; preview.image=selectedIcon.image=string.IsNullOrEmpty(resource)?null:Resources.Load<Texture2D>(resource); }
            for(int i=0;i<slotButtons.Length;i++)
            {bool active=(int)v.Slot==i;slotButtons[i].style.backgroundColor=active?Crimson:Highest;Border(slotButtons[i],active?Gold:Edge);}
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
