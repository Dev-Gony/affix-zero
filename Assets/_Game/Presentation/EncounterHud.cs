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
        private FirstEncounter encounter;
        private UIDocument document;
        private PanelSettings panelSettings;
        private VisualElement root, enemyFill, healthFill, attackFill, mapArea, mapHero, mapEnemy, result, pickupLoot, nextEncounter;
        private Label enemyValue, healthValue, manaValue, areaCooldown, recoveryCooldown, currency, clock, attackState, xpValue, resultText, paused, lootText;
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
        private Label sectionCaption, killCount, huntRewards, combatStats, mapCaption;
        private readonly VisualElement[] sectionTicks=new VisualElement[3];
        private Label saveStatus;
        private VisualElement saveRetry;
        private readonly HashSet<MeleeActor> observedActors=new HashSet<MeleeActor>();
        private int selectedItemIndex = -1;
        private Image actionIcon;
        private string actionIconResource;
        private readonly List<DamageLabel> damageLabels = new List<DamageLabel>();
        private readonly List<VisualElement> enemyMarkers = new List<VisualElement>();
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
            var top = Box(root, "top-navigation", 0, 0, 0, 56, Surface);
            top.style.right = 0; top.style.width = StyleKeyword.Auto;
            Box(top,"location-mark",18,19,10,17,Crimson);
            Text(top, "AFFIX : ZERO", 38, 8, 194, 25, 19, Cream);
            Text(top, "사원 마당  /  TEMPLE", 39, 33, 198, 17, 10, Gold);
            dungeonTab = Click(top, "dungeon-tab", "던전 사냥", 254, 11, 102, 34, () => encounter.ShowManagement(ManagementScreen.None), Crimson);
            equipmentTab = Click(top, "character-tab", "인벤토리 [I]", 364, 11, 116, 34, () => ToggleManagement(ManagementScreen.Equipment), Surface);
            talentTab = Click(top, "talents-tab", "특성 [K]", 488, 11, 92, 34, () => ToggleManagement(ManagementScreen.Talents), Surface);
            forgeTab = Click(top, "forge-tab", "대장간 [F]", 588, 11, 110, 34, () => ToggleManagement(ManagementScreen.Forge), Surface);
            huntButton=Click(top,"autohunt-toggle","자동사냥 시작",728,11,134,34,()=> {
                if(encounter.Hunt.Running)encounter.Hunt.StopHunt();else encounter.Hunt.StartHunt();
            },Crimson);
            huntCaption=huntButton.Q<Label>();
            var wallet=Box(top,"currency-inset",0,12,240,31,Ink);
            wallet.style.left=StyleKeyword.Auto;wallet.style.right=142;
            currency = Text(wallet, "", 10, 5, 220, 21, 12, Gold);
            currency.name = "gold-value";
            currency.style.unityTextAlign=TextAnchor.MiddleCenter;
            pauseButton = Click(top, "pause-button", "일시정지", 0, 12, 106, 31, TogglePause, Ink);
            pauseButton.style.left = StyleKeyword.Auto; pauseButton.style.right = 18;
            pauseCaption = pauseButton.Q<Label>();
        }
        private void BuildEnemy()
        {
            var ribbon=Box(root,"dungeon-progress",0,64,650,28,Edge);
            ribbon.style.left=Length.Percent(50);ribbon.style.marginLeft=-325;
            sectionCaption=Text(ribbon,"",12,3,580,23,13,Gold);
            for(int i=0;i<sectionTicks.Length;i++)sectionTicks[i]=Box(ribbon,"section-tick-"+i,580+i*20,9,14,10,Ink);
            var enemy = Box(root, "enemy-health", 0, 98, 650,67, Surface);
            enemy.style.left = Length.Percent(50); enemy.style.marginLeft = -325;
            Box(enemy,"target-accent",0,0,3,67,Crimson);
            enemyTitle=Text(enemy, "사원 경비병", 12, 6, 375, 23, 15, Cream);
            enemyValue = Text(enemy, "", 402, 9, 236, 19, 11, new Color32(255,179,180,255));
            enemyValue.name = "enemy-hp-value";
            enemyValue.style.unityTextAlign = TextAnchor.MiddleRight;
            var track = Box(enemy, "enemy-track", 12, 33, 626, 12, Ink);
            enemyFill = Box(track, "enemy-fill", 0, 0, 626, 12, Crimson);
            Text(enemy,"근접 전투",12,48,180,16,10,Muted);
            Text(enemy,"자동 탐색 · 자동 수거",432,48,206,16,10,Sky).style.unityTextAlign=TextAnchor.MiddleRight;
            var stats=Box(root,"hunt-summary",16,186,236,102,Surface);
            killCount=Text(stats,"",14,5,208,35,25,Gold);killCount.name="hunt-kills";
            huntRewards=Text(stats,"",14,43,208,20,11,Cream);
            clock = Text(stats, "", 14,71,208,18,10,Muted);
            Box(stats,"hunt-summary-rule",0,99,236,3,Gold);
            huntStatus=Text(root,"",18,298,236,55,12,Gold);huntStatus.name="autohunt-status";huntStatus.style.whiteSpace=WhiteSpace.Normal;
            paused = Text(root, "일시정지  /  PAUSED", 0, 172, 210, 24, 12, Gold);
            paused.style.left = Length.Percent(50); paused.style.marginLeft = -105;
            paused.style.unityTextAlign = TextAnchor.MiddleCenter;
        }
        private void BuildMap()
        {
            var frame = Box(root, "minimap", 0, 186, 172, 168, Surface);
            frame.style.left = StyleKeyword.Auto; frame.style.right = 16;
            Text(frame, "MAP  /  사원 마당", 8, 7, 157, 18, 10, Cream);
            var map = Box(frame, "minimap-image", 8, 31, 156, 104, Ink);mapArea=map;
            if (room != null) map.style.backgroundImage = new StyleBackground(room);
            else Text(map,"ROOM ART MISSING",4,40,150,20,10,Crimson);
            mapHero = Box(map,"hero-map-marker",0,0,5,5,Sky);
            mapEnemy = Box(map,"enemy-map-marker",0,0,5,5,Crimson);
            enemyMarkers.Add(mapEnemy);
            Border(mapHero,Color.white,1); Border(mapEnemy,Gold,1);
            mapCaption=Text(frame,"",8,142,156,18,10,Sky);
        }
        private void BuildBottom()
        {
            var bottom = Box(root,"bottom-hud",0,0,0,130,Surface);
            bottom.style.top = StyleKeyword.Auto; bottom.style.bottom = 0; bottom.style.right = 0; bottom.style.width = StyleKeyword.Auto;
            healthFill = Orb(bottom,"health-orb",38,6,84,Crimson,out healthValue);
            healthValue.name = "hero-hp-value";
            combatStats=Text(bottom,"",148,18,255,45,12,Cream);combatStats.style.whiteSpace=WhiteSpace.Normal;
            saveStatus=Text(bottom,"",148,68,255,32,10,Muted);saveStatus.name="save-status";
            saveStatus.style.whiteSpace=WhiteSpace.Normal;
            saveRetry=Click(bottom,"save-retry","저장 다시 시도",915,20,170,30,()=>encounter.RetrySave(),Crimson);
            Text(bottom,"생명력 / HP",27,91,106,16,10,Cream).style.unityTextAlign=TextAnchor.MiddleCenter;
            var mana = Orb(bottom,"mana-orb",0,6,84,new Color32(0,81,129,255),out manaValue);
            var manaShell=mana.parent;
            manaShell.style.left=StyleKeyword.Auto;manaShell.style.right=38;
            mana.style.height=Length.Percent(100);manaShell.tooltip="자동 스킬 자원 · 현재 빌드의 최대 마나";
            var manaLabel=Text(bottom,"마나 / MP",0,91,106,16,10,Muted);
            manaLabel.style.left=StyleKeyword.Auto;manaLabel.style.right=27;manaLabel.style.unityTextAlign=TextAnchor.MiddleCenter;
            var actions=Box(bottom,"action-dock",0,11,440,84,Ink);
            actions.style.left=Length.Percent(50);actions.style.marginLeft=-220;
            var slot=Box(actions,"attack-slot",8,5,56,56,Surface);
            Border(slot,Crimson,2);
            if(attackIcon!=null) {
                var icon=new Image {image=attackIcon,scaleMode=ScaleMode.ScaleToFit,pickingMode=PickingMode.Ignore};
                Place(icon,11,8,32,32);slot.Add(icon);actionIcon=icon;
            }
            Text(slot,"AUTO",8,39,42,14,9,Gold).style.unityTextAlign=TextAnchor.MiddleCenter;
            var area=Box(actions,"auto-area-skill",70,5,54,56,Surface);Border(area,Gold,1);
            Text(area,"회전",0,5,54,20,11,Gold).style.unityTextAlign=TextAnchor.MiddleCenter;
            areaCooldown=Text(area,"",0,26,54,22,12,Cream);areaCooldown.style.unityTextAlign=TextAnchor.MiddleCenter;
            area.tooltip="적 2명 이상 접근 시 자동 범위 공격";
            var recovery=Box(actions,"auto-recovery-skill",130,5,54,56,Surface);Border(recovery,Sky,1);
            Text(recovery,"회복",0,5,54,20,11,Sky).style.unityTextAlign=TextAnchor.MiddleCenter;
            recoveryCooldown=Text(recovery,"",0,26,54,22,12,Cream);recoveryCooldown.style.unityTextAlign=TextAnchor.MiddleCenter;
            recovery.tooltip="체력 45% 이하에서 자동 회복";
            for(int i=0;i<4;i++)
            {
                var empty=Box(actions,"unassigned-skill-"+i,190+i*60,5,54,56,Surface);
                Border(empty,Edge,1);Text(empty,"—",0,13,54,29,17,Muted).style.unityTextAlign=TextAnchor.MiddleCenter;
                empty.tooltip="확장 가능한 자동 스킬 슬롯";
            }
            Text(actions,"기본 공격",10,66,92,17,10,Cream);
            attackState=Text(actions,"",108,66,122,17,10,Gold);
            Text(actions,"AUTO ATTACK",280,66,150,17,10,Muted).style.unityTextAlign=TextAnchor.MiddleRight;
            var attackTrack=Box(slot,"attack-elapsed",0,51,52,3,Edge);
            attackFill=Box(attackTrack,"attack-elapsed-fill",0,0,0,4,Gold);
            Text(bottom,"I 인벤토리    K 특성    F 대장간    ESC 닫기 / 정지",0,110,465,17,10,Muted).style.left=Length.Percent(50);
            var hints=bottom[bottom.childCount-1];hints.style.marginLeft=-232;hints.style.unityTextAlign=TextAnchor.MiddleCenter;
            var xpTrack=Box(bottom,"experience-line",18,107,0,2,Edge);
            xpTrack.style.right=18;xpTrack.style.width=StyleKeyword.Auto;
            // No level threshold exists yet: show the earned total, not an invented progress percentage.
            xpValue=Text(bottom,"",18,111,350,17,10,Gold);
            xpValue.name="xp-value";
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
            var hero=encounter.Hero;var enemy=encounter.Enemy;
            var hunt=encounter.Hunt;
            if(hunt!=null)
            {
                foreach(var actor in hunt.Enemies)ObserveActor(actor);
                huntButton.SetEnabled(hunt.Initialized);
                huntCaption.text=hunt.Running?"자동사냥 중지":"자동사냥 시작";
                huntStatus.text=hunt.StateCaption+"\n연결형 사원 · 생존 적 "+hunt.AliveEnemies;
                sectionCaption.text="CONNECTED TEMPLE     상시 배치 "+hunt.Enemies.Count+"체  ·  정화 "+hunt.CompletedRuns+"회";
                for(int i=0;i<sectionTicks.Length;i++)sectionTicks[i].style.display=DisplayStyle.None;
                killCount.text=hunt.TotalKills+"  KILLS";
                huntRewards.text="수거 "+hunt.CollectedItems+"개  ·  재도전 "+hunt.DeathRetries+"회";
                int alive=0;foreach(var actor in hunt.Enemies)if(actor.isActiveAndEnabled&&!actor.IsDead)alive++;
                mapCaption.text="전체 던전  ·  생존 적 "+alive;
            }
            enemyFill.style.width=Length.Percent(100f*enemy.Hp/Mathf.Max(1,enemy.MaxHp));
            enemyValue.text=enemy.Hp+" / "+enemy.MaxHp+" HP";
            bool hasTarget=hero.CurrentTarget!=null&&hero.CurrentTarget.isActiveAndEnabled&&!hero.CurrentTarget.IsDead;
            enemyTitle.text=hasTarget?"사원 경비병  /  현재 사냥 대상":"사원 순찰  /  자동 탐색";
            if(!hasTarget){enemyFill.style.width=0;enemyValue.text="다음 목표 탐색 중";}
            healthFill.style.height=Length.Percent(100f*hero.Hp/Mathf.Max(1,hero.MaxHp));
            healthValue.text=hero.Hp+"\n/ "+hero.MaxHp;
            manaValue.text=encounter.Progression.TotalMana+"\nMP";
            if(hunt!=null)
            {
                areaCooldown.text=hunt.AreaCooldownRemaining<=0?"READY":hunt.AreaCooldownRemaining.ToString("0.0")+"s";
                recoveryCooldown.text=hunt.RecoveryCooldownRemaining<=0?"READY":hunt.RecoveryCooldownRemaining.ToString("0.0")+"s";
            }
            currency.text="GOLD  "+encounter.Progression.TotalGold+"     XP  "+encounter.Progression.TotalExperience;
            combatStats.text="공격력  "+encounter.Progression.TotalDamage+"   방어력  "+hero.Defense+"\n가방  "+encounter.Progression.Inventory.Count+" / 24   특성  "+encounter.Progression.UnspentPoints;
            xpValue.text="획득 경험치  "+encounter.Progression.TotalExperience+" XP";
            int seconds=Mathf.FloorToInt(encounter.ElapsedSeconds);
            clock.text="TEMPLE  "+(seconds/60).ToString("00")+":"+(seconds%60).ToString("00");
            pauseCaption.text=encounter.IsPaused?"계속하기":"일시정지";
            paused.style.display=encounter.IsPaused&&!encounter.ManagementVisible&&!encounter.HasEnded?DisplayStyle.Flex:DisplayStyle.None;
            attackState.text=encounter.HasEnded?"전투 종료":encounter.IsPaused?"일시정지":hero.IsAttacking?"공격 중":hero.CurrentClip==ActorClip.Hit?"피격":hero.CurrentClip==ActorClip.Walk?"접근 중":"대기";
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
            marker.style.left=Mathf.InverseLerp(DungeonWorld.Bounds.xMin,DungeonWorld.Bounds.xMax,world.x)*151;
            marker.style.top=(1-Mathf.InverseLerp(DungeonWorld.Bounds.yMin,DungeonWorld.Bounds.yMax,world.y))*99;
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
        private void OnDamaged(MeleeActor actor,HitReceipt receipt)
        {
            if(!receipt.Accepted || root==null || encounter.ManagementVisible) return;
            if(damageLabels.Count==16) {damageLabels[0].label.RemoveFromHierarchy();damageLabels.RemoveAt(0);}
            var label=Text(root,receipt.Damage.ToString(),0,0,90,32,22,actor==encounter.Hero?new Color(1,0.48f,0.36f):Gold);
            label.style.unityFontStyleAndWeight=FontStyle.Bold;label.style.unityTextAlign=TextAnchor.MiddleCenter;
            damageLabels.Add(new DamageLabel {label=label,position=actor.transform.position+Vector3.up*0.8f,time=Time.time});
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
                item.label.style.left=point.x-45;item.label.style.top=point.y-16;item.label.style.opacity=Mathf.Clamp01((0.7f-age)/0.25f);
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
