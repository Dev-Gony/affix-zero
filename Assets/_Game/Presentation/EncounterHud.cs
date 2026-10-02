using System;
using System.Collections.Generic;
using AffixZero.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace AffixZero.Presentation
{
    // Native runtime UI for the connected 72 x 32 dungeon.
    // Stitch provides layout reference only; every changing value comes from FirstEncounter.
    public sealed class EncounterHud : MonoBehaviour
    {
        private static readonly Color Surface = new Color32(26, 28, 31, 250);
        private static readonly Color Ink = new Color32(12, 14, 17, 250);
        private static readonly Color Edge = new Color32(51, 53, 56, 255);
        private static readonly Color Crimson = new Color32(196, 30, 58, 255);
        private static readonly Color Gold = new Color32(233, 195, 73, 255);
        private static readonly Color Sky = new Color32(151, 203, 255, 255);
        private static readonly Color Cream = new Color32(229, 225, 223, 255);
        private static readonly Color Muted = new Color32(159, 163, 170, 255);
        private static readonly Color FrameEdge = new Color32(108, 86, 58, 255);
        private static readonly Color FrameDark = new Color32(20, 18, 18, 252);
        private static readonly Color Burgundy = new Color32(75, 22, 31, 250);
        private static readonly Color Purple = new Color32(116, 67, 173, 255);
        private FirstEncounter encounter;
        private UIDocument document;
        private PanelSettings panelSettings;
        private VisualElement root, enemyFrame, enemyFill, healthFill, xpFill, dockXpFill, attackFill, mapArea, mapHero, mapEnemy, result, pickupLoot, nextEncounter;
        private Label enemyValue, healthValue, levelValue, areaCooldown, recoveryCooldown, difficultyStatus, currency, clock, attackState, xpValue, resultText, paused, lootText;
        private VisualElement pauseButton;
        private Label pauseCaption;
        private Texture2D room, attackIcon;
        private EquipmentPanel equipmentPanel;
        private TalentPanel talentPanel;
        private ForgePanel forgePanel;
        private VisualElement dungeonTab, equipmentTab, talentTab, forgeTab;
        private VisualElement huntButton;
        private Label huntCaption, huntStatus;
        private Label enemyTitle;
        private Label sectionCaption, killCount, huntRewards, combatStats, mapCaption, mapTitle, difficultyInfo;
        private readonly VisualElement[] sectionTicks=new VisualElement[3];
        private readonly VisualElement[] difficultyButtons=new VisualElement[3];
        private VisualElement effectsButton;
        private Label saveStatus;
        private VisualElement saveRetry;
        private readonly HashSet<MeleeActor> observedActors=new HashSet<MeleeActor>();
        private int selectedItemIndex = -1;
        private Image actionIcon;
        private string actionIconResource;
        private readonly List<DamageLabel> damageLabels = new List<DamageLabel>();
        private readonly List<VisualElement> enemyMarkers = new List<VisualElement>();
        private readonly List<VisualElement> obstacleMarkers = new List<VisualElement>();
        public void Configure(FirstEncounter owner) { encounter = owner; }

        private void Start()
        {
            if (encounter == null) encounter = GetComponent<FirstEncounter>();
            panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            panelSettings.name = "Encounter Runtime Panel";
            panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panelSettings.referenceResolution = new Vector2Int(1280, 720);
            panelSettings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            panelSettings.match = 0.5f;
            panelSettings.sortingOrder = 20;
            panelSettings.themeStyleSheet = Resources.Load<ThemeStyleSheet>("AffixUI/RuntimeTheme");
            if (panelSettings.themeStyleSheet == null) Debug.LogError("Runtime UI theme missing: AffixUI/RuntimeTheme", this);
            document = gameObject.AddComponent<UIDocument>();
            document.panelSettings = panelSettings;
            root = document.rootVisualElement;
            root.name = "stitch-hud";
            root.pickingMode = PickingMode.Ignore;
            root.style.position = Position.Absolute;
            root.style.left = root.style.right = root.style.top = root.style.bottom = 0;
            root.style.color = Cream;
            root.style.fontSize = 12;
            Font korean = Resources.Load<Font>("AffixUI/Korean");
            if (korean != null) root.style.unityFontDefinition = FontDefinition.FromFont(korean);
            else Debug.LogError("HUD font missing: Resources/AffixUI/Korean", this);
            room = Resources.Load<Texture2D>("AffixOriginal/TempleRoom-v2");
            attackIcon = Resources.Load<Texture2D>("AffixGenerated/AttackIcon");
            if (room == null || attackIcon == null) Debug.LogError("HUD art missing: TempleRoom-v2 or AttackIcon", this);
            BuildTop(); BuildEnemy(); BuildMap(); BuildBottom(); BuildCharacter(); BuildResult();
            if (encounter == null) return;
            ObserveActor(encounter.Hero);ObserveActor(encounter.Enemy);
            Refresh();
        }

        private void BuildTop()
        {
            // Compact corner unit frame: real HP and XP only, with no invented mana.
            var body=Box(root,"hero-unit-frame",30,12,190,78,FrameDark);Border(body,FrameEdge,2);
            Box(body,"hero-frame-toplight",3,3,184,2,new Color(1f,.78f,.38f,.22f));
            Box(body,"hero-frame-red-corner",180,6,6,16,Crimson);
            Box(body,"hero-frame-red-foot",175,70,11,4,Burgundy);
            var portraitShell=Box(root,"hero-portrait-medallion",10,8,72,72,Ink);
            portraitShell.style.borderTopLeftRadius=portraitShell.style.borderTopRightRadius=
                portraitShell.style.borderBottomLeftRadius=portraitShell.style.borderBottomRightRadius=36;
            portraitShell.style.overflow=Overflow.Hidden;Border(portraitShell,FrameEdge,3);
            Box(root,"portrait-horn-left",6,20,7,43,FrameEdge);
            Box(root,"portrait-horn-right",78,20,7,43,FrameEdge);
            Sprite portrait=encounter.Hero==null||encounter.Hero.AnimationSet==null?null:encounter.Hero.AnimationSet.Frame(ActorClip.Idle,0);
            if(portrait!=null)
            {
                var image=new Image {image=portrait.texture,scaleMode=ScaleMode.ScaleToFit,pickingMode=PickingMode.Ignore};
                Place(image,6,6,60,60);portraitShell.Add(image);
                Rect source=portrait.textureRect;
                image.sourceRect=new Rect(source.x,portrait.texture.height-source.yMax,source.width,source.height);
            }
            var levelRibbon=Box(root,"hero-level-ribbon",19,71,55,25,FrameDark);Border(levelRibbon,FrameEdge,2);
            Text(levelRibbon,"LV",4,2,17,20,8,Muted);
            levelValue=Text(levelRibbon,"",20,1,31,22,12,Gold);levelValue.style.unityTextAlign=TextAnchor.MiddleCenter;
            Text(body,"AFFIX HUNTER",56,4,126,17,11,Gold);
            var healthTrack=Box(body,"hero-health-track",56,23,126,15,Ink);Border(healthTrack,new Color32(91,42,45,255),1);
            healthFill=Box(healthTrack,"hero-health-fill",1,1,124,13,Crimson);
            healthValue=Text(healthTrack,"",2,0,122,15,9,Color.white);healthValue.name="hero-hp-value";healthValue.style.unityTextAlign=TextAnchor.MiddleCenter;
            var progressTrack=Box(body,"hero-xp-track",56,42,126,11,Ink);Border(progressTrack,new Color32(61,45,82,255),1);
            xpFill=Box(progressTrack,"hero-xp-fill",1,1,124,9,Purple);
            xpValue=Text(progressTrack,"",2,-1,122,11,8,Cream);xpValue.name="xp-value";xpValue.style.unityTextAlign=TextAnchor.MiddleCenter;
            var difficultyPlate=Box(body,"difficulty-status",56,58,36,16,Burgundy);Border(difficultyPlate,FrameEdge,1);
            difficultyStatus=Text(difficultyPlate,"",1,0,34,16,7,Gold);difficultyStatus.style.unityTextAlign=TextAnchor.MiddleCenter;
            var areaPlate=Box(body,"area-status",96,58,38,16,Surface);Border(areaPlate,Edge,1);
            Text(areaPlate,"A",2,0,8,16,7,Muted);
            areaCooldown=Text(areaPlate,"",10,0,27,16,7,Cream);areaCooldown.style.unityTextAlign=TextAnchor.MiddleCenter;
            var recoveryPlate=Box(body,"recovery-status",138,58,44,16,Surface);Border(recoveryPlate,Edge,1);
            Text(recoveryPlate,"H",2,0,8,16,7,Muted);
            recoveryCooldown=Text(recoveryPlate,"",10,0,33,16,7,Sky);recoveryCooldown.style.unityTextAlign=TextAnchor.MiddleCenter;
        }
        private void BuildEnemy()
        {
            enemyFrame=Box(root,"enemy-health",0,12,300,40,FrameDark);
            enemyFrame.style.left=Length.Percent(50);enemyFrame.style.marginLeft=-150;Border(enemyFrame,FrameEdge,1);
            Box(enemyFrame,"target-accent",0,0,3,40,Crimson);
            enemyTitle=Text(enemyFrame,"TARGET",9,2,182,16,10,Cream);
            enemyValue=Text(enemyFrame,"",191,2,100,16,9,new Color32(255,179,180,255));
            enemyValue.name = "enemy-hp-value";
            enemyValue.style.unityTextAlign = TextAnchor.MiddleRight;
            var track=Box(enemyFrame,"enemy-track",9,21,282,10,Ink);
            enemyFill=Box(track,"enemy-fill",1,1,280,8,Crimson);
            var region=Box(root,"region-panel",0,12,150,130,FrameDark);
            region.style.left=StyleKeyword.Auto;region.style.right=172;Border(region,FrameEdge,1);
            Box(region,"region-accent",0,0,3,130,Burgundy);
            sectionCaption=Text(region,"",8,5,132,28,9,Gold);sectionCaption.style.whiteSpace=WhiteSpace.Normal;
            for(int i=0;i<sectionTicks.Length;i++)sectionTicks[i]=Box(region,"section-tick-"+i,137+i*4,7,3,11,i==0?Crimson:Edge);
            huntStatus=Text(region,"",8,34,134,25,8,Cream);huntStatus.name="autohunt-status";huntStatus.style.whiteSpace=WhiteSpace.Normal;
            killCount=Text(region,"",8,60,64,20,14,Gold);killCount.name="hunt-kills";
            clock=Text(region,"",72,61,70,18,8,Muted);clock.style.unityTextAlign=TextAnchor.MiddleRight;
            huntRewards=Text(region,"",8,80,134,16,8,Cream);
            difficultyButtons[0]=Click(region,"difficulty-scout","SCOUT",8,98,41,20,()=>encounter.Hunt.SetDifficulty(DungeonDifficulty.Scout),Surface);
            difficultyButtons[1]=Click(region,"difficulty-veteran","VET",54,98,41,20,()=>encounter.Hunt.SetDifficulty(DungeonDifficulty.Veteran),Surface);
            difficultyButtons[2]=Click(region,"difficulty-torment","TORM",100,98,42,20,()=>encounter.Hunt.SetDifficulty(DungeonDifficulty.Torment),Surface);
            for(int i=0;i<difficultyButtons.Length;i++)difficultyButtons[i].Q<Label>().style.fontSize=8;
            difficultyInfo=Text(region,"",8,119,134,10,7,Muted);
            paused = Text(root, "일시정지  /  PAUSED", 0, 59, 180, 22, 10, Gold);
            paused.style.left = Length.Percent(50); paused.style.marginLeft = -90;
            paused.style.unityTextAlign = TextAnchor.MiddleCenter;
        }
        private void BuildMap()
        {
            var frame=Box(root,"minimap",0,12,156,130,FrameDark);
            frame.style.left=StyleKeyword.Auto;frame.style.right=10;Border(frame,FrameEdge,2);
            Box(frame,"minimap-red-corner",143,4,7,16,Burgundy);
            mapTitle=Text(frame,"MAP",7,4,142,16,9,Gold);
            var map=Box(frame,"minimap-image",7,24,142,93,Ink);mapArea=map;Border(map,Edge,1);
            if (room != null) map.style.backgroundImage = new StyleBackground(room);
            else Text(map,"ROOM ART MISSING",3,36,136,18,8,Crimson);
            mapHero = Box(map,"hero-map-marker",0,0,5,5,Sky);
            mapEnemy = Box(map,"enemy-map-marker",0,0,5,5,Crimson);
            enemyMarkers.Add(mapEnemy);
            Border(mapHero,Color.white,1); Border(mapEnemy,Gold,1);
            mapCaption=Text(frame,"",7,118,142,11,7,Sky);
        }
        private void BuildBottom()
        {
            var bottom=Box(root,"bottom-hud",10,0,360,122,FrameDark);
            bottom.style.top=StyleKeyword.Auto;bottom.style.bottom=10;Border(bottom,FrameEdge,2);
            Box(bottom,"dock-toplight",4,3,352,2,new Color(1f,.78f,.38f,.18f));
            Box(bottom,"dock-red-corner",4,8,5,22,Burgundy);
            var slot=Box(bottom,"attack-slot",8,8,34,52,Surface);
            Border(slot,Crimson,2);
            if(attackIcon!=null) {
                var icon=new Image {image=attackIcon,scaleMode=ScaleMode.ScaleToFit,pickingMode=PickingMode.Ignore};
                Place(icon,5,7,24,24);slot.Add(icon);actionIcon=icon;
            }
            attackState=Text(slot,"AUTO",2,38,30,12,7,Gold);attackState.style.unityTextAlign=TextAnchor.MiddleCenter;
            var area=Box(bottom,"auto-area-skill",46,8,34,52,Surface);Border(area,Gold,1);
            Text(area,"AREA",0,7,34,18,8,Gold).style.unityTextAlign=TextAnchor.MiddleCenter;
            Text(area,"AUTO",0,27,34,15,7,Muted).style.unityTextAlign=TextAnchor.MiddleCenter;
            area.tooltip="적 2명 이상 접근 시 자동 범위 공격";
            var recovery=Box(bottom,"auto-recovery-skill",84,8,34,52,Surface);Border(recovery,Sky,1);
            Text(recovery,"HEAL",0,7,34,18,8,Sky).style.unityTextAlign=TextAnchor.MiddleCenter;
            Text(recovery,"AUTO",0,27,34,15,7,Muted).style.unityTextAlign=TextAnchor.MiddleCenter;
            recovery.tooltip="체력 45% 이하에서 자동 회복";
            for(int i=0;i<4;i++)
            {
                var empty=Box(bottom,"unassigned-skill-"+i,122+i*38,8,34,52,Surface);
                Border(empty,Edge,1);Text(empty,"—",0,10,34,27,13,Muted).style.unityTextAlign=TextAnchor.MiddleCenter;
                Text(empty,(i+4).ToString(),2,38,30,11,7,Muted).style.unityTextAlign=TextAnchor.MiddleRight;
                empty.tooltip="확장 가능한 자동 스킬 슬롯";
            }
            var attackTrack=Box(slot,"attack-elapsed",0,49,32,3,Edge);
            attackFill=Box(attackTrack,"attack-elapsed-fill",0,0,0,4,Gold);
            var xpTrack=Box(bottom,"dock-experience-line",8,65,236,6,Ink);Border(xpTrack,Edge,1);
            dockXpFill=Box(xpTrack,"dock-experience-fill",1,1,0,4,Purple);
            combatStats=Text(bottom,"",252,7,100,37,8,Cream);combatStats.style.whiteSpace=WhiteSpace.Normal;
            currency=Text(bottom,"",252,45,100,18,8,Gold);currency.name="gold-value";currency.style.unityTextAlign=TextAnchor.MiddleRight;
            saveStatus=Text(bottom,"",252,64,100,14,7,Muted);saveStatus.name="save-status";
            saveStatus.style.overflow=Overflow.Hidden;saveStatus.style.textOverflow=TextOverflow.Ellipsis;
            saveRetry=Click(bottom,"save-retry","RETRY",252,35,100,28,()=>encounter.RetrySave(),Crimson);
            dungeonTab=Click(bottom,"dungeon-tab","HUNT",8,83,36,28,()=>encounter.ShowManagement(ManagementScreen.None),Burgundy);
            equipmentTab=Click(bottom,"character-tab","BAG",48,83,42,28,()=>ToggleManagement(ManagementScreen.Equipment),Surface);
            talentTab=Click(bottom,"talents-tab","TREE",94,83,42,28,()=>ToggleManagement(ManagementScreen.Talents),Surface);
            forgeTab=Click(bottom,"forge-tab","FORGE",140,83,44,28,()=>ToggleManagement(ManagementScreen.Forge),Surface);
            effectsButton=Click(bottom,"effects-toggle","FX FULL",188,83,40,28,()=>encounter.Hunt.ToggleReducedEffects(),Surface);
            huntButton=Click(bottom,"autohunt-toggle","START",232,83,64,28,()=> {
                if(encounter.Hunt.Running)encounter.Hunt.StopHunt();else encounter.Hunt.StartHunt();
            },Crimson);
            huntCaption=huntButton.Q<Label>();
            pauseButton=Click(bottom,"pause-button","PAUSE",300,83,52,28,TogglePause,Surface);
            pauseCaption=pauseButton.Q<Label>();
            foreach(var button in new[]{dungeonTab,equipmentTab,talentTab,forgeTab,effectsButton,huntButton,pauseButton})
                button.Q<Label>().style.fontSize=8;
        }
        private VisualElement Orb(VisualElement parent,string name,float x,float y,float size,Color color,out Label value)
        {
            var shell=Box(parent,name,x,y,size,size,Ink);
            shell.style.borderTopLeftRadius=shell.style.borderTopRightRadius=shell.style.borderBottomLeftRadius=shell.style.borderBottomRightRadius=size/2;
            shell.style.overflow=Overflow.Hidden;Border(shell,new Color32(91,64,64,255),2);
            var fill=Box(shell,name+"-fill",0,0,0,0,color);
            fill.style.width=Length.Percent(100);fill.style.height=Length.Percent(100);
            fill.style.top=StyleKeyword.Auto;fill.style.bottom=0;
            var rim=Box(shell,name+"-rim",3,0,size-10,2,new Color(1,1,1,.16f));
            var shine=Box(shell,name+"-shine",8,8,37,12,new Color(1,1,1,0.13f));
            shine.style.borderBottomLeftRadius=shine.style.borderBottomRightRadius=shine.style.borderTopLeftRadius=shine.style.borderTopRightRadius=14;
            value=Text(shell,"",4,25,size-12,39,14,Cream);
            value.style.whiteSpace=WhiteSpace.Normal;value.style.unityTextAlign=TextAnchor.MiddleCenter;
            return fill;
        }
        private void BuildCharacter()
        {
            Sprite portrait=encounter.Hero==null||encounter.Hero.AnimationSet==null?null:encounter.Hero.AnimationSet.Frame(ActorClip.Idle,0);
            equipmentPanel=new EquipmentPanel(root,ReadEquipment,SelectItem,
                ()=>encounter.EquipItem(selectedItemIndex),()=>encounter.DiscardItem(selectedItemIndex),
                ()=>encounter.SpendTalent(TalentId.Fury),()=>encounter.SpendTalent(TalentId.Precision),()=>encounter.SpendTalent(TalentId.Keystone),
                ()=>encounter.ResetTalents(),()=>encounter.SetEquipmentVisible(false),portrait);
            talentPanel=new TalentPanel(root,()=> {
                var p=encounter.Progression;
                return new TalentPanel.View {Points=p.UnspentPoints,Spent=p.SpentPoints,Fury=p.FuryRank,
                    Precision=p.PrecisionRank,Keystone=p.KeystoneRank,Vitality=p.VitalityRank,Cleave=p.CleaveRank,Haste=p.HasteRank,
                    TotalDamage=p.TotalDamage,TotalHealth=p.TotalMaxHp};
            },id=>encounter.SpendTalent(id),()=>encounter.ResetTalents(),()=>encounter.ShowManagement(ManagementScreen.None));
            forgePanel=new ForgePanel(root,()=> {
                var p=encounter.Progression;var item=p.EquippedWeapon;
                return new ForgePanel.View {Name=item.Name,Icon=item.IconResource,Rank=item.EnhancementRank,
                    WeaponDamage=item.DamageBonus,TotalDamage=p.TotalDamage,Gold=p.TotalGold,Cost=p.EquippedEnhancementCost,
                    CanEnhance=p.CanEnhanceEquipped,Affix="기본 피해 +"+item.FlatDamage+"\n"+item.AffixName+"  어픽스 피해 +"+item.AffixDamage,
                    Notice=encounter.ProgressionNotice,HasItem=true,Slot=EquipmentSlot.Weapon,
                    PrimaryLabel="WEAPON ATK",PrimaryValue=item.DamageBonus.ToString(),
                    NextPrimaryValue=(item.DamageBonus+item.NextEnhancementDamage).ToString(),
                    SecondaryLabel="TOTAL ATK",SecondaryValue=p.TotalDamage.ToString(),
                    NextSecondaryValue=(p.TotalDamage+item.NextEnhancementDamage).ToString()};
            },()=>encounter.EnhanceWeapon(),()=>encounter.ShowManagement(ManagementScreen.Equipment),()=>encounter.ShowManagement(ManagementScreen.None));
        }
        private void SelectItem(int index) { selectedItemIndex=index; }
        private EquipmentPanel.View ReadEquipment()
        {
            HeroProgression p=encounter.Progression;
            if(p==null || encounter.Hero==null)return null;
            selectedItemIndex=p.Inventory.Count==0?-1:Mathf.Clamp(selectedItemIndex,-1,p.Inventory.Count-1);
            WeaponItem item=selectedItemIndex>=0?p.Inventory[selectedItemIndex]:null;
            var items=new EquipmentPanel.ItemView[p.Inventory.Count];
            for(int i=0;i<items.Length;i++)items[i]=ItemView(p.Inventory[i]);
            WeaponItem current=item==null?null:p.GetEquipped(item.EquipmentSlot);
            int damageDelta=item==null?0:item.DamageBonus-(current?.DamageBonus??0);
            int defenseDelta=item==null?0:item.DefenseBonus-(current?.DefenseBonus??0);
            int healthDelta=item==null?0:item.HealthBonus-(current?.HealthBonus??0);
            float speedDelta=item==null?0:item.SpeedBonus-(current?.SpeedBonus??0);
            return new EquipmentPanel.View {
                Items=items,EquippedSlots=new[]{ItemView(p.EquippedWeapon),ItemView(p.EquippedHelmet),ItemView(p.EquippedArmor),ItemView(p.EquippedGloves),ItemView(p.EquippedBoots),ItemView(p.EquippedRing),ItemView(p.EquippedAmulet),ItemView(p.EquippedRelic)},Selected=ItemView(item),
                Health=encounter.Hero.Hp+" / "+encounter.Hero.MaxHp,
                Damage=p.TotalDamage.ToString(),Defense=encounter.Hero.Defense.ToString(),
                Duration=encounter.Hero.AnimationSet==null?"—":encounter.Hero.AnimationSet.AttackDuration.ToString("0.00")+" s",
                Comparison=item==null?"가방에서 장비를 선택하세요.":"현재 슬롯 비교 · 피해 "+Signed(damageDelta)+"  방어 "+Signed(defenseDelta)+"  체력 "+Signed(healthDelta),
                Delta=item==null?"":"이동속도 "+(speedDelta>=0?"+":"")+(speedDelta*100f).ToString("0")+"% · "+item.EquipmentSlot,
                Affix=item==null?"":DescribeItem(item),
                Status=encounter.ProgressionNotice,Points=p.UnspentPoints,Fury=p.FuryRank,Precision=p.PrecisionRank,Keystone=p.KeystoneRank,
                CanEquip=item!=null,CanSalvage=item!=null,CanReset=p.SpentPoints>0,
                CanFury=p.UnspentPoints>0&&p.FuryRank<2,
                CanPrecision=p.UnspentPoints>0&&p.FuryRank>=2&&p.PrecisionRank<1,
                CanKeystone=p.UnspentPoints>0&&p.PrecisionRank>=1&&p.KeystoneRank<1
            };
        }
        private static EquipmentPanel.ItemView ItemView(WeaponItem item)
        {
            return item==null?null:new EquipmentPanel.ItemView {Id=item.Id,Name=item.Name,Icon=item.IconResource,
                SlotText=item.EquipmentSlot.ToString().ToUpperInvariant(),Summary=item.Rarity+" · "+PrimarySummary(item),
                PrimaryValue=PrimaryValue(item),PrimaryLabel=PrimaryLabel(item),Rarity=item.Rarity,
                Affix=DescribeItem(item),Damage=item.DamageBonus,Defense=item.DefenseBonus,Health=item.HealthBonus,Speed=item.SpeedBonus};
        }
        private static string Signed(int value)=>value>=0?"+"+value:value.ToString();
        private static string PrimaryValue(WeaponItem item)=>item.DamageBonus>0?item.DamageBonus.ToString():item.DefenseBonus>0?item.DefenseBonus.ToString():item.HealthBonus.ToString();
        private static string PrimaryLabel(WeaponItem item)=>item.DamageBonus>0?"피해":item.DefenseBonus>0?"방어":"체력";
        private static string PrimarySummary(WeaponItem item)=>PrimaryLabel(item)+" +"+PrimaryValue(item);
        private static string DescribeItem(WeaponItem item)
        {
            var lines=new List<string>{item.Rarity+" · "+item.EquipmentSlot};
            if(item.DamageBonus!=0)lines.Add("공격 +"+item.DamageBonus);
            if(item.DefenseBonus!=0)lines.Add("방어 +"+item.DefenseBonus);
            if(item.HealthBonus!=0)lines.Add("체력 +"+item.HealthBonus);
            foreach(ItemOption option in item.Options)lines.Add(option.Name+" +"+(option.Stat==AffixStat.Speed?(option.Value*100f).ToString("0")+"%":option.Value.ToString("0.#")));
            if(item.EnhancementRank>0)lines.Add("강화 +"+item.EnhancementRank);
            return string.Join("  ·  ",lines);
        }
        private void BuildResult()
        {
            result=Box(root,"encounter-result",0,132,520,118,Ink);
            result.style.left=Length.Percent(50);result.style.marginLeft=-260;
            resultText=Text(result,"",10,8,500,22,14,Gold);resultText.style.unityTextAlign=TextAnchor.MiddleCenter;
            lootText=Text(result,"",12,37,496,21,12,Cream);lootText.style.unityTextAlign=TextAnchor.MiddleCenter;
            pickupLoot=Click(result,"pickup-loot","전리품 회수",86,73,162,30,()=>encounter.CollectLoot(),Crimson);
            nextEncounter=Click(result,"next-encounter","다음 전투",270,73,162,30,()=> {
                if(encounter.Hero.IsDead)encounter.RestartEncounter();else encounter.NextEncounter();
            },Surface);
            result.style.display=DisplayStyle.None;
        }
        private void TogglePause()
        {
            if(encounter.ManagementVisible) encounter.ShowManagement(ManagementScreen.None);
            else encounter.SetPaused(!encounter.IsPaused);
        }
        private void ToggleManagement(ManagementScreen target)
        {
            encounter.ShowManagement(encounter.Screen==target?ManagementScreen.None:target);
        }
        private void Update()
        {
            if(root==null || encounter==null) return;
            // Project activeInputHandler=0: preserve the existing legacy key bindings without package changes.
            if(Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.I)) encounter.SetEquipmentVisible(!encounter.EquipmentVisible);
            if(Input.GetKeyDown(KeyCode.K)) ToggleManagement(ManagementScreen.Talents);
            if(Input.GetKeyDown(KeyCode.F)) ToggleManagement(ManagementScreen.Forge);
            if(Input.GetKeyDown(KeyCode.Escape)) TogglePause();
            if(Input.GetKeyDown(KeyCode.R) && encounter.HasEnded) encounter.RestartEncounter();
        }
        private void LateUpdate()
        {
            if(root==null || encounter==null) return;
            saveStatus.text=encounter.Persistence.Status;
            saveStatus.style.color=encounter.CanProgress?Muted:Gold;
            saveRetry.style.display=!encounter.CanProgress && encounter.Persistence.CanRetry?DisplayStyle.Flex:DisplayStyle.None;
            Refresh();UpdateDamage();
        }
        private void Refresh()
        {
            if(encounter==null || encounter.Hero==null || encounter.Enemy==null) return;
            var hero=encounter.Hero;var enemy=hero.CurrentTarget??encounter.Enemy;
            var hunt=encounter.Hunt;
            if(hunt!=null)
            {
                foreach(var actor in hunt.Enemies)ObserveActor(actor);
                huntButton.SetEnabled(hunt.Initialized);
                huntCaption.text=hunt.Running?"STOP":"START";
                huntStatus.text=hunt.StateCaption+"\nALIVE "+hunt.AliveEnemies+(hunt.KillChain>1?"   CHAIN x"+hunt.KillChain:"");
                sectionCaption.text=hunt.CurrentLayoutName+"\n"+hunt.Difficulty.Name.ToUpperInvariant()+"  ·  CLEAR "+encounter.Progression.DungeonClears;
                for(int i=0;i<sectionTicks.Length;i++)
                {
                    sectionTicks[i].style.display=DisplayStyle.Flex;
                    sectionTicks[i].style.backgroundColor=(int)hunt.World.LayoutId==i?Crimson:Edge;
                }
                killCount.text=hunt.TotalKills+"  KILLS";
                huntRewards.text="수거 "+hunt.CollectedItems+"개  ·  재도전 "+hunt.DeathRetries+"회";
                int alive=0;foreach(var actor in hunt.Enemies)if(actor.isActiveAndEnabled&&!actor.IsDead)alive++;
                mapTitle.text="MAP  /  "+hunt.CurrentLayoutName;
                mapCaption.text=hunt.Difficulty.Name.ToUpperInvariant()+"  ·  ALIVE "+alive;
                DifficultyRule rule=hunt.Difficulty;
                difficultyInfo.text="HPx"+rule.EnemyHealthMultiplier.ToString("0.00")+"  DMGx"+rule.EnemyDamageMultiplier.ToString("0.00")+
                    "  DROPx"+rule.DropMultiplier.ToString("0.00");
                difficultyStatus.text=hunt.Difficulty.Name.ToUpperInvariant();
                for(int i=0;i<difficultyButtons.Length;i++)
                {
                    difficultyButtons[i].SetEnabled(hunt.CanChangeDifficulty&&(int)hunt.CurrentDifficulty!=i);
                    difficultyButtons[i].style.backgroundColor=(int)hunt.CurrentDifficulty==i?Crimson:Surface;
                }
                effectsButton.Q<Label>().text=hunt.ReducedEffects?"FX LOW":"FX FULL";
                RefreshObstacleMarkers(hunt.World);
            }
            bool hasTarget=hero.CurrentTarget!=null&&hero.CurrentTarget.isActiveAndEnabled&&!hero.CurrentTarget.IsDead;
            enemyFrame.style.display=hasTarget?DisplayStyle.Flex:DisplayStyle.None;
            enemyFill.style.width=Length.Percent(100f*enemy.Hp/Mathf.Max(1,enemy.MaxHp));
            enemyValue.text=enemy.Hp+" / "+enemy.MaxHp;
            enemyTitle.text=hasTarget?enemy.name.ToUpperInvariant():"";
            healthFill.style.width=Length.Percent(100f*hero.Hp/Mathf.Max(1,hero.MaxHp));
            healthValue.text=hero.Hp+" / "+hero.MaxHp+" HP";
            int levelXp=encounter.Progression.TotalExperience%HeroProgression.ExperiencePerLevel;
            float xpPercent=100f*levelXp/HeroProgression.ExperiencePerLevel;
            xpFill.style.width=Length.Percent(xpPercent);dockXpFill.style.width=Length.Percent(xpPercent);
            xpValue.text="XP  "+levelXp+" / "+HeroProgression.ExperiencePerLevel;
            levelValue.text=encounter.Progression.Level.ToString();
            if(hunt!=null)
            {
                areaCooldown.text=hunt.AreaCooldownRemaining<=0?"READY":hunt.AreaCooldownRemaining.ToString("0.0")+"s";
                recoveryCooldown.text=hunt.RecoveryCooldownRemaining<=0?"READY":hunt.RecoveryCooldownRemaining.ToString("0.0")+"s";
            }
            currency.text="GOLD  "+encounter.Progression.TotalGold;
            combatStats.text="ATK "+encounter.Progression.TotalDamage+"   DEF "+hero.Defense+
                "\nBAG "+encounter.Progression.Inventory.Count+"/24   PT "+encounter.Progression.UnspentPoints;
            int seconds=Mathf.FloorToInt(encounter.ElapsedSeconds);
            clock.text="TEMPLE  "+(seconds/60).ToString("00")+":"+(seconds%60).ToString("00");
            pauseCaption.text=encounter.IsPaused?"RESUME":"PAUSE";
            paused.style.display=encounter.IsPaused&&!encounter.ManagementVisible&&!encounter.HasEnded?DisplayStyle.Flex:DisplayStyle.None;
            attackState.text=encounter.HasEnded?"END":encounter.IsPaused?"PAUSE":hero.IsAttacking?"STRIKE":hero.CurrentClip==ActorClip.Hit?"HIT":hero.CurrentClip==ActorClip.Walk?"MOVE":"AUTO";
            float progress=hero.IsAttacking && hero.AnimationSet!=null?(float)(hero.AttackElapsed/hero.AnimationSet.AttackDuration):0;
            attackFill.style.width=Length.Percent(100*Mathf.Clamp01(progress));
            PositionMarker(mapHero,hero.transform.position);
            if(hunt!=null)RefreshEnemyMarkers(hunt.Enemies);
            equipmentPanel.Refresh(encounter.EquipmentVisible);
            talentPanel.Refresh(encounter.Screen==ManagementScreen.Talents);
            forgePanel.Refresh(encounter.Screen==ManagementScreen.Forge);
            dungeonTab.style.backgroundColor=encounter.Screen==ManagementScreen.None?Crimson:Surface;
            equipmentTab.style.backgroundColor=encounter.Screen==ManagementScreen.Equipment?Crimson:Surface;
            talentTab.style.backgroundColor=encounter.Screen==ManagementScreen.Talents?Crimson:Surface;
            forgeTab.style.backgroundColor=encounter.Screen==ManagementScreen.Forge?Crimson:Surface;
            if(encounter.Progression!=null && actionIcon!=null && actionIconResource!=encounter.Progression.EquippedWeapon.IconResource)
            {
                actionIconResource=encounter.Progression.EquippedWeapon.IconResource;
                actionIcon.image=Resources.Load<Texture2D>(actionIconResource);
            }
            bool ended=encounter.HasEnded&&encounter.SecondsSinceEnd>=0.8f;
            // Auto hunting owns recovery, loot and reruns; no per-kill Continue prompt.
            result.style.display=DisplayStyle.None;
            var loot=encounter.Progression==null?null:encounter.Progression.PendingLoot;
            lootText.text=loot==null?encounter.ProgressionNotice:"전리품  ·  "+loot.Name;
            pickupLoot.SetEnabled(loot!=null && !hero.IsDead);pickupLoot.style.opacity=loot==null?0.4f:1;
            nextEncounter.SetEnabled(hero.IsDead || loot==null);nextEncounter.style.opacity=hero.IsDead||loot==null?1:0.4f;
            nextEncounter.Q<Label>().text=hero.IsDead?"다시 시작 [R]":"다음 전투";
            resultText.text=hero.IsDead?"영웅 쓰러짐":"전투 완료  +"+encounter.Experience+" XP  +"+encounter.Gold+" GOLD";
        }
        private static void PositionMarker(VisualElement marker,Vector3 world)
        {
            marker.style.left=Mathf.InverseLerp(DungeonWorld.Bounds.xMin,DungeonWorld.Bounds.xMax,world.x)*137;
            marker.style.top=(1-Mathf.InverseLerp(DungeonWorld.Bounds.yMin,DungeonWorld.Bounds.yMax,world.y))*88;
        }
        private void RefreshEnemyMarkers(IReadOnlyList<MeleeActor> enemies)
        {
            while(enemyMarkers.Count<enemies.Count)
            {
                var marker=Box(mapArea,"enemy-map-marker-"+enemyMarkers.Count,0,0,4,4,Crimson);Border(marker,Gold,1);enemyMarkers.Add(marker);
            }
            for(int i=0;i<enemyMarkers.Count;i++)
            {
                bool visible=i<enemies.Count&&enemies[i].isActiveAndEnabled&&!enemies[i].IsDead;
                enemyMarkers[i].style.display=visible?DisplayStyle.Flex:DisplayStyle.None;
                if(visible)PositionMarker(enemyMarkers[i],enemies[i].transform.position);
            }
        }
        private void RefreshObstacleMarkers(DungeonWorld world)
        {
            if(world==null)return;
            while(obstacleMarkers.Count<world.ObstacleCenters.Count)
            {
                var marker=Box(mapArea,"obstacle-map-marker-"+obstacleMarkers.Count,0,0,5,5,world.AccentColor);
                marker.style.opacity=.8f;obstacleMarkers.Add(marker);
            }
            for(int i=0;i<obstacleMarkers.Count;i++)
            {
                bool visible=i<world.ObstacleCenters.Count;
                obstacleMarkers[i].style.display=visible?DisplayStyle.Flex:DisplayStyle.None;
                if(visible)PositionMarker(obstacleMarkers[i],world.ObstacleCenters[i]);
            }
        }
        private void OnDamaged(MeleeActor actor,HitReceipt receipt)
        {
            if(!receipt.Accepted || root==null || encounter.ManagementVisible) return;
            if(damageLabels.Count==20) {damageLabels[0].label.RemoveFromHierarchy();damageLabels.RemoveAt(0);}
            string prefix=actor.LastHitKilled?"KO  ":actor.LastHitCritical?"CRIT  ":"";
            int size=actor.LastHitKilled?28:actor.LastHitCritical?25:22;
            Color color=actor==encounter.Hero?new Color(1,0.48f,0.36f):actor.LastHitKilled?new Color(1,.38f,.16f):actor.LastHitCritical?Color.white:Gold;
            var label=Text(root,prefix+receipt.Damage,0,0,110,36,size,color);
            label.style.unityFontStyleAndWeight=FontStyle.Bold;label.style.unityTextAlign=TextAnchor.MiddleCenter;
            damageLabels.Add(new DamageLabel {label=label,position=actor.transform.position+Vector3.up*(actor.LastHitKilled?1f:.8f),time=Time.time});
        }
        private void UpdateDamage()
        {
            Camera camera=Camera.main;
            for(int i=damageLabels.Count-1;i>=0;i--)
            {
                var item=damageLabels[i];float age=Time.time-item.time;
                if(age>0.7f || encounter.ManagementVisible) {item.label.RemoveFromHierarchy();damageLabels.RemoveAt(i);continue;}
                if(camera==null || root.panel==null) continue;
                Vector3 screen=camera.WorldToScreenPoint(item.position+Vector3.up*(age*0.65f));
                Vector2 point=RuntimePanelUtils.ScreenToPanel(root.panel,new Vector2(screen.x,Screen.height-screen.y));
                item.label.style.left=point.x-55;item.label.style.top=point.y-18;item.label.style.opacity=Mathf.Clamp01((0.7f-age)/0.25f);
            }
        }
        private void OnDestroy()
        {
            foreach(var actor in observedActors)if(actor!=null)actor.Damaged-=OnDamaged;
            if(document!=null) Destroy(document);
            if(panelSettings!=null) Destroy(panelSettings);
        }
        private void ObserveActor(MeleeActor actor){if(actor!=null&&observedActors.Add(actor))actor.Damaged+=OnDamaged;}
        private static void Place(VisualElement element,float x,float y,float width,float height)
        {
            element.style.position=Position.Absolute;element.style.left=x;element.style.top=y;element.style.width=width;element.style.height=height;
        }
        private static VisualElement Box(VisualElement parent,string name,float x,float y,float width,float height,Color color)
        {
            var element=new VisualElement {name=name,pickingMode=PickingMode.Ignore};Place(element,x,y,width,height);element.style.backgroundColor=color;parent.Add(element);return element;
        }
        private static Label Text(VisualElement parent,string text,float x,float y,float width,float height,int size,Color color)
        {
            var label=new Label(text){pickingMode=PickingMode.Ignore};Place(label,x,y,width,height);label.style.fontSize=size;label.style.color=color;
            label.style.marginLeft=label.style.marginRight=label.style.marginTop=label.style.marginBottom=0;
            label.style.paddingLeft=label.style.paddingRight=label.style.paddingTop=label.style.paddingBottom=0;
            parent.Add(label);return label;
        }
        private static VisualElement Click(VisualElement parent,string name,string caption,float x,float y,float width,float height,Action action,Color color)
        {
            var element=Box(parent,name,x,y,width,height,color);element.pickingMode=PickingMode.Position;element.focusable=true;Border(element,Edge,1);
            var text=Text(element,caption,0,0,width,height,12,Cream);text.style.unityTextAlign=TextAnchor.MiddleCenter;
            element.RegisterCallback<ClickEvent>(_=>action());
            element.RegisterCallback<NavigationSubmitEvent>(evt=>{action();evt.StopPropagation();});
            element.RegisterCallback<MouseEnterEvent>(_=>element.style.backgroundColor=new Color(color.r+0.06f,color.g+0.06f,color.b+0.06f,1));
            element.RegisterCallback<MouseLeaveEvent>(_=>element.style.backgroundColor=color);
            return element;
        }
        private static void Border(VisualElement element,Color color,float width)
        {
            element.style.borderTopColor=element.style.borderRightColor=element.style.borderBottomColor=element.style.borderLeftColor=color;
            element.style.borderTopWidth=element.style.borderRightWidth=element.style.borderBottomWidth=element.style.borderLeftWidth=width;
        }
        private sealed class DamageLabel {public Label label;public Vector3 position;public float time;}
    }
}
