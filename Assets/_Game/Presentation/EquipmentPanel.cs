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
        private readonly Label equippedName, equippedDetails, health, damage, defense, duration, bagCount, selectedName, comparison, delta, affix, points, status;
        private readonly Label furyCaption, precisionCaption, keystoneCaption;
        private readonly VisualElement equipButton, salvageButton, furyButton, precisionButton, keystoneButton, resetButton;
        private readonly Image equippedIcon;
        private readonly Func<View> read;
        private readonly Action<int> select;
        private readonly Action discard;
        private bool discardConfirm;
        private string selectedId, discardItemId;
        private static readonly Color Surface = new Color32(25,28,30,255), Ink = new Color32(10,12,15,255), Edge = new Color32(65,66,70,255);
        private static readonly Color Crimson = new Color32(196,30,58,255), Gold = new Color32(233,195,73,255), Cream = new Color32(229,225,223,255), Muted = new Color32(159,163,170,255), Sky = new Color32(151,203,255,255);
        private readonly System.Collections.Generic.Dictionary<string,Texture2D> textures = new System.Collections.Generic.Dictionary<string,Texture2D>();

        public EquipmentPanel(VisualElement parent,Func<View> read,Action<int> select,Action equip,Action salvage,Action fury,Action precision,Action keystone,Action reset,Action close,Sprite heroPortrait)
        {
            this.read=read;this.select=select;this.discard=salvage;
            Root=Box(parent,"character-panel",18,60,1244,500,Ink);Border(Root,Edge,1);
            Root.pickingMode=PickingMode.Position;
            Text(Root,"영웅 관리  /  EQUIPMENT & TALENTS",18,10,810,30,19,Cream);
            Button(Root,"close-character","닫기  [TAB]",1106,9,121,31,close,Surface);
            var equipment=Box(Root,"equipment-section",12,49,702,188,Surface);
            Text(equipment,"장착 무기",12,9,178,23,14,Gold);
            var weaponSlot=Box(equipment,"equipped-weapon",14,45,64,64,Ink);Border(weaponSlot,Gold,1);
            equippedIcon=Icon(weaponSlot,12,12,40,40);
            equippedName=Text(equipment,"",90,48,214,25,15,Cream);
            equippedDetails=Text(equipment,"",90,81,214,65,12,Muted);equippedDetails.style.whiteSpace=WhiteSpace.Normal;
            var portraitBox=Box(equipment,"hero-portrait",318,39,112,133,Ink);
            if(heroPortrait!=null)
            {
                var image=new Image {image=heroPortrait.texture,scaleMode=ScaleMode.ScaleToFit,pickingMode=PickingMode.Ignore};Place(image,0,5,112,122);portraitBox.Add(image);
                // Image.sourceRect requires a Texture, with a top-left origin; Sprite.rect uses bottom-left.
                Rect source=heroPortrait.textureRect;
                image.sourceRect=new Rect(source.x,heroPortrait.texture.height-source.yMax,source.width,source.height);
                if(heroPortrait.rect.width==100 && heroPortrait.rect.height==100)
                {
                    Rect cell=heroPortrait.rect;
                    image.sourceRect=new Rect(cell.x+30,heroPortrait.texture.height-cell.yMax+30,40,40);
                }
            }
            Text(portraitBox,"HERO",8,112,96,18,10,Gold).style.unityTextAlign=TextAnchor.MiddleCenter;
            health=Stat(equipment,453,42,"생명력");damage=Stat(equipment,453,75,"공격력");defense=Stat(equipment,453,108,"방어력");duration=Stat(equipment,453,141,"공격 주기");
            var bag=Box(Root,"inventory-grid",12,249,702,239,Surface);
            bagCount=Text(bag,"소지품",12,8,386,23,14,Gold);
            Text(bag,"아이템 선택 → 비교 → 장착",403,11,282,20,11,Muted);
            for(int i=0;i<24;i++)
            {
                int index=i;float x=14+(i%8)*84,y=40+(i/8)*62;
                var slot=Box(bag,"inventory-slot-"+i,x,y,78,56,Ink);Border(slot,Edge,1);slot.pickingMode=PickingMode.Position;slot.focusable=true;
                slotImages[i]=Icon(slot,22,5,28,28);slotNames[i]=Text(slot,"",3,35,72,17,9,Cream);slotNames[i].style.unityTextAlign=TextAnchor.MiddleCenter;
                slotBadges[i]=Text(slot,"",3,1,72,13,8,Gold);slots[i]=slot;
                slot.RegisterCallback<ClickEvent>(_=>{discardConfirm=false;this.select(index);});slot.RegisterCallback<NavigationSubmitEvent>(evt=>{discardConfirm=false;this.select(index);evt.StopPropagation();});
            }
            var talents=Box(Root,"talent-section",726,49,506,204,Surface);
            Text(talents,"특성 룬 마스터리",12,8,250,24,15,Cream);
            points=Text(talents,"",270,11,106,20,11,Gold);points.name="talent-points";
            resetButton=Button(talents,"talent-reset","초기화",393,7,101,27,reset,Ink);
            Box(talents,"fury-to-precision",166,68,160,1,Edge);
            Text(talents,"→",235,51,26,25,17,Gold);
            Box(talents,"precision-to-keystone-down",465,68,1,86,Edge);
            Box(talents,"precision-to-keystone-out",449,68,17,1,Edge);
            Box(talents,"precision-to-keystone-left",308,154,158,1,Edge);
            Text(talents,"←",345,138,26,25,17,Gold);
            furyButton=Rune(talents,"talent-fury","PowerRune",43,45,123,47,fury,out furyCaption);
            precisionButton=Rune(talents,"talent-precision","PrecisionRune",326,45,123,47,precision,out precisionCaption);
            keystoneButton=Rune(talents,"talent-keystone","VeteranRune",185,131,123,47,keystone,out keystoneCaption);
            furyButton.tooltip="포인트 1 · 공격력 +3 · 최대 2단계";
            precisionButton.tooltip="분노 2 필요 · 포인트 1 · 공격력 +4";
            keystoneButton.tooltip="정밀 1 필요 · 포인트 1 · 공격력 +6";
            Text(talents,"분노 / 피해 +3",27,95,158,17,10,Muted).style.unityTextAlign=TextAnchor.MiddleCenter;
            Text(talents,"분노 2 필요 · 피해 +4",308,95,174,17,10,Muted).style.unityTextAlign=TextAnchor.MiddleCenter;
            Text(talents,"정밀 1 필요 · 피해 +6",161,180,175,17,10,Gold).style.unityTextAlign=TextAnchor.MiddleCenter;
            var inspect=Box(Root,"item-comparison",726,265,506,223,Surface);
            Text(inspect,"아이템 비교  /  EQUIPPED → SELECTED",12,8,478,23,13,Cream);
            selectedName=Text(inspect,"",14,38,474,26,19,Gold);selectedName.name="selected-item-name";
            comparison=Text(inspect,"",14,72,474,23,13,Cream);
            delta=Text(inspect,"",14,98,474,22,13,Sky);delta.name="comparison-delta";
            affix=Text(inspect,"",14,126,474,40,12,Muted);affix.style.whiteSpace=WhiteSpace.Normal;
            equipButton=Button(inspect,"equip-button","장착",14,178,146,32,equip,Crimson);
            salvageButton=Button(inspect,"salvage-button","버리기",170,178,100,32,RequestDiscard,Ink);
            status=Text(inspect,"",282,178,210,35,10,Muted);status.style.whiteSpace=WhiteSpace.Normal;
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
                Border(slots[i],item!=null&&v.Selected!=null&&item.Id==v.Selected.Id?Gold:Edge,1);
                slots[i].SetEnabled(item!=null);
            }
            selectedName.text=v.Selected==null?"비교할 아이템을 선택하세요":v.Selected.Name;
            comparison.text=v.Comparison??"";delta.text=v.Delta??"";affix.text=v.Affix??"";status.text=v.Status??"";
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
            var icon=Icon(node,8,9,28,28);icon.image=Texture(resource);
            caption=Text(node,"",43,13,76,22,11,Gold);return node;
        }
        private static Image Icon(VisualElement parent,float x,float y,float width,float height)
        {
            var image=new Image {scaleMode=ScaleMode.ScaleToFit,pickingMode=PickingMode.Ignore};Place(image,x,y,width,height);parent.Add(image);return image;
        }
        private static Label Stat(VisualElement parent,float x,float y,string caption)
        {
            Text(parent,caption,x,y,98,23,12,Muted);var value=Text(parent,"",x+92,y,134,23,13,Cream);value.style.unityTextAlign=TextAnchor.MiddleRight;return value;
        }
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
