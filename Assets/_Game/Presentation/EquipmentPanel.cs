using System;
using AffixZero.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace AffixZero.Presentation
{
    // View-only snapshot keeps inventory rules and stat calculation in the encounter/core layer.
    public sealed class EquipmentPanel
    {
        public sealed class ItemView
        {
            public string Id, Name, Affix, Icon, SlotText, Summary, PrimaryValue, PrimaryLabel, Rarity;
            public int Damage, Defense, Health;
            public float Speed;
            public bool Equipped;
        }
        public sealed class View
        {
            public ItemView[] Items = Array.Empty<ItemView>();
            public ItemView[] EquippedSlots = Array.Empty<ItemView>();
            public ItemView Selected;
            public string Health, Damage, Defense, Duration, Comparison, Delta, Affix, Status;
            public int Points, Fury, Precision, Keystone, SalvageValue;
            public bool CanEquip, CanSalvage, CanFury, CanPrecision, CanKeystone, CanReset;
        }
        public readonly VisualElement Root;
        private readonly VisualElement[] slots = new VisualElement[24];
        private readonly Image[] slotImages = new Image[24];
        private readonly Label[] slotNames = new Label[24];
        private readonly Label[] slotBadges = new Label[24];
        private readonly Label health, damage, defense, duration, bagCount, selectedName, comparison, delta, affix, points, status, selectedDamage, selectedPrimaryLabel;
        private readonly Label[] equippedNames=new Label[8],equippedSummaries=new Label[8];
        private readonly Image[] equippedImages=new Image[8];
        private readonly Label furyCaption, precisionCaption, keystoneCaption;
        private readonly VisualElement equipButton, salvageButton, furyButton, precisionButton, keystoneButton, resetButton;
        private readonly Image selectedIcon;
        private readonly Func<View> read;
        private readonly Action<int> select;
        private readonly Action discard;
        private bool discardConfirm;
        private string selectedId, discardItemId;
        private static readonly Color Surface = new Color32(26,28,31,255), Ink = new Color32(12,14,17,255), Edge = new Color32(40,42,45,255);
        private static readonly Color Crimson = new Color32(196,30,58,255), Gold = new Color32(233,195,73,255), Cream = new Color32(226,226,230,255), Muted = new Color32(175,177,184,255), Sky = new Color32(151,203,255,255), Rose = new Color32(255,179,180,255);
        private readonly System.Collections.Generic.Dictionary<string,Texture2D> textures = new System.Collections.Generic.Dictionary<string,Texture2D>();

        public EquipmentPanel(VisualElement parent,Func<View> read,Action<int> select,Action equip,Action salvage,Action fury,Action precision,Action keystone,Action reset,Action close,Sprite heroPortrait,Action openTalents=null)
        {
            this.read=read;this.select=select;this.discard=salvage;
            Root=Box(parent,"character-panel",16,64,1248,516,Ink);Border(Root,Edge,1);
            Root.pickingMode=PickingMode.Position;
            var ribbon=Box(Root,"management-header",0,0,1246,42,Surface);
            Box(ribbon,"management-accent",14,14,9,14,Crimson);
            Text(ribbon,"영웅 관리",34,7,172,28,20,Cream);
            Text(ribbon,"EQUIPMENT  /  INVENTORY  /  COMPARE",207,12,400,23,12,Rose);
            Button(ribbon,"close-character","던전으로  [TAB]",1098,6,136,30,close,Edge);
            var equipment=Box(Root,"equipment-section",12,52,430,452,new Color32(52,19,27,255));Border(equipment,new Color32(111,67,54,255),1);
            Header(equipment,"equipment-heading","장착 장비  /  WEAPON · ARMOR · RELIC",14,8,678);
            var weaponCard=Box(equipment,"equipped-item-card",14,44,402,252,new Color32(24,18,20,255));
            string[] equippedIds={"equipped-weapon","equipped-helmet","equipped-armor","equipped-gloves","equipped-boots","equipped-ring","equipped-amulet","equipped-relic"};
            float[] equippedX={8,8,8,8,334,334,334,334};
            float[] equippedY={8,68,128,188,8,68,128,188};
            for(int i=0;i<8;i++)
            {
                var row=Box(weaponCard,equippedIds[i],equippedX[i],equippedY[i],60,54,Edge);Border(row,new Color32(126,82,59,255),1);
                Box(row,"equipped-accent-"+i,0,0,3,54,i==0?Gold:i==2?Sky:Rose);
                equippedImages[i]=Icon(row,13,3,34,34);
                equippedNames[i]=Text(row,"",3,38,54,14,8,Cream);equippedNames[i].style.unityTextAlign=TextAnchor.MiddleCenter;
                equippedNames[i].style.overflow=Overflow.Hidden;equippedNames[i].style.textOverflow=TextOverflow.Ellipsis;
                equippedSummaries[i]=Text(row,"",0,0,1,1,1,Muted);equippedSummaries[i].style.display=DisplayStyle.None;
            }
            var portraitBox=Box(weaponCard,"hero-portrait",94,8,214,234,Ink);Border(portraitBox,new Color32(91,64,64,255),1);
            Box(portraitBox,"hero-plinth",36,207,142,2,Edge);
            if(heroPortrait!=null)
            {
                var image=new Image {image=heroPortrait.texture,scaleMode=ScaleMode.ScaleToFit,pickingMode=PickingMode.Ignore};Place(image,17,3,180,204);portraitBox.Add(image);
                // Image.sourceRect requires a Texture, with a top-left origin; Sprite.rect uses bottom-left.
                Rect source=heroPortrait.textureRect;
                image.sourceRect=new Rect(source.x,heroPortrait.texture.height-source.yMax,source.width,source.height);
                if(heroPortrait.rect.width==100 && heroPortrait.rect.height==100)
                {
                    Rect cell=heroPortrait.rect;
                    image.sourceRect=new Rect(cell.x+30,heroPortrait.texture.height-cell.yMax+30,40,40);
                }
            }
            Text(portraitBox,"AFFIX HUNTER",55,211,104,18,9,Gold).style.unityTextAlign=TextAnchor.MiddleCenter;
            var stats=Box(equipment,"hero-stat-table",14,296,402,142,Ink);equipment.style.overflow=Overflow.Hidden;
            Text(stats,"능력치 세부 정보",10,6,196,23,14,Gold);
            health=Stat(stats,10,37,"생명력");damage=Stat(stats,10,67,"공격력");defense=Stat(stats,10,97,"방어력");duration=Stat(stats,10,127,"공격 주기");
            damage.name="equipment-damage-value";damage.style.color=Rose;damage.style.unityFontStyleAndWeight=FontStyle.Bold;
            equipment.Q("equipment-heading").style.width=402;
            var bag=Box(Root,"inventory-grid",454,52,430,452,Surface);Border(bag,new Color32(91,64,64,255),1);bag.style.overflow=Overflow.Hidden;
            Box(bag,"bag-heading",14,8,678,28,Ink);
            bagCount=Text(bag,"소지품",24,10,340,25,15,Cream);
            Text(bag,"선택 → 비교 → 장착",436,14,242,19,11,Muted).style.unityTextAlign=TextAnchor.MiddleRight;
            for(int i=0;i<24;i++)
            {
                int index=i;float x=14+(i%6)*66,y=43+(i/6)*96;
                var slot=Box(bag,"inventory-slot-"+i,x,y,62,88,Ink);Border(slot,Edge,1);slot.pickingMode=PickingMode.Position;slot.focusable=true;
                slotImages[i]=Icon(slot,11,6,40,40);slotNames[i]=Text(slot,"",4,48,54,21,9,Cream);slotNames[i].style.unityTextAlign=TextAnchor.MiddleCenter;
                slotNames[i].style.overflow=Overflow.Hidden;slotNames[i].style.textOverflow=TextOverflow.Ellipsis;
                slotBadges[i]=Text(slot,"",4,70,54,14,8,Gold);slotBadges[i].style.unityTextAlign=TextAnchor.MiddleCenter;slots[i]=slot;
                slot.RegisterCallback<ClickEvent>(_=>{discardConfirm=false;this.select(index);});slot.RegisterCallback<NavigationSubmitEvent>(evt=>{discardConfirm=false;this.select(index);evt.StopPropagation();});
            }
            bag.Q("bag-heading").style.width=402;
            var talents=Box(Root,"talent-section",730,52,506,180,Surface);talents.style.display=DisplayStyle.None;
            Header(talents,"talent-heading","특성 룬  /  MASTERY",12,8,482);
            points=Text(talents,"",287,12,100,22,12,Gold);points.name="talent-points";
            resetButton=Button(talents,"talent-reset","초기화",401,10,81,23,reset,Edge);
            Box(talents,"fury-to-precision",147,85,32,2,Crimson);
            Box(talents,"precision-to-keystone-down",323,85,30,2,Crimson);
            furyButton=Rune(talents,"talent-fury","PowerRune",19,51,128,69,fury,out furyCaption);
            precisionButton=Rune(talents,"talent-precision","PrecisionRune",184,51,128,69,precision,out precisionCaption);
            keystoneButton=Rune(talents,"talent-keystone","VeteranRune",350,51,137,69,keystone,out keystoneCaption);
            furyButton.tooltip="포인트 1 · 공격력 +3 · 최대 "+HeroProgression.GetTalentDefinition(TalentId.Fury).MaxRank+"단계";
            precisionButton.tooltip="격노 2 필요 · 포인트 1 · 공격력 +4 · 최대 "+HeroProgression.GetTalentDefinition(TalentId.Precision).MaxRank+"단계";
            keystoneButton.tooltip="정밀 1 필요 · 포인트 1 · 공격력 +6 · 최대 "+HeroProgression.GetTalentDefinition(TalentId.Keystone).MaxRank+"단계";
            Text(talents,"피해 +3 / 단계",18,123,132,19,11,Muted).style.unityTextAlign=TextAnchor.MiddleCenter;
            Text(talents,"격노 2 필요 · +4",175,123,146,19,11,Muted).style.unityTextAlign=TextAnchor.MiddleCenter;
            Text(talents,"정밀 1 필요 · +6",343,123,151,19,11,Gold).style.unityTextAlign=TextAnchor.MiddleCenter;
            Text(talents,"초반 250 XP / PT · 심화 구간 점진 증가",20,151,338,20,11,Muted);
            if(openTalents!=null)Button(talents,"open-full-talents","전체 특성 →",369,150,117,23,openTalents,Edge);
            var inspect=Box(Root,"item-comparison",896,52,340,452,new Color32(20,18,20,255));Border(inspect,new Color32(111,67,54,255),1);inspect.style.overflow=Overflow.Hidden;
            Header(inspect,"comparison-heading","아이템 비교  /  EQUIPPED → LOOT",12,8,482);
            inspect.Q("comparison-heading").style.width=316;
            var selectedSlot=Box(inspect,"selected-weapon-icon",14,48,58,58,Ink);Border(selectedSlot,Crimson,1);
            selectedIcon=Icon(selectedSlot,7,7,44,44);
            selectedName=Text(inspect,"",82,48,244,29,18,Rose);selectedName.name="selected-item-name";
            delta=Text(inspect,"",82,78,244,28,11,Sky);delta.name="comparison-delta";delta.style.whiteSpace=WhiteSpace.Normal;
            var damageStrip=Box(inspect,"selected-damage-strip",14,118,312,78,Ink);
            selectedDamage=Text(damageStrip,"",10,0,106,33,28,Cream);
            selectedPrimaryLabel=Text(damageStrip,"능력치",12,33,114,16,10,Muted);
            comparison=Text(damageStrip,"",124,7,176,64,11,Rose);comparison.style.whiteSpace=WhiteSpace.Normal;
            Text(inspect,"BASE + ROLLED OPTIONS",15,211,310,18,10,Gold);
            affix=Text(inspect,"",15,234,310,104,11,Cream);affix.style.whiteSpace=WhiteSpace.Normal;
            status=Text(inspect,"",15,350,310,42,10,Muted);status.style.whiteSpace=WhiteSpace.Normal;status.style.overflow=Overflow.Hidden;status.style.textOverflow=TextOverflow.Ellipsis;
            equipButton=Button(inspect,"equip-button","장비 교체  /  EQUIP",14,223,288,26,equip,Rose);
            equipButton.Q<Label>().style.color=Ink;
            salvageButton=Button(inspect,"salvage-button","버리기",314,223,178,26,RequestDiscard,Edge);
            Place(equipButton,14,408,182,30);equipButton.Q<Label>().text="EQUIP";
            Place(salvageButton,202,408,124,30);salvageButton.Q<Label>().text="SALVAGE";
            Root.style.display=DisplayStyle.None;
        }
        public void Refresh(bool visible)
        {
            Root.style.display=visible?DisplayStyle.Flex:DisplayStyle.None;if(!visible){discardConfirm=false;return;}
            View v=read();if(v==null)return;
            string nextId=v.Selected==null?null:v.Selected.Id;
            if(selectedId!=nextId){discardConfirm=false;selectedId=nextId;}
            salvageButton.Q<Label>().text=discardConfirm?"버리기 확인":"버리기";
            salvageButton.Q<Label>().text=discardConfirm?"CONFIRM +"+v.SalvageValue+" G":"SALVAGE +"+v.SalvageValue+" G";
            string[] equippedCaptions={"WEAPON","HELMET","ARMOR","GLOVES","BOOTS","RING","AMULET","RELIC"};
            for(int i=0;i<equippedNames.Length;i++)RefreshEquipped(i,i<v.EquippedSlots.Length?v.EquippedSlots[i]:null,equippedCaptions[i]);
            health.text=v.Health;damage.text=v.Damage;defense.text=v.Defense;duration.text=v.Duration;
            bagCount.text="소지품 가방  "+v.Items.Length+" / 24";
            for(int i=0;i<24;i++)
            {
                ItemView item=i<v.Items.Length?v.Items[i]:null;
                slotImages[i].image=Texture(item==null?null:item.Icon);slotNames[i].text=item==null?"":item.Name;
                slotBadges[i].text=item==null?"":item.Equipped?"장착 중":item.SlotText??"";
                bool selected=item!=null&&v.Selected!=null&&item.Id==v.Selected.Id;
                Border(slots[i],selected?Rose:item!=null?RarityColor(item.Rarity):Edge,selected?2:1);
                slots[i].style.backgroundColor=selected?(Color)new Color32(61,30,38,255):item!=null?Edge:Ink;
                slots[i].tooltip=item==null?"":item.Name+"\n"+item.Affix;
                slots[i].SetEnabled(item!=null);
            }
            selectedName.text=v.Selected==null?"비교할 아이템을 선택하세요":v.Selected.Name;
            selectedDamage.text=v.Selected==null?"—":v.Selected.PrimaryValue??v.Selected.Damage.ToString();
            selectedPrimaryLabel.text=v.Selected==null?"능력치":v.Selected.PrimaryLabel??"무기 피해";
            selectedIcon.image=Texture(v.Selected==null?null:v.Selected.Icon);
            selectedName.style.color=v.Selected==null?Muted:RarityColor(v.Selected.Rarity);
            comparison.text=v.Comparison??"";delta.text=v.Delta??"";affix.text=v.Affix??"";status.text=v.Status??"";
            status.tooltip=v.Status??"";
            points.text="잔여 "+v.Points+" PT";
            furyCaption.text="격노 "+v.Fury+" / "+HeroProgression.GetTalentDefinition(TalentId.Fury).MaxRank;
            precisionCaption.text="정밀 "+v.Precision+" / "+HeroProgression.GetTalentDefinition(TalentId.Precision).MaxRank;
            keystoneCaption.text="숙련 "+v.Keystone+" / "+HeroProgression.GetTalentDefinition(TalentId.Keystone).MaxRank;
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
        private static Color RarityColor(string rarity)
        {
            string value=(rarity??"").ToLowerInvariant();
            return value=="epic"?new Color32(255,86,108,255):value=="legend"?new Color32(255,143,63,255):
                value=="unique"?new Color32(184,116,255,255):value=="rare"?Gold:value=="magic"?Sky:Cream;
        }
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
        private void RefreshEquipped(int index,ItemView item,string slot)
        {
            equippedImages[index].image=Texture(item?.Icon);
            equippedNames[index].text=item==null?slot+" 미장착":item.Name;
            equippedSummaries[index].text=item==null?"가방에서 장비를 선택하세요":item.Summary??item.Affix??"";
            string description=item==null?slot+" 슬롯":slot+" · "+item.Name+"\n"+(item.Summary??item.Affix??"");
            equippedNames[index].tooltip=equippedSummaries[index].tooltip=description;
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
