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
        private VisualElement root, healthFill, xpFill, dockXpFill, attackFill, mapArea, mapHero, mapEnemy, result, pickupLoot, nextEncounter;
        private VisualElement bossFrame, bossHealthFill;
        private Label healthValue, levelValue, areaCooldown, recoveryCooldown, difficultyStatus, currency, clock, attackState, xpValue, resultText, paused, lootText;
        private Label areaSkillLabel, recoverySkillLabel;
        private Label bossName, bossHealthValue, bossPattern;
        private VisualElement areaSkillSlot, recoverySkillSlot;
        private VisualElement pauseButton;
        private Label pauseCaption;
        private readonly VisualElement[] speedButtons=new VisualElement[3];
        private Label speedValue;
        private Texture2D room, attackIcon;
        private EquipmentPanel equipmentPanel;
        private TalentPanel talentPanel;
        private ForgePanel forgePanel;
        private VisualElement dungeonTab, equipmentTab, talentTab, forgeTab;
        private VisualElement huntButton;
        private Label huntCaption, huntStatus;
        private Label sectionCaption, killCount, huntRewards, combatStats, mapCaption, mapTitle, difficultyInfo;
        private readonly VisualElement[] sectionTicks=new VisualElement[3];
        private readonly VisualElement[] difficultyButtons=new VisualElement[3];
        private VisualElement effectsButton;
        private Label saveStatus;
        private VisualElement saveRetry;
        private readonly HashSet<MeleeActor> observedActors=new HashSet<MeleeActor>();
        private int selectedItemIndex = -1;
        private EquipmentSlot? selectedEquippedSlot;
        private int selectedSocketIndex,selectedRuneIndex;
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
            BuildTop(); BuildBoss(); BuildEnemy(); BuildMap(); BuildBottom(); BuildCharacter(); BuildResult();
            if (encounter == null) return;
            ObserveActor(encounter.Hero);ObserveActor(encounter.Enemy);
            Refresh();
        }

        private void BuildTop()
        {
            var body=Box(root,"hero-unit-frame",86,14,218,78,FrameDark);
            Skin(body,"AffixUIVisual/FrameBar");
            var crest=Box(root,"hero-crest",6,5,106,106,Color.clear);
            Skin(crest,"AffixUIVisual/FrameCrest");
            var portraitShell=Box(crest,"hero-portrait-medallion",18,21,70,66,Ink);
            portraitShell.style.overflow=Overflow.Hidden;
            Sprite portrait=encounter.Hero==null||encounter.Hero.AnimationSet==null?null:encounter.Hero.AnimationSet.Frame(ActorClip.Idle,0);
            if(portrait!=null)
            {
                var image=new Image {image=portrait.texture,scaleMode=ScaleMode.ScaleToFit,pickingMode=PickingMode.Ignore};
                Place(image,4,2,62,62);portraitShell.Add(image);
                Rect source=portrait.textureRect;
                image.sourceRect=new Rect(source.x,portrait.texture.height-source.yMax,source.width,source.height);
            }
            var levelRibbon=Box(root,"hero-level-ribbon",24,88,64,22,new Color32(26,18,18,255));Border(levelRibbon,FrameEdge,1);
            Text(levelRibbon,"레벨",3,2,21,20,7,Muted);
            levelValue=Text(levelRibbon,"",22,1,36,20,12,Gold);levelValue.style.unityTextAlign=TextAnchor.MiddleCenter;
            Text(body,"AFFIX: ZERO",28,7,174,17,11,Gold);
            var healthTrack=Box(body,"hero-health-track",18,28,184,12,Ink);Border(healthTrack,new Color32(91,42,45,255),1);
            healthFill=Box(healthTrack,"hero-health-fill",1,1,182,10,Crimson);
            healthValue=Text(healthTrack,"",2,-1,180,12,8,Color.white);healthValue.name="hero-hp-value";healthValue.style.unityTextAlign=TextAnchor.MiddleCenter;
            var progressTrack=Box(body,"hero-xp-track",18,44,184,7,Ink);Border(progressTrack,new Color32(61,45,82,255),1);
            xpFill=Box(progressTrack,"hero-xp-fill",1,1,182,5,Purple);
            xpValue=Text(progressTrack,"",2,-3,180,10,7,Cream);xpValue.name="xp-value";xpValue.style.unityTextAlign=TextAnchor.MiddleCenter;
            var difficultyPlate=Box(body,"difficulty-status",18,57,46,15,Burgundy);Border(difficultyPlate,FrameEdge,1);
            difficultyStatus=Text(difficultyPlate,"",1,0,44,15,7,Gold);difficultyStatus.style.unityTextAlign=TextAnchor.MiddleCenter;
            var areaPlate=Box(body,"area-status",70,57,58,15,Surface);Border(areaPlate,Edge,1);
            Text(areaPlate,"A",2,0,8,16,7,Muted);
            areaCooldown=Text(areaPlate,"",10,0,46,15,7,Cream);areaCooldown.style.unityTextAlign=TextAnchor.MiddleCenter;
            var recoveryPlate=Box(body,"recovery-status",134,57,68,15,Surface);Border(recoveryPlate,Edge,1);
            Text(recoveryPlate,"H",2,0,8,16,7,Muted);
            recoveryCooldown=Text(recoveryPlate,"",10,0,56,15,7,Sky);recoveryCooldown.style.unityTextAlign=TextAnchor.MiddleCenter;
        }
        private void BuildBoss()
        {
            bossFrame=Box(root,"boss-frame",0,11,382,58,FrameDark);
            bossFrame.style.left=Length.Percent(50);bossFrame.style.marginLeft=-191;bossFrame.style.display=DisplayStyle.None;
            Skin(bossFrame,"AffixUIVisual/FrameBar");Border(bossFrame,new Color32(116,64,55,255),2);
            Box(bossFrame,"boss-corner-left",7,7,5,34,Crimson);
            var right=Box(bossFrame,"boss-corner-right",370,7,5,34,Crimson);
            right.style.opacity=.85f;
            bossName=Text(bossFrame,"",18,4,346,17,10,Gold);bossName.name="boss-name";bossName.style.unityTextAlign=TextAnchor.MiddleCenter;
            var track=Box(bossFrame,"boss-health-track",22,23,338,13,Ink);Border(track,new Color32(126,48,54,255),1);
            bossHealthFill=Box(track,"boss-health-fill",1,1,336,11,new Color32(174,24,46,255));
            bossHealthValue=Text(track,"",2,-1,334,13,8,Color.white);bossHealthValue.name="boss-hp-value";bossHealthValue.style.unityTextAlign=TextAnchor.MiddleCenter;
            bossPattern=Text(bossFrame,"구역 우두머리",18,39,346,13,7,Muted);bossPattern.name="boss-pattern-state";bossPattern.style.unityTextAlign=TextAnchor.MiddleCenter;
        }
        private void BuildEnemy()
        {
            var region=Box(root,"region-panel",0,12,150,130,FrameDark);
            region.style.left=StyleKeyword.Auto;region.style.right=172;Skin(region,"AffixUIVisual/FramePanel");Border(region,FrameEdge,1);
            Box(region,"region-accent",0,0,3,130,Burgundy);
            sectionCaption=Text(region,"",8,5,132,28,9,Gold);sectionCaption.style.whiteSpace=WhiteSpace.Normal;
            for(int i=0;i<sectionTicks.Length;i++)sectionTicks[i]=Box(region,"section-tick-"+i,137+i*4,7,3,11,i==0?Crimson:Edge);
            huntStatus=Text(region,"",8,34,134,25,8,Cream);huntStatus.name="autohunt-status";huntStatus.style.whiteSpace=WhiteSpace.Normal;
            killCount=Text(region,"",8,60,64,20,14,Gold);killCount.name="hunt-kills";
            clock=Text(region,"",72,61,70,18,8,Muted);clock.style.unityTextAlign=TextAnchor.MiddleRight;
            huntRewards=Text(region,"",8,80,134,16,8,Cream);
            difficultyButtons[0]=Click(region,"difficulty-scout","정찰",8,98,41,20,()=>encounter.Hunt.SetDifficulty(DungeonDifficulty.Scout),Surface);
            difficultyButtons[1]=Click(region,"difficulty-veteran","숙련",54,98,41,20,()=>encounter.Hunt.SetDifficulty(DungeonDifficulty.Veteran),Surface);
            difficultyButtons[2]=Click(region,"difficulty-torment","고행",100,98,42,20,()=>encounter.Hunt.SetDifficulty(DungeonDifficulty.Torment),Surface);
            for(int i=0;i<difficultyButtons.Length;i++)difficultyButtons[i].Q<Label>().style.fontSize=8;
            difficultyInfo=Text(region,"",8,119,134,10,7,Muted);
            paused = Text(root, "일시정지", 0, 59, 180, 22, 10, Gold);
            paused.style.left = Length.Percent(50); paused.style.marginLeft = -90;
            paused.style.unityTextAlign = TextAnchor.MiddleCenter;
        }
        private void BuildMap()
        {
            var frame=Box(root,"minimap",0,12,156,130,FrameDark);
            frame.style.left=StyleKeyword.Auto;frame.style.right=10;Skin(frame,"AffixUIVisual/FramePanel");Border(frame,FrameEdge,2);
            Box(frame,"minimap-red-corner",143,4,7,16,Burgundy);
            mapTitle=Text(frame,"지도",7,4,142,16,9,Gold);
            var map=Box(frame,"minimap-image",7,24,142,93,Ink);mapArea=map;Border(map,Edge,1);
            if (room != null) map.style.backgroundImage = new StyleBackground(room);
            else Text(map,"지도 이미지를 찾을 수 없음",3,36,136,18,7,Crimson);
            mapHero = Box(map,"hero-map-marker",0,0,5,5,Sky);
            mapEnemy = Box(map,"enemy-map-marker",0,0,5,5,Crimson);
            enemyMarkers.Add(mapEnemy);
            Border(mapHero,Color.white,1); Border(mapEnemy,Gold,1);
            mapCaption=Text(frame,"",7,118,142,11,7,Sky);
        }
        private void BuildBottom()
        {
            var bottom=Box(root,"bottom-hud",8,0,364,132,Color.clear);
            bottom.style.top=StyleKeyword.Auto;bottom.style.bottom=8;Skin(bottom,"AffixUIVisual/FrameDock");
            var slot=Box(bottom,"attack-slot",18,16,42,52,Surface);
            Skin(slot,"AffixUIVisual/FrameSlotGold");Border(slot,Crimson,1);
            if(attackIcon!=null) {
                var icon=new Image {image=attackIcon,scaleMode=ScaleMode.ScaleToFit,pickingMode=PickingMode.Ignore};
                Place(icon,6,5,30,30);slot.Add(icon);actionIcon=icon;
            }
            attackState=Text(slot,"자동",2,37,38,11,7,Gold);attackState.style.unityTextAlign=TextAnchor.MiddleCenter;
            var area=Box(bottom,"auto-area-skill",64,16,42,52,Surface);areaSkillSlot=area;Skin(area,"AffixUIVisual/FrameSlotGold");Border(area,Gold,1);
            var areaIcon=new Image {image=Resources.Load<Texture2D>("AffixUIVisual/SkillArea"),scaleMode=ScaleMode.ScaleToFit,pickingMode=PickingMode.Ignore};Place(areaIcon,6,5,30,30);area.Add(areaIcon);
            areaSkillLabel=Text(area,"범위",1,37,40,11,7,Gold);areaSkillLabel.style.unityTextAlign=TextAnchor.MiddleCenter;
            area.tooltip="적 2명 이상 접근 시 자동 범위 공격";
            var recovery=Box(bottom,"auto-recovery-skill",110,16,42,52,Surface);recoverySkillSlot=recovery;Skin(recovery,"AffixUIVisual/FrameSlotSilver");Border(recovery,Sky,1);
            var healIcon=new Image {image=Resources.Load<Texture2D>("AffixUIVisual/SkillHeal"),scaleMode=ScaleMode.ScaleToFit,pickingMode=PickingMode.Ignore};Place(healIcon,6,5,30,30);recovery.Add(healIcon);
            recoverySkillLabel=Text(recovery,"회복",1,37,40,11,7,Sky);recoverySkillLabel.style.unityTextAlign=TextAnchor.MiddleCenter;
            recovery.tooltip="체력 45% 이하에서 자동 회복";
            for(int i=0;i<4;i++)
            {
                var empty=Box(bottom,"unassigned-skill-"+i,156+i*40,16,36,52,Surface);
                Skin(empty,"AffixUIVisual/FrameSlotSilver");empty.style.opacity=.46f;
                Border(empty,Edge,1);Text(empty,"—",0,10,34,27,13,Muted).style.unityTextAlign=TextAnchor.MiddleCenter;
                Text(empty,(i+4).ToString(),3,37,29,11,7,Muted).style.unityTextAlign=TextAnchor.MiddleRight;
                empty.tooltip="확장 가능한 자동 스킬 슬롯";
            }
            var attackTrack=Box(slot,"attack-elapsed",3,48,36,3,Edge);
            attackFill=Box(attackTrack,"attack-elapsed-fill",0,0,0,3,Gold);
            var xpTrack=Box(bottom,"dock-experience-line",18,74,294,5,Ink);Border(xpTrack,Edge,1);
            dockXpFill=Box(xpTrack,"dock-experience-fill",1,1,0,3,Purple);
            combatStats=Text(bottom,"",316,16,34,40,7,Cream);combatStats.style.whiteSpace=WhiteSpace.Normal;
            currency=Text(bottom,"",252,56,58,16,8,Gold);currency.name="gold-value";currency.style.unityTextAlign=TextAnchor.MiddleRight;
            saveStatus=Text(bottom,"",252,98,98,12,6,Muted);saveStatus.name="save-status";
            saveStatus.style.overflow=Overflow.Hidden;saveStatus.style.textOverflow=TextOverflow.Ellipsis;
            saveRetry=Click(bottom,"save-retry","저장 재시도",252,78,98,30,()=>encounter.RetrySave(),Crimson);
            dungeonTab=Click(bottom,"dungeon-tab","사냥",18,88,40,26,()=>encounter.ShowManagement(ManagementScreen.None),Burgundy);
            equipmentTab=Click(bottom,"character-tab","가방",61,88,38,26,()=>ToggleManagement(ManagementScreen.Equipment),Surface);
            talentTab=Click(bottom,"talents-tab","특성",102,88,38,26,()=>ToggleManagement(ManagementScreen.Talents),Surface);
            forgeTab=Click(bottom,"forge-tab","강화",143,88,42,26,()=>ToggleManagement(ManagementScreen.Forge),Surface);
            effectsButton=Click(bottom,"effects-toggle","효과",188,88,28,26,()=>encounter.Hunt.ToggleReducedEffects(),Surface);
            huntButton=Click(bottom,"autohunt-toggle","시작",219,88,48,26,()=> {
                if(encounter.Hunt.Running)encounter.Hunt.StopHunt();else encounter.Hunt.StartHunt();
            },Crimson);
            huntCaption=huntButton.Q<Label>();
            pauseButton=Click(bottom,"pause-button","정지",270,88,45,26,ToggleExplicitPause,Surface);
            pauseCaption=pauseButton.Q<Label>();
            speedValue=Text(bottom,"1x",317,58,33,16,10,Gold);speedValue.name="simulation-speed-value";speedValue.style.unityTextAlign=TextAnchor.MiddleCenter;
            speedButtons[0]=Click(bottom,"speed-1x","1x",318,88,10,26,()=>encounter.SetSimulationSpeed(1),Surface);
            speedButtons[1]=Click(bottom,"speed-2x","2x",329,88,10,26,()=>encounter.SetSimulationSpeed(2),Surface);
            speedButtons[2]=Click(bottom,"speed-4x","4x",340,88,10,26,()=>encounter.SetSimulationSpeed(4),Surface);
            foreach(var button in new[]{dungeonTab,equipmentTab,talentTab,forgeTab,effectsButton,huntButton,pauseButton,speedButtons[0],speedButtons[1],speedButtons[2]})
                button.Q<Label>().style.fontSize=button.name.StartsWith("speed-")?6:7;
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
            equipmentPanel=new EquipmentPanel(root,ReadEquipment,SelectItem,SelectEquippedItem,
                SelectSocket,CycleRune,InsertRune,RemoveRune,
                ()=>encounter.EquipItem(selectedItemIndex),()=>encounter.SalvageItem(selectedItemIndex),
                ()=>encounter.SpendTalent(TalentId.Fury),()=>encounter.SpendTalent(TalentId.Precision),()=>encounter.SpendTalent(TalentId.Keystone),
                ()=>encounter.ResetTalents(),()=>encounter.SetEquipmentVisible(false),portrait);
            talentPanel=new TalentPanel(root,()=> {
                var p=encounter.Progression;
                return new TalentPanel.View {Points=p.UnspentPoints,Spent=p.SpentPoints,Fury=p.FuryRank,
                    Precision=p.PrecisionRank,Keystone=p.KeystoneRank,Vitality=p.VitalityRank,Cleave=p.CleaveRank,Haste=p.HasteRank,
                    TotalDamage=p.TotalDamage,TotalHealth=p.TotalMaxHp,CriticalChance=p.CriticalChance,Penetration=p.Penetration,
                    AreaTargets=p.AreaSkillMinimumTargets,RecoveryThreshold=p.RecoveryThresholdPercent,RecoveryHeal=p.RecoveryHealPercent,
                    ActiveEvolutions=p.ActiveEvolutionCount,AreaRadius=p.AreaSkillRadius,AreaDamageMultiplier=p.AreaSkillDamageMultiplier,
                    AreaArming=p.AreaSkillArmingDelay,RecoveryArming=p.RecoveryArmingDelay,
                     AreaName=KoreanDisplay.Skill(p.AreaSkillName),AreaTradeoff=AreaTradeoff(p),RecoveryName=KoreanDisplay.Skill(p.RecoverySkillName)};
            },id=>encounter.SpendTalent(id),()=>encounter.ResetTalents(),()=>encounter.ShowManagement(ManagementScreen.None));
            forgePanel=new ForgePanel(root,()=> {
                var p=encounter.Progression;var item=p.EquippedWeapon;
                return new ForgePanel.View {Name=KoreanDisplay.ItemName(item),Icon=item.IconResource,Rank=item.EnhancementRank,
                    WeaponDamage=item.DamageBonus,TotalDamage=p.TotalDamage,Gold=p.TotalGold,Cost=p.EquippedEnhancementCost,
                    CanEnhance=p.CanEnhanceEquipped,Affix=KoreanDisplay.Rarity(item.Rarity)+" · "+KoreanDisplay.Slot(item.EquipmentSlot)+
                        "\n"+PrimarySummary(item)+" · 옵션 "+item.Options.Count+"개",
                    Notice=encounter.ProgressionNotice,HasItem=true,Slot=EquipmentSlot.Weapon,
                    PrimaryLabel="무기 공격력",PrimaryValue=item.DamageBonus.ToString(),
                    NextPrimaryValue=(item.DamageBonus+item.NextEnhancementDamage).ToString(),
                    SecondaryLabel="총 공격력",SecondaryValue=p.TotalDamage.ToString(),
                    NextSecondaryValue=(p.TotalDamage+item.NextEnhancementDamage).ToString()};
            },()=>encounter.EnhanceWeapon(),()=>encounter.ShowManagement(ManagementScreen.Equipment),()=>encounter.ShowManagement(ManagementScreen.None));
        }
        private void SelectItem(int index)
        {
            selectedItemIndex=index;selectedEquippedSlot=null;selectedSocketIndex=0;selectedRuneIndex=0;
        }
        private void SelectEquippedItem(EquipmentSlot slot)
        {
            selectedEquippedSlot=slot;selectedItemIndex=-1;selectedSocketIndex=0;selectedRuneIndex=0;
        }
        private void SelectSocket(int index){selectedSocketIndex=index;}
        private void CycleRune(int direction)
        {
            WeaponItem item=SelectedEquipmentItem();
            if(item==null)return;
            int count=encounter.Progression.CompatibleOwnedRunes(item.EquipmentSlot).Count;
            if(count<=0){selectedRuneIndex=0;return;}
            selectedRuneIndex=(selectedRuneIndex+direction)%count;
            if(selectedRuneIndex<0)selectedRuneIndex+=count;
        }
        private WeaponItem SelectedEquipmentItem()
        {
            HeroProgression p=encounter==null?null:encounter.Progression;
            if(p==null)return null;
            if(selectedEquippedSlot.HasValue)return p.GetEquipped(selectedEquippedSlot.Value);
            return selectedItemIndex>=0&&selectedItemIndex<p.Inventory.Count?p.Inventory[selectedItemIndex]:null;
        }
        private void InsertRune()
        {
            WeaponItem item=SelectedEquipmentItem();if(item==null)return;
            IReadOnlyList<RuneDefinition> compatible=encounter.Progression.CompatibleOwnedRunes(item.EquipmentSlot);
            if(compatible.Count==0)return;
            selectedRuneIndex=Mathf.Clamp(selectedRuneIndex,0,compatible.Count-1);
            encounter.SocketRune(selectedItemIndex,selectedEquippedSlot,selectedSocketIndex,compatible[selectedRuneIndex].Id);
        }
        private void RemoveRune()
        {
            if(SelectedEquipmentItem()==null)return;
            encounter.UnsocketRune(selectedItemIndex,selectedEquippedSlot,selectedSocketIndex);
        }
        private EquipmentPanel.View ReadEquipment()
        {
            HeroProgression p=encounter.Progression;
            if(p==null || encounter.Hero==null)return null;
            selectedItemIndex=p.Inventory.Count==0?-1:Mathf.Clamp(selectedItemIndex,-1,p.Inventory.Count-1);
            if(selectedEquippedSlot.HasValue&&p.GetEquipped(selectedEquippedSlot.Value)==null)selectedEquippedSlot=null;
            WeaponItem item=selectedEquippedSlot.HasValue?p.GetEquipped(selectedEquippedSlot.Value):
                selectedItemIndex>=0?p.Inventory[selectedItemIndex]:null;
            var items=new EquipmentPanel.ItemView[p.Inventory.Count];
            for(int i=0;i<items.Length;i++)items[i]=ItemView(p.Inventory[i],false);
            bool equippedSelection=selectedEquippedSlot.HasValue;
            WeaponItem current=item==null?null:equippedSelection?item:p.GetEquipped(item.EquipmentSlot);
            int damageDelta=item==null?0:item.DamageBonus-(current?.DamageBonus??0);
            int defenseDelta=item==null?0:item.DefenseBonus-(current?.DefenseBonus??0);
            int healthDelta=item==null?0:item.HealthBonus-(current?.HealthBonus??0);
            float speedDelta=item==null?0:item.SpeedBonus-(current?.SpeedBonus??0);
            IReadOnlyList<RuneDefinition> compatible=item==null?Array.Empty<RuneDefinition>():p.CompatibleOwnedRunes(item.EquipmentSlot);
            selectedRuneIndex=compatible.Count==0?0:Mathf.Clamp(selectedRuneIndex,0,compatible.Count-1);
            selectedSocketIndex=item==null||item.SocketCapacity==0?0:Mathf.Clamp(selectedSocketIndex,0,item.SocketCapacity-1);
            var runeViews=new EquipmentPanel.RuneView[compatible.Count];
            for(int i=0;i<runeViews.Length;i++)runeViews[i]=RuneView(compatible[i],p.RuneCount(compatible[i].Id));
            RuneDefinition chosen=compatible.Count==0?null:compatible[selectedRuneIndex];
            bool socketOpen=item!=null&&selectedSocketIndex<item.OpenedSocketCount;
            string socketRune=socketOpen?item.SocketedRunes[selectedSocketIndex]:null;
            return new EquipmentPanel.View {
                Items=items,EquippedSlots=new[]{ItemView(p.EquippedWeapon,true),ItemView(p.EquippedHelmet,true),ItemView(p.EquippedArmor,true),ItemView(p.EquippedGloves,true),ItemView(p.EquippedBoots,true),ItemView(p.EquippedRing,true),ItemView(p.EquippedAmulet,true),ItemView(p.EquippedRelic,true)},
                Selected=ItemView(item,equippedSelection),SelectedEquipped=equippedSelection,
                CompatibleRunes=runeViews,SelectedRune=chosen==null?null:RuneView(chosen,p.RuneCount(chosen.Id)),SelectedSocketIndex=selectedSocketIndex,
                Health=encounter.Hero.Hp+" / "+encounter.Hero.MaxHp,
                Damage=p.TotalDamage.ToString(),Defense=encounter.Hero.Defense.ToString(),
                Duration=encounter.Hero.AnimationSet==null?"—":encounter.Hero.AnimationSet.AttackDuration.ToString("0.00")+"초",
                Comparison=item==null?"가방에서 장비를 선택하세요.":"현재 슬롯 비교 · 피해 "+Signed(damageDelta)+"  방어 "+Signed(defenseDelta)+"  체력 "+Signed(healthDelta),
                Delta=item==null?"":"공격 속도 "+(speedDelta>=0?"+":"")+(speedDelta*100f).ToString("0")+"% · "+KoreanDisplay.Slot(item.EquipmentSlot),
                Affix=item==null?"":DescribeItemDetailed(item),
                Status=encounter.ProgressionNotice,Points=p.UnspentPoints,Fury=p.FuryRank,Precision=p.PrecisionRank,Keystone=p.KeystoneRank,
                SalvageValue=equippedSelection?0:p.GetSalvageValue(selectedItemIndex),
                SalvageRuneReturnCount=equippedSelection||item==null?0:item.FilledSocketCount,
                CanEquip=item!=null&&!equippedSelection,CanSalvage=item!=null&&!equippedSelection,
                CanInsertRune=socketOpen&&string.IsNullOrEmpty(socketRune)&&chosen!=null,
                CanRemoveRune=socketOpen&&!string.IsNullOrEmpty(socketRune)&&p.RuneCount(socketRune)<SocketCatalog.MaxRuneStack,
                CanReset=p.SpentPoints>0,
                CanFury=p.UnspentPoints>0&&p.FuryRank<2,
                CanPrecision=p.UnspentPoints>0&&p.FuryRank>=2&&p.PrecisionRank<1,
                CanKeystone=p.UnspentPoints>0&&p.PrecisionRank>=1&&p.KeystoneRank<1
            };
        }
        private static EquipmentPanel.RuneView RuneView(RuneDefinition rune,int count)=>rune==null?null:new EquipmentPanel.RuneView
        {Id=rune.Id,Name=KoreanDisplay.RuneName(rune.Id),Mark=rune.Mark,Effect=KoreanDisplay.RuneEffect(rune),Count=count};
        private static EquipmentPanel.ItemView ItemView(WeaponItem item,bool equipped)
        {
            return item==null?null:new EquipmentPanel.ItemView {Id=item.Id,Name=KoreanDisplay.ItemName(item),Icon=item.IconResource,
                SlotText=KoreanDisplay.Slot(item.EquipmentSlot),Summary=KoreanDisplay.Rarity(item.Rarity)+" · "+PrimarySummary(item),
                PrimaryValue=PrimaryValue(item),PrimaryLabel=PrimaryLabel(item),Rarity=item.Rarity,
                Affix=DescribeItemDetailed(item),Damage=item.DamageBonus,Defense=item.DefenseBonus,Health=item.HealthBonus,Speed=item.SpeedBonus,
                SocketCapacity=item.SocketCapacity,OpenedSocketCount=item.OpenedSocketCount,
                SocketRuneIds=new List<string>(item.SocketedRunes).ToArray(),Equipped=equipped};
        }
        private static string Signed(int value)=>value>=0?"+"+value:value.ToString();
        private static string PrimaryValue(WeaponItem item)=>item.DamageBonus>0?item.DamageBonus.ToString():item.DefenseBonus>0?item.DefenseBonus.ToString():item.HealthBonus.ToString();
        private static string PrimaryLabel(WeaponItem item)=>item.DamageBonus>0?"피해":item.DefenseBonus>0?"방어":"체력";
        private static string PrimarySummary(WeaponItem item)=>PrimaryLabel(item)+" +"+PrimaryValue(item);
        private static string AreaTradeoff(HeroProgression progression)
        {
            if(progression.AreaTrajectory==AreaSkillTrajectory.Chain)return "연쇄 "+progression.AreaSkillMaxTargets+"명 · 도약 "+progression.AreaSkillJumpRange.ToString("0.0")+"m";
            if(progression.AreaTrajectory==AreaSkillTrajectory.Pierce)return "관통 "+progression.AreaSkillMaxTargets+"명 · 폭 "+progression.AreaSkillPierceWidth.ToString("0.00")+"m";
            if(progression.AreaTrajectory==AreaSkillTrajectory.Quake)return "2단계 · 외곽 65% · 반경 "+progression.AreaSkillRadius.ToString("0.0")+"m";
            return "전방위 · 반경 "+progression.AreaSkillRadius.ToString("0.0")+"m";
        }
        private static string RollRange(AffixStat stat)
        {
            switch(stat)
            {
                case AffixStat.Attack:return "층당 1~15";
                case AffixStat.Defense:return "층당 1~10";
                case AffixStat.Health:return "층당 5~60";
                case AffixStat.Mana:return "층당 5~40";
                case AffixStat.Speed:return "층당 5~30%";
                case AffixStat.Critical:return "층당 1~15";
                case AffixStat.Vampirism:return "층당 1~8";
                case AffixStat.Experience:return "층당 3~20";
                case AffixStat.Gold:return "층당 5~30";
                case AffixStat.Penetration:return "층당 1~10";
                default:return "설정 범위";
            }
        }
        private static string DescribeItem(WeaponItem item)
        {
            var lines=new List<string>{KoreanDisplay.Rarity(item.Rarity)+" · "+KoreanDisplay.Slot(item.EquipmentSlot)};
            if(item.DamageBonus!=0)lines.Add("공격 +"+item.DamageBonus);
            if(item.DefenseBonus!=0)lines.Add("방어 +"+item.DefenseBonus);
            if(item.HealthBonus!=0)lines.Add("체력 +"+item.HealthBonus);
            foreach(ItemOption option in item.Options)lines.Add(KoreanDisplay.Stat(option.Stat)+" +"+(option.Stat==AffixStat.Speed?(option.Value*100f).ToString("0")+"%":option.Value.ToString("0.#")));
            if(item.SocketCapacity>0)lines.Add("소켓 "+item.FilledSocketCount+"/"+item.OpenedSocketCount+" · 최대 "+item.SocketCapacity);
            if(item.EnhancementRank>0)lines.Add("강화 +"+item.EnhancementRank);
            return string.Join("  ·  ",lines);
        }
        private static string DescribeItemDetailed(WeaponItem item)
        {
            var lines=new List<string>{KoreanDisplay.Rarity(item.Rarity)+"  /  "+KoreanDisplay.Slot(item.EquipmentSlot)};
            if(item.FlatDamage!=0)lines.Add("기본 공격력  +"+item.FlatDamage);
            if(item.AffixDamage!=0)lines.Add("추가 공격력  +"+item.AffixDamage);
            if(item.FlatDefense!=0)lines.Add("기본 방어력  +"+item.FlatDefense);
            if(item.FlatHealth!=0)lines.Add("기본 체력  +"+item.FlatHealth);
            foreach(ItemOption option in item.Options)lines.Add(KoreanDisplay.Stat(option.Stat)+"  +"+
                (option.Stat==AffixStat.Speed?(option.Value*100f).ToString("0")+"%":option.Value.ToString("0.#"))+"  ["+RollRange(option.Stat)+"]");
            lines.Add("소켓  "+item.FilledSocketCount+" / "+item.OpenedSocketCount+"  ·  최대 "+item.SocketCapacity);
            for(int i=0;i<item.OpenedSocketCount;i++)
            {
                RuneDefinition rune=SocketCatalog.GetRune(item.SocketedRunes[i]);
                lines.Add(rune==null?"룬 "+(i+1)+"  ·  열림 / 비어 있음":
                    "룬 "+(i+1)+"  ·  "+KoreanDisplay.RuneName(rune.Id)+"  "+KoreanDisplay.RuneEffect(rune));
            }
            if(item.EnhancementRank>0)lines.Add("강화  +"+item.EnhancementRank);
            return string.Join("\n",lines);
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
        private void ToggleExplicitPause()=>encounter.SetPaused(!encounter.IsPaused);
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
            var hero=encounter.Hero;
            var hunt=encounter.Hunt;
            if(hunt!=null)
            {
                foreach(var actor in hunt.Enemies)ObserveActor(actor);
                huntButton.SetEnabled(hunt.Initialized);
                huntCaption.text=hunt.Running?"중지":"시작";
                huntStatus.text=hunt.StateCaption+"\n생존 "+hunt.AliveEnemies+(hunt.KillChain>1?"   연속 처치 ×"+hunt.KillChain:"");
                sectionCaption.text=KoreanDisplay.Layout(hunt.World.LayoutId)+"\n"+KoreanDisplay.Difficulty(hunt.CurrentDifficulty)+"  ·  정화 "+encounter.Progression.DungeonClears;
                for(int i=0;i<sectionTicks.Length;i++)
                {
                    sectionTicks[i].style.display=DisplayStyle.Flex;
                    sectionTicks[i].style.backgroundColor=(int)hunt.World.LayoutId==i?Crimson:Edge;
                }
                killCount.text=hunt.TotalKills+"  처치";
                huntRewards.text="수거 "+hunt.CollectedItems+"개  ·  재도전 "+hunt.DeathRetries+"회";
                int alive=0;foreach(var actor in hunt.Enemies)if(actor.isActiveAndEnabled&&!actor.IsDead)alive++;
                mapTitle.text="지도  /  "+KoreanDisplay.VisualIdentity(hunt.World.LayoutId);
                mapTitle.tooltip=KoreanDisplay.Layout(hunt.World.LayoutId)+"\n고유 지형 장식";
                mapCaption.text=KoreanDisplay.Difficulty(hunt.CurrentDifficulty)+"  ·  생존 "+alive;
                DifficultyRule rule=hunt.Difficulty;
                difficultyInfo.text="체력×"+rule.EnemyHealthMultiplier.ToString("0.00")+" 피해×"+rule.EnemyDamageMultiplier.ToString("0.00")+
                    " 획득×"+rule.DropMultiplier.ToString("0.00");
                difficultyStatus.text=KoreanDisplay.Difficulty(hunt.CurrentDifficulty);
                for(int i=0;i<difficultyButtons.Length;i++)
                {
                    difficultyButtons[i].SetEnabled(hunt.CanChangeDifficulty&&(int)hunt.CurrentDifficulty!=i);
                    difficultyButtons[i].style.backgroundColor=(int)hunt.CurrentDifficulty==i?Crimson:Surface;
                }
                effectsButton.Q<Label>().text=hunt.ReducedEffects?"효과↓":"효과";
                RefreshObstacleMarkers(hunt.World);
                bool showBoss=hunt.BossAlive;
                bossFrame.style.display=showBoss?DisplayStyle.Flex:DisplayStyle.None;
                if(showBoss)
                {
                    MeleeActor boss=hunt.Boss;
                    bossName.text=KoreanDisplay.Boss(hunt.World.LayoutId);
                    bossHealthFill.style.width=Length.Percent(100f*boss.Hp/Mathf.Max(1,boss.MaxHp));
                    bossHealthValue.text=boss.Hp+" / "+boss.MaxHp;
                    bossPattern.text=hunt.BossTelegraphing?"파멸의 파동  ·  회피 중":hunt.BossEngaged?"구역 우두머리  ·  자동 교전":"봉인됨  ·  수호자 23명 처치";
                    bossPattern.style.color=hunt.BossTelegraphing?Color.white:Muted;
                    Color bossEdge=hunt.BossTelegraphing?Crimson:new Color32(116,64,55,255);
                    bossFrame.style.borderTopColor=bossEdge;bossFrame.style.borderRightColor=bossEdge;
                    bossFrame.style.borderBottomColor=bossEdge;bossFrame.style.borderLeftColor=bossEdge;
                }
            }
            healthFill.style.width=Length.Percent(100f*hero.Hp/Mathf.Max(1,hero.MaxHp));
            healthValue.text=hero.Hp+" / "+hero.MaxHp;
            int levelXp=encounter.Progression.TotalExperience%HeroProgression.ExperiencePerLevel;
            float xpPercent=100f*levelXp/HeroProgression.ExperiencePerLevel;
            xpFill.style.width=Length.Percent(xpPercent);dockXpFill.style.width=Length.Percent(xpPercent);
            xpValue.text="경험치  "+levelXp+" / "+HeroProgression.ExperiencePerLevel;
            levelValue.text=encounter.Progression.Level.ToString();
            if(hunt!=null)
            {
                areaCooldown.text=hunt.AreaCooldownRemaining<=0?"준비":hunt.AreaCooldownRemaining.ToString("0.0")+"초";
                recoveryCooldown.text=hunt.RecoveryCooldownRemaining<=0?"준비":hunt.RecoveryCooldownRemaining.ToString("0.0")+"초";
                string areaName=KoreanDisplay.Skill(hunt.AreaEvolutionName);
                areaSkillLabel.text=areaName;
                areaSkillLabel.style.fontSize=areaName.Length>7?5:areaName.Length>5?6:8;
                areaSkillSlot.tooltip=areaName+"  공격력의 "+Mathf.RoundToInt(hunt.AreaSkillDamageMultiplier*100)+"%\n"+
                    AreaTradeoff(encounter.Progression)+"\n무기별 경로: 검 연쇄 참격 / 도끼 충격파 / 지팡이 관통 창";
                string recoveryName=KoreanDisplay.Skill(hunt.RecoveryEvolutionName);
                recoverySkillLabel.text=recoveryName;
                recoverySkillLabel.style.fontSize=recoveryName.Length>7?5:8;
                recoverySkillSlot.tooltip=recoveryName+"  체력 "+hunt.RecoveryThresholdPercent+
                    "% 이하 / "+hunt.RecoveryHealPercent+"% 회복";
            }
            currency.text="골드  "+encounter.Progression.TotalGold;
            combatStats.text="공"+encounter.Progression.TotalDamage+"\n방"+hero.Defense+
                "\n가"+encounter.Progression.Inventory.Count+"/24\n특"+encounter.Progression.UnspentPoints;
            int seconds=Mathf.FloorToInt(encounter.ElapsedSeconds);
            clock.text="사냥 "+(seconds/60).ToString("00")+":"+(seconds%60).ToString("00");
            int wallSeconds=Mathf.FloorToInt(encounter.WallClockSeconds);
            clock.tooltip="게임 시간 "+(seconds/60).ToString("00")+":"+(seconds%60).ToString("00")+
                " / 실제 시간 "+(wallSeconds/60).ToString("00")+":"+(wallSeconds%60).ToString("00");
            speedValue.text=encounter.SimulationSpeed.ToString("0")+"x";
            for(int i=0;i<speedButtons.Length;i++)
                speedButtons[i].style.backgroundColor=Mathf.Approximately(encounter.SimulationSpeed,1<<i)?Crimson:Surface;
            pauseCaption.text=encounter.IsPaused?"계속":"정지";
            paused.style.display=encounter.IsPaused&&!encounter.ManagementVisible&&!encounter.HasEnded?DisplayStyle.Flex:DisplayStyle.None;
            attackState.text=encounter.HasEnded?"종료":encounter.IsPaused?"정지":hero.IsAttacking?"공격":hero.CurrentClip==ActorClip.Hit?"피격":hero.CurrentClip==ActorClip.Walk?"이동":"자동";
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
            lootText.text=loot==null?encounter.ProgressionNotice:"전리품  ·  "+KoreanDisplay.ItemName(loot);
            pickupLoot.SetEnabled(loot!=null && !hero.IsDead);pickupLoot.style.opacity=loot==null?0.4f:1;
            nextEncounter.SetEnabled(hero.IsDead || loot==null);nextEncounter.style.opacity=hero.IsDead||loot==null?1:0.4f;
            nextEncounter.Q<Label>().text=hero.IsDead?"다시 시작 [R]":"다음 전투";
            resultText.text=hero.IsDead?"영웅 쓰러짐":"전투 완료  경험치 +"+encounter.Experience+"  골드 +"+encounter.Gold;
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
            string prefix=actor.LastHitKilled?"처치  ":actor.LastHitCritical?"치명  ":"";
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
        private static void Skin(VisualElement element,string resource)
        {
            Texture2D texture=Resources.Load<Texture2D>(resource);
            if(texture!=null)element.style.backgroundImage=new StyleBackground(texture);
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
