using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace AffixZero.Presentation
{
    // View-only snapshot keeps inventory rules and stat calculation in the encounter/core layer.
    public sealed class EquipmentPanel
    {
        public sealed class ItemView
        {
            public string Id, Name, Affix, Icon;
            public int Damage, Defense;
            public bool Equipped;
        }
        public sealed class View
        {
            public ItemView[] Items = Array.Empty<ItemView>();
            public ItemView Equipped, Selected;
            public string Health, Damage, Defense, Duration, Comparison, Delta, Affix, Status;
            public int Points, Fury, Precision, Keystone;
            public bool CanEquip, CanSalvage, CanFury, CanPrecision, CanKeystone, CanReset;
        }
        public readonly VisualElement Root;
        private readonly VisualElement[] slots = new VisualElement[24];
        private readonly Image[] slotImages = new Image[24];
        private readonly Label[] slotNames = new Label[24];
        private readonly Label[] slotBadges = new Label[24];
        private readonly Label equippedName, equippedDetails, health, damage, defense, duration, bagCount, selectedName, comparison, delta, affix, points, status, selectedDamage;
        private readonly Label furyCaption, precisionCaption, keystoneCaption;
        private readonly VisualElement equipButton, salvageButton, furyButton, precisionButton, keystoneButton, resetButton;
        private readonly Image equippedIcon, selectedIcon;
        private readonly Func<View> read;
        private readonly Action<int> select;
        private readonly Action discard;
        private bool discardConfirm;
        private string selectedId, discardItemId;
        private static readonly Color Surface = new Color32(26,28,31,255), Ink = new Color32(12,14,17,255), Edge = new Color32(40,42,45,255);
        private static readonly Color Crimson = new Color32(196,30,58,255), Gold = new Color32(233,195,73,255), Cream = new Color32(226,226,230,255), Muted = new Color32(175,177,184,255), Sky = new Color32(151,203,255,255), Rose = new Color32(255,179,180,255);
        private readonly System.Collections.Generic.Dictionary<string,Texture2D> textures = new System.Collections.Generic.Dictionary<string,Texture2D>();

        public EquipmentPanel(VisualElement parent,Func<View> read,Action<int> select,Action equip,Action salvage,Action fury,Action precision,Action keystone,Action reset,Action close,Sprite heroPortrait)
        {
            this.read=read;this.select=select;this.discard=salvage;
            Root=Box(parent,"character-panel",16,64,1248,516,Ink);Border(Root,Edge,1);
            Root.pickingMode=PickingMode.Position;
            var ribbon=Box(Root,"management-header",0,0,1246,42,Surface);
            Box(ribbon,"management-accent",14,14,9,14,Crimson);
            Text(ribbon,"영웅 관리",34,7,172,28,20,Cream);
            Text(ribbon,"EQUIPMENT  /  TALENT RUNES",207,12,400,23,12,Rose);
            Button(ribbon,"close-character","던전으로  [TAB]",1098,6,136,30,close,Edge);
            var equipment=Box(Root,"equipment-section",12,52,706,220,Surface);
            Header(equipment,"equipment-heading","장착 무기  /  EQUIPMENT",14,8,678);
            var weaponCard=Box(equipment,"equipped-item-card",14,44,226,162,Ink);
            Box(weaponCard,"equipped-accent",0,0,3,162,Gold);
            Text(weaponCard,"EQUIPPED",12,5,200,18,11,Gold);
            var weaponSlot=Box(weaponCard,"equipped-weapon",12,32,62,64,Edge);Border(weaponSlot,Gold,1);
            equippedIcon=Icon(weaponSlot,7,7,48,48);
            equippedName=Text(weaponCard,"",86,33,128,59,15,Gold);equippedName.style.whiteSpace=WhiteSpace.Normal;
            equippedDetails=Text(weaponCard,"",12,107,202,46,12,Rose);equippedDetails.style.whiteSpace=WhiteSpace.Normal;
            var portraitBox=Box(equipment,"hero-portrait",250,44,214,162,Ink);
            Box(portraitBox,"hero-plinth",36,140,142,2,Edge);
            if(heroPortrait!=null)
            {
                var image=new Image {image=heroPortrait.texture,scaleMode=ScaleMode.ScaleToFit,pickingMode=PickingMode.Ignore};Place(image,27,0,160,151);portraitBox.Add(image);
                // Image.sourceRect requires a Texture, with a top-left origin; Sprite.rect uses bottom-left.
                Rect source=heroPortrait.textureRect;
                image.sourceRect=new Rect(source.x,heroPortrait.texture.height-source.yMax,source.width,source.height);
                if(heroPortrait.rect.width==100 && heroPortrait.rect.height==100)
                {
                    Rect cell=heroPortrait.rect;
                    image.sourceRect=new Rect(cell.x+30,heroPortrait.texture.height-cell.yMax+30,40,40);
                }
            }
            Text(portraitBox,"HERO",65,143,84,18,10,Muted).style.unityTextAlign=TextAnchor.MiddleCenter;
            var stats=Box(equipment,"hero-stat-table",476,44,216,162,Ink);
            Text(stats,"능력치 세부 정보",10,6,196,23,14,Gold);
            health=Stat(stats,10,37,"생명력");damage=Stat(stats,10,67,"공격력");defense=Stat(stats,10,97,"방어력");duration=Stat(stats,10,127,"공격 주기");
            damage.name="equipment-damage-value";damage.style.color=Rose;damage.style.unityFontStyleAndWeight=FontStyle.Bold;
            var bag=Box(Root,"inventory-grid",12,284,706,220,Surface);
            Box(bag,"bag-heading",14,8,678,28,Ink);
            bagCount=Text(bag,"소지품",24,10,340,25,15,Cream);
            Text(bag,"선택 → 비교 → 장착",436,14,242,19,11,Muted).style.unityTextAlign=TextAnchor.MiddleRight;
            for(int i=0;i<24;i++)
            {
                int index=i;float x=14+(i%8)*85,y=43+(i/8)*55;
                var slot=Box(bag,"inventory-slot-"+i,x,y,80,50,Ink);Border(slot,Edge,1);slot.pickingMode=PickingMode.Position;slot.focusable=true;
                slotImages[i]=Icon(slot,24,2,30,30);slotNames[i]=Text(slot,"",4,32,72,16,10,Cream);slotNames[i].style.unityTextAlign=TextAnchor.MiddleCenter;
                slotNames[i].style.overflow=Overflow.Hidden;slotNames[i].style.textOverflow=TextOverflow.Ellipsis;
                slotBadges[i]=Text(slot,"",4,2,72,13,9,Gold);slots[i]=slot;
                slot.RegisterCallback<ClickEvent>(_=>{discardConfirm=false;this.select(index);});slot.RegisterCallback<NavigationSubmitEvent>(evt=>{discardConfirm=false;this.select(index);evt.StopPropagation();});
            }
            var talents=Box(Root,"talent-section",730,52,506,180,Surface);
            Header(talents,"talent-heading","특성 룬  /  MASTERY",12,8,482);
            points=Text(talents,"",287,12,100,22,12,Gold);points.name="talent-points";
            resetButton=Button(talents,"talent-reset","초기화",401,10,81,23,reset,Edge);
            Box(talents,"fury-to-precision",147,85,32,2,Crimson);
            Box(talents,"precision-to-keystone-down",323,85,30,2,Crimson);
            furyButton=Rune(talents,"talent-fury","PowerRune",19,51,128,69,fury,out furyCaption);
            precisionButton=Rune(talents,"talent-precision","PrecisionRune",184,51,128,69,precision,out precisionCaption);
            keystoneButton=Rune(talents,"talent-keystone","VeteranRune",350,51,137,69,keystone,out keystoneCaption);
            furyButton.tooltip="포인트 1 · 공격력 +3 · 최대 2단계";
            precisionButton.tooltip="분노 2 필요 · 포인트 1 · 공격력 +4";
            keystoneButton.tooltip="정밀 1 필요 · 포인트 1 · 공격력 +6";
            Text(talents,"피해 +3 / 단계",18,123,132,19,11,Muted).style.unityTextAlign=TextAnchor.MiddleCenter;
            Text(talents,"분노 2 필요 · +4",175,123,146,19,11,Muted).style.unityTextAlign=TextAnchor.MiddleCenter;
            Text(talents,"정밀 1 필요 · +6",343,123,151,19,11,Gold).style.unityTextAlign=TextAnchor.MiddleCenter;
            Text(talents,"처치 1회 = 포인트 1  ·  초기화 시 전액 반환",20,151,468,20,11,Muted);
            var inspect=Box(Root,"item-comparison",730,244,506,260,Surface);
            Header(inspect,"comparison-heading","아이템 비교  /  EQUIPPED → LOOT",12,8,482);
            var selectedSlot=Box(inspect,"selected-weapon-icon",14,47,50,50,Ink);Border(selectedSlot,Crimson,1);
            selectedIcon=Icon(selectedSlot,5,5,40,40);
            selectedName=Text(inspect,"",76,47,410,27,19,Rose);selectedName.name="selected-item-name";
            delta=Text(inspect,"",76,77,410,22,13,Sky);delta.name="comparison-delta";
            var damageStrip=Box(inspect,"selected-damage-strip",14,106,478,51,Ink);
            selectedDamage=Text(damageStrip,"",10,0,106,33,28,Cream);
            Text(damageStrip,"무기 피해",12,33,100,16,10,Muted);
            comparison=Text(damageStrip,"",139,14,323,26,14,Rose);
            affix=Text(inspect,"",15,165,476,34,12,Gold);affix.style.whiteSpace=WhiteSpace.Normal;
            status=Text(inspect,"",15,199,476,18,11,Muted);status.style.overflow=Overflow.Hidden;status.style.textOverflow=TextOverflow.Ellipsis;
            equipButton=Button(inspect,"equip-button","장비 교체  /  EQUIP",14,223,288,26,equip,Rose);
            equipButton.Q<Label>().style.color=Ink;
            salvageButton=Button(inspect,"salvage-button","버리기",314,223,178,26,RequestDiscard,Edge);
            Root.style.display=DisplayStyle.None;
        }
        public void Refresh(bool visible)
        {
            Root.style.display=visible?DisplayStyle.Flex:DisplayStyle.None;if(!visible){discardConfirm=false;return;}
            View v=read();if(v==null)return;
            string nextId=v.Selected==null?null:v.Selected.Id;
            if(selectedId!=nextId){discardConfirm=false;selectedId=nextId;}
            salvageButton.Q<Label>().text=discardConfirm?"버리기 확인":"버리기";
            equippedName.text=v.Equipped==null?"장착 무기 없음":v.Equipped.Name;
            equippedDetails.text=v.Equipped==null?"":v.Equipped.Affix;
            equippedIcon.image=Texture(v.Equipped==null?null:v.Equipped.Icon);
            health.text=v.Health;damage.text=v.Damage;defense.text=v.Defense;duration.text=v.Duration;
            bagCount.text="소지품 가방  "+v.Items.Length+" / 24";
            for(int i=0;i<24;i++)
            {
                ItemView item=i<v.Items.Length?v.Items[i]:null;
                slotImages[i].image=Texture(item==null?null:item.Icon);slotNames[i].text=item==null?"":item.Name;
                slotBadges[i].text=item!=null&&item.Equipped?"장착 중":"";
                bool selected=item!=null&&v.Selected!=null&&item.Id==v.Selected.Id;
                Border(slots[i],selected?Rose:Edge,selected?2:1);
                slots[i].style.backgroundColor=selected?(Color)new Color32(61,30,38,255):item!=null?Edge:Ink;
                slots[i].tooltip=item==null?"":item.Name+"\n"+item.Affix;
                slots[i].SetEnabled(item!=null);
            }
            selectedName.text=v.Selected==null?"비교할 아이템을 선택하세요":v.Selected.Name;
            selectedDamage.text=v.Selected==null?"—":v.Selected.Damage.ToString();
            selectedIcon.image=Texture(v.Selected==null?null:v.Selected.Icon);
            comparison.text=v.Comparison??"";delta.text=v.Delta??"";affix.text=v.Affix??"";status.text=v.Status??"";
            status.tooltip=v.Status??"";equippedDetails.tooltip=v.Equipped==null?"":v.Equipped.Affix;
            points.text="잔여 "+v.Points+" PT";
            furyCaption.text="분노 "+v.Fury+" / 2";precisionCaption.text="정밀 "+v.Precision+" / 1";keystoneCaption.text="숙련 "+v.Keystone+" / 1";
            SetAction(equipButton,v.CanEquip);SetAction(salvageButton,v.CanSalvage);SetAction(furyButton,v.CanFury);SetAction(precisionButton,v.CanPrecision);SetAction(keystoneButton,v.CanKeystone);SetAction(resetButton,v.CanReset);
        }
        private void RequestDiscard()
        {
            View latest=read();
            string currentId=latest==null||latest.Selected==null?null:latest.Selected.Id;
            if(currentId==null){discardConfirm=false;return;}
            if(!discardConfirm || discardItemId!=currentId){discardConfirm=true;discardItemId=currentId;salvageButton.Q<Label>().text="버리기 확인";return;}
            discardConfirm=false;discardItemId=null;discard();
        }
        private static void SetAction(VisualElement element,bool available){element.SetEnabled(available);element.style.opacity=available?1:0.45f;}
        private Texture2D Texture(string key)
        {
            if(string.IsNullOrEmpty(key))return null;
            if(!textures.TryGetValue(key,out Texture2D texture)) {texture=Resources.Load<Texture2D>(key.Contains("/")?key:"AffixGenerated/"+key);textures[key]=texture;}
            return texture;
        }
        private VisualElement Rune(VisualElement parent,string name,string resource,float x,float y,float w,float h,Action action,out Label caption)
        {
            var node=Button(parent,name,"",x,y,w,h,action,Ink);
            var icon=Icon(node,(w-32)/2,5,32,32);icon.image=Texture(resource);
            caption=Text(node,"",4,42,w-8,23,12,Gold);caption.style.unityTextAlign=TextAnchor.MiddleCenter;return node;
        }
        private static Image Icon(VisualElement parent,float x,float y,float width,float height)
        {
            var image=new Image {scaleMode=ScaleMode.ScaleToFit,pickingMode=PickingMode.Ignore};Place(image,x,y,width,height);parent.Add(image);return image;
        }
        private static Label Stat(VisualElement parent,float x,float y,string caption)
        {
            Text(parent,caption,x,y,94,23,12,Muted);var value=Text(parent,"",x+95,y,101,23,14,Cream);value.style.unityTextAlign=TextAnchor.MiddleRight;return value;
        }
        private static void Header(VisualElement parent,string name,string title,float x,float y,float width)
        {var band=Box(parent,name,x,y,width,28,Ink);Text(band,title,10,3,width-20,24,14,Cream);}
        private static VisualElement Button(VisualElement parent,string name,string caption,float x,float y,float width,float height,Action action,Color color)
        {
            var button=Box(parent,name,x,y,width,height,color);button.pickingMode=PickingMode.Position;button.focusable=true;Border(button,Edge,1);
            Text(button,caption,0,0,width,height,12,Cream).style.unityTextAlign=TextAnchor.MiddleCenter;
            button.RegisterCallback<ClickEvent>(_=>action());button.RegisterCallback<NavigationSubmitEvent>(evt=>{action();evt.StopPropagation();});return button;
        }
        private static void Place(VisualElement element,float x,float y,float width,float height){element.style.position=Position.Absolute;element.style.left=x;element.style.top=y;element.style.width=width;element.style.height=height;}
        private static VisualElement Box(VisualElement parent,string name,float x,float y,float width,float height,Color color)
        {var box=new VisualElement{name=name,pickingMode=PickingMode.Ignore};Place(box,x,y,width,height);box.style.backgroundColor=color;parent.Add(box);return box;}
        private static Label Text(VisualElement parent,string value,float x,float y,float width,float height,int size,Color color)
        {
            var label=new Label(value){pickingMode=PickingMode.Ignore};Place(label,x,y,width,height);label.style.fontSize=size;label.style.color=color;
            label.style.marginLeft=label.style.marginRight=label.style.marginTop=label.style.marginBottom=0;
            label.style.paddingLeft=label.style.paddingRight=label.style.paddingTop=label.style.paddingBottom=0;parent.Add(label);return label;
        }
        private static void Border(VisualElement element,Color color,float width)
        {element.style.borderTopColor=element.style.borderRightColor=element.style.borderBottomColor=element.style.borderLeftColor=color;element.style.borderTopWidth=element.style.borderRightWidth=element.style.borderBottomWidth=element.style.borderLeftWidth=width;}
    }
}
