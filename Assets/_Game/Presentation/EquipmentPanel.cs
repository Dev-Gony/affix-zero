using System;
using System.Collections.Generic;
using AffixZero.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace AffixZero.Presentation
{
    // Right-docked paper-doll inventory. Rules and calculated values remain in the core layer.
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
        private readonly VisualElement[] slotRarity = new VisualElement[24];
        private readonly Image[] slotImages = new Image[24];
        private readonly Label[] slotNames = new Label[24];
        private readonly Label[] slotBadges = new Label[24];
        private readonly Label health, damage, defense, duration, bagCount, selectedName, comparison, delta, affix, points, status, selectedDamage, selectedPrimaryLabel;
        private readonly Label[] equippedNames = new Label[8], equippedSummaries = new Label[8];
        private readonly Image[] equippedImages = new Image[8];
        private readonly Label furyCaption, precisionCaption, keystoneCaption;
        private readonly VisualElement equipButton, salvageButton, furyButton, precisionButton, keystoneButton, resetButton, inspect;
        private readonly Image selectedIcon;
        private readonly Func<View> read;
        private readonly Action<int> select;
        private readonly Action discard;
        private bool discardConfirm;
        private string selectedId, discardItemId;
        private static readonly Color Surface = new Color32(24, 25, 28, 255), Ink = new Color32(9, 11, 14, 255), Edge = new Color32(58, 55, 54, 255);
        private static readonly Color Crimson = new Color32(153, 25, 47, 255), Gold = new Color32(224, 185, 87, 255), Cream = new Color32(238, 226, 195, 255), Muted = new Color32(155, 150, 143, 255), Sky = new Color32(151, 203, 255, 255), Rose = new Color32(236, 105, 122, 255), Purple = new Color32(184, 116, 255, 255);
        private readonly Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        private static readonly string[] EmptyIcons =
        {
            "AffixGenerated/EmberSword", "AffixUIVisual/GearHelmet", "AffixGenerated/GearArmor", "AffixUIVisual/GearGloves",
            "AffixUIVisual/GearBoots", "AffixUIVisual/GearRing", "AffixUIVisual/GearAmulet", "AffixGenerated/GearRelic"
        };

        public EquipmentPanel(VisualElement parent, Func<View> read, Action<int> select, Action equip, Action salvage, Action fury, Action precision, Action keystone, Action reset, Action close, Sprite heroPortrait, Action openTalents = null)
        {
            this.read = read;
            this.select = select;
            discard = salvage;

            Root = Box(parent, "character-panel", 772, 4, 500, 712, Color.clear);
            Root.pickingMode = PickingMode.Position;
            Root.style.overflow = Overflow.Visible;
            Skin(Root, "AffixUIVisual/FramePanelTall");

            var header = Box(Root, "management-header", 16, 14, 468, 34, new Color32(19, 16, 17, 238));
            Border(header, new Color32(110, 74, 47, 255), 1);
            var title = Text(header, "HUNTER'S RELIQUARY", 40, 3, 388, 27, 17, Cream);
            title.style.unityTextAlign = TextAnchor.MiddleCenter;
            Box(header, "title-ornament-left", 10, 14, 52, 2, Crimson);
            Box(header, "title-ornament-right", 406, 14, 52, 2, Crimson);
            Button(header, "close-character", "X", 432, 4, 28, 26, close, new Color32(72, 24, 30, 255));

            var equipment = Box(Root, "equipment-section", 16, 54, 468, 350, new Color32(54, 14, 25, 255));
            Skin(equipment, "AffixUIVisual/LeatherBurgundy");
            Border(equipment, new Color32(116, 68, 53, 255), 1);
            Text(equipment, "EQUIPPED RELICS", 14, 8, 440, 22, 12, Gold).style.unityTextAlign = TextAnchor.MiddleCenter;
            var paperDoll = Box(equipment, "equipped-paper-doll", 100, 34, 268, 252, new Color(0.04f, 0.04f, 0.05f, 0.32f));
            Border(paperDoll, new Color(0.58f, 0.35f, 0.26f, 0.42f), 1);
            if (heroPortrait != null)
            {
                var hero = new Image { image = heroPortrait.texture, scaleMode = ScaleMode.ScaleToFit, pickingMode = PickingMode.Ignore, tintColor = new Color(1, 1, 1, .22f) };
                Place(hero, 74, 46, 120, 158);
                Rect source = heroPortrait.textureRect;
                hero.sourceRect = new Rect(source.x, heroPortrait.texture.height - source.yMax, source.width, source.height);
                paperDoll.Add(hero);
            }

            string[] equippedIds = { "equipped-weapon", "equipped-helmet", "equipped-armor", "equipped-gloves", "equipped-boots", "equipped-ring", "equipped-amulet", "equipped-relic" };
            float[] equippedX = { 14, 199, 199, 121, 199, 368, 368, 368 };
            float[] equippedY = { 124, 42, 124, 212, 212, 212, 42, 124 };
            float[] equippedSize = { 74, 66, 74, 66, 66, 58, 58, 74 };
            for (int i = 0; i < 8; i++)
            {
                float size = equippedSize[i];
                var slot = Box(equipment, equippedIds[i], equippedX[i], equippedY[i], size, size, Ink);
                Skin(slot, i == 0 || i == 2 || i == 7 ? "AffixUIVisual/FrameSlotGold" : "AffixUIVisual/FrameSlotSilver");
                slot.pickingMode = PickingMode.Position;
                equippedImages[i] = Icon(slot, 9, 8, size - 18, size - 18);
                equippedNames[i] = Text(slot, "", 3, size - 17, size - 6, 13, 7, Cream);
                equippedNames[i].style.unityTextAlign = TextAnchor.MiddleCenter;
                equippedNames[i].style.overflow = Overflow.Hidden;
                equippedNames[i].style.textOverflow = TextOverflow.Ellipsis;
                equippedSummaries[i] = Text(slot, "", 0, 0, 1, 1, 1, Muted);
                equippedSummaries[i].style.display = DisplayStyle.None;
            }

            var stats = Box(equipment, "hero-stat-table", 14, 292, 440, 44, new Color32(11, 12, 15, 218));
            health = CompactStat(stats, 5, "HP");
            damage = CompactStat(stats, 117, "DMG"); damage.name = "equipment-damage-value";
            defense = CompactStat(stats, 229, "DEF");
            duration = CompactStat(stats, 341, "ATK");

            var bag = Box(Root, "inventory-grid", 16, 410, 468, 234, new Color32(12, 13, 16, 252));
            Border(bag, new Color32(91, 64, 48, 255), 1);
            bagCount = Text(bag, "", 14, 5, 250, 20, 11, Cream);
            bagCount.name = "bag-count";
            Text(bag, "INVENTORY", 316, 5, 138, 20, 10, Gold).style.unityTextAlign = TextAnchor.MiddleRight;
            for (int i = 0; i < 24; i++)
            {
                int index = i;
                float x = 10 + (i % 6) * 76;
                float y = 29 + (i / 6) * 50;
                var slot = Box(bag, "inventory-slot-" + i, x, y, 70, 45, Ink);
                Skin(slot, "AffixUIVisual/BagCellWide");
                Border(slot, Edge, 1);
                slot.pickingMode = PickingMode.Position;
                slot.focusable = true;
                slotImages[i] = Icon(slot, 4, 4, 37, 37);
                slotNames[i] = Text(slot, "", 42, 4, 23, 20, 7, Cream);
                slotNames[i].style.overflow = Overflow.Hidden;
                slotNames[i].style.textOverflow = TextOverflow.Ellipsis;
                slotBadges[i] = Text(slot, "", 42, 24, 23, 14, 6, Gold);
                slotBadges[i].style.overflow = Overflow.Hidden;
                slotRarity[i] = Box(slot, "rarity-mark-" + i, 65, 5, 3, 35, Color.clear);
                slots[i] = slot;
                slot.RegisterCallback<ClickEvent>(_ => { discardConfirm = false; this.select(index); });
                slot.RegisterCallback<NavigationSubmitEvent>(evt => { discardConfirm = false; this.select(index); evt.StopPropagation(); });
            }

            var footer = Box(Root, "inventory-footer", 16, 650, 468, 36, new Color32(17, 17, 19, 248));
            Border(footer, new Color32(91, 64, 48, 255), 1);
            Button(footer, "equipment-tab-active", "EQUIPMENT", 7, 5, 96, 24, () => { }, new Color32(86, 23, 34, 255));
            if (openTalents != null) Button(footer, "open-full-talents", "MASTERY", 109, 5, 82, 24, openTalents, Surface);
            points = Text(footer, "", 200, 6, 100, 22, 10, Gold); points.name = "talent-points";
            Text(footer, "I / TAB", 377, 6, 78, 22, 9, Muted).style.unityTextAlign = TextAnchor.MiddleRight;

            var talents = Box(Root, "talent-section", 0, 0, 1, 1, Color.clear); talents.style.display = DisplayStyle.None;
            furyButton = HiddenButton(talents, "talent-fury", fury, out furyCaption);
            precisionButton = HiddenButton(talents, "talent-precision", precision, out precisionCaption);
            keystoneButton = HiddenButton(talents, "talent-keystone", keystone, out keystoneCaption);
            resetButton = HiddenButton(talents, "talent-reset", reset, out _);

            inspect = Box(Root, "item-comparison", -306, 60, 300, 424, new Color32(10, 10, 13, 252));
            Skin(inspect, "AffixUIVisual/FrameTooltip");
            inspect.style.overflow = Overflow.Hidden;
            Text(inspect, "LOOT INSPECTION", 24, 18, 244, 24, 13, Gold).style.unityTextAlign = TextAnchor.MiddleCenter;
            var selectedSlot = Box(inspect, "selected-weapon-icon", 22, 54, 66, 66, Ink);
            Skin(selectedSlot, "AffixUIVisual/FrameSlotGold");
            selectedIcon = Icon(selectedSlot, 10, 9, 46, 46);
            selectedName = Text(inspect, "", 98, 55, 170, 32, 16, Rose); selectedName.name = "selected-item-name";
            selectedName.style.whiteSpace = WhiteSpace.Normal;
            delta = Text(inspect, "", 98, 90, 170, 35, 10, Sky); delta.name = "comparison-delta"; delta.style.whiteSpace = WhiteSpace.Normal;
            var primary = Box(inspect, "selected-damage-strip", 22, 132, 246, 62, new Color32(17, 15, 18, 245));
            selectedDamage = Text(primary, "", 10, 4, 68, 30, 25, Cream);
            selectedPrimaryLabel = Text(primary, "POWER", 10, 35, 68, 16, 8, Muted);
            comparison = Text(primary, "", 84, 7, 152, 48, 9, Rose); comparison.style.whiteSpace = WhiteSpace.Normal;
            Text(inspect, "BASE + ROLLED AFFIXES", 23, 205, 244, 18, 9, Gold);
            affix = Text(inspect, "", 23, 227, 244, 112, 10, Purple); affix.style.whiteSpace = WhiteSpace.Normal;
            status = Text(inspect, "", 23, 343, 244, 30, 8, Muted); status.style.whiteSpace = WhiteSpace.Normal;
            status.style.overflow = Overflow.Hidden; status.style.textOverflow = TextOverflow.Ellipsis;
            equipButton = Button(inspect, "equip-button", "EQUIP", 22, 378, 142, 28, equip, new Color32(93, 22, 35, 255));
            salvageButton = Button(inspect, "salvage-button", "SALVAGE", 170, 378, 98, 28, RequestDiscard, Surface);

            Root.style.display = DisplayStyle.None;
        }

        public void Refresh(bool visible)
        {
            Root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            if (!visible) { discardConfirm = false; return; }
            View v = read();
            if (v == null) return;
            string nextId = v.Selected == null ? null : v.Selected.Id;
            if (selectedId != nextId) { discardConfirm = false; selectedId = nextId; }
            salvageButton.Q<Label>().text = discardConfirm ? "CONFIRM +" + v.SalvageValue + " G" : "SALVAGE +" + v.SalvageValue + " G";
            string[] captions = { "WEAPON", "HELMET", "ARMOR", "GLOVES", "BOOTS", "RING", "AMULET", "RELIC" };
            for (int i = 0; i < equippedNames.Length; i++) RefreshEquipped(i, i < v.EquippedSlots.Length ? v.EquippedSlots[i] : null, captions[i]);
            health.text = v.Health; damage.text = v.Damage; defense.text = v.Defense; duration.text = v.Duration;
            bagCount.text = "BAG  " + v.Items.Length + " / 24";
            for (int i = 0; i < 24; i++)
            {
                ItemView item = i < v.Items.Length ? v.Items[i] : null;
                slotImages[i].image = Texture(item == null ? null : item.Icon);
                slotImages[i].style.opacity = item == null ? 0 : 1;
                slotNames[i].text = item == null ? "" : ShortName(item.Name);
                slotBadges[i].text = item == null ? "" : item.Equipped ? "ON" : (item.SlotText ?? "");
                slotRarity[i].style.backgroundColor = item == null ? Color.clear : RarityColor(item.Rarity);
                bool selected = item != null && v.Selected != null && item.Id == v.Selected.Id;
                Border(slots[i], selected ? Rose : item != null ? RarityColor(item.Rarity) : Edge, selected ? 2 : 1);
                slots[i].tooltip = item == null ? "Empty inventory slot" : item.Name + "\n" + item.Affix;
                slots[i].SetEnabled(item != null);
            }
            inspect.style.display = v.Selected == null ? DisplayStyle.None : DisplayStyle.Flex;
            selectedName.text = v.Selected == null ? "" : v.Selected.Name;
            selectedDamage.text = v.Selected == null ? "" : v.Selected.PrimaryValue ?? v.Selected.Damage.ToString();
            selectedPrimaryLabel.text = v.Selected == null ? "POWER" : v.Selected.PrimaryLabel ?? "POWER";
            selectedIcon.image = Texture(v.Selected == null ? null : v.Selected.Icon);
            selectedName.style.color = v.Selected == null ? Muted : RarityColor(v.Selected.Rarity);
            comparison.text = v.Comparison ?? ""; delta.text = v.Delta ?? ""; affix.text = v.Affix ?? ""; status.text = v.Status ?? "";
            status.tooltip = v.Status ?? "";
            points.text = "POINTS  " + v.Points;
            furyCaption.text = "FURY " + v.Fury; precisionCaption.text = "PREC " + v.Precision; keystoneCaption.text = "KEY " + v.Keystone;
            SetAction(equipButton, v.CanEquip); SetAction(salvageButton, v.CanSalvage); SetAction(furyButton, v.CanFury); SetAction(precisionButton, v.CanPrecision); SetAction(keystoneButton, v.CanKeystone); SetAction(resetButton, v.CanReset);
        }

        private static string ShortName(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            string[] words = value.Split(' ');
            return words.Length == 1 ? (value.Length > 6 ? value.Substring(0, 6) : value) : words[words.Length - 1];
        }

        private void RequestDiscard()
        {
            View latest = read();
            string currentId = latest == null || latest.Selected == null ? null : latest.Selected.Id;
            if (currentId == null) { discardConfirm = false; return; }
            if (!discardConfirm || discardItemId != currentId) { discardConfirm = true; discardItemId = currentId; return; }
            discardConfirm = false; discardItemId = null; discard();
        }

        private static void SetAction(VisualElement element, bool available) { element.SetEnabled(available); element.style.opacity = available ? 1 : .45f; }
        private static Color RarityColor(string rarity)
        {
            string value = (rarity ?? "").ToLowerInvariant();
            return value == "epic" ? new Color32(255, 86, 108, 255) : value == "legend" ? new Color32(255, 143, 63, 255) :
                value == "unique" ? Purple : value == "rare" ? Gold : value == "magic" ? Sky : Cream;
        }

        private Texture2D Texture(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            if (!textures.TryGetValue(key, out Texture2D texture))
            {
                texture = Resources.Load<Texture2D>(key.Contains("/") ? key : "AffixGenerated/" + key);
                textures[key] = texture;
            }
            return texture;
        }

        private void Skin(VisualElement element, string key)
        {
            Texture2D texture = Texture(key);
            if (texture != null) element.style.backgroundImage = new StyleBackground(texture);
        }

        private VisualElement HiddenButton(VisualElement parent, string name, Action action, out Label caption)
        {
            var node = Button(parent, name, "", 0, 0, 1, 1, action, Color.clear);
            caption = node.Q<Label>();
            return node;
        }

        private static Image Icon(VisualElement parent, float x, float y, float width, float height)
        {
            var image = new Image { scaleMode = ScaleMode.ScaleToFit, pickingMode = PickingMode.Ignore };
            Place(image, x, y, width, height); parent.Add(image); return image;
        }

        private static Label CompactStat(VisualElement parent, float x, string caption)
        {
            Text(parent, caption, x, 5, 42, 14, 7, Muted);
            return Text(parent, "", x, 18, 101, 22, 12, Cream);
        }

        private void RefreshEquipped(int index, ItemView item, string slot)
        {
            equippedImages[index].image = Texture(item == null ? EmptyIcons[index] : item.Icon);
            equippedImages[index].style.opacity = item == null ? .13f : 1f;
            equippedImages[index].tintColor = item == null ? new Color(.72f, .72f, .75f, 1f) : Color.white;
            equippedNames[index].text = item == null ? slot : ShortName(item.Name);
            equippedNames[index].style.color = item == null ? Muted : RarityColor(item.Rarity);
            equippedSummaries[index].text = item == null ? "" : item.Summary ?? item.Affix ?? "";
            string description = item == null ? slot + " — EMPTY" : slot + " — " + item.Name + "\n" + (item.Summary ?? item.Affix ?? "");
            equippedNames[index].tooltip = equippedSummaries[index].tooltip = description;
        }

        private static VisualElement Button(VisualElement parent, string name, string caption, float x, float y, float width, float height, Action action, Color color)
        {
            var button = Box(parent, name, x, y, width, height, color); button.pickingMode = PickingMode.Position; button.focusable = true; Border(button, Edge, 1);
            Text(button, caption, 0, 0, width, height, 10, Cream).style.unityTextAlign = TextAnchor.MiddleCenter;
            button.RegisterCallback<ClickEvent>(_ => action());
            button.RegisterCallback<NavigationSubmitEvent>(evt => { action(); evt.StopPropagation(); });
            return button;
        }

        private static void Place(VisualElement element, float x, float y, float width, float height) { element.style.position = Position.Absolute; element.style.left = x; element.style.top = y; element.style.width = width; element.style.height = height; }
        private static VisualElement Box(VisualElement parent, string name, float x, float y, float width, float height, Color color)
        {
            var box = new VisualElement { name = name, pickingMode = PickingMode.Ignore }; Place(box, x, y, width, height); box.style.backgroundColor = color; parent.Add(box); return box;
        }
        private static Label Text(VisualElement parent, string value, float x, float y, float width, float height, int size, Color color)
        {
            var label = new Label(value) { pickingMode = PickingMode.Ignore }; Place(label, x, y, width, height); label.style.fontSize = size; label.style.color = color;
            label.style.marginLeft = label.style.marginRight = label.style.marginTop = label.style.marginBottom = 0;
            label.style.paddingLeft = label.style.paddingRight = label.style.paddingTop = label.style.paddingBottom = 0; parent.Add(label); return label;
        }
        private static void Border(VisualElement element, Color color, float width)
        {
            element.style.borderTopColor = element.style.borderRightColor = element.style.borderBottomColor = element.style.borderLeftColor = color;
            element.style.borderTopWidth = element.style.borderRightWidth = element.style.borderBottomWidth = element.style.borderLeftWidth = width;
        }
    }
}
