extends Node

signal game_state_changed(state: GameState)
signal stats_changed
signal inventory_changed
signal equipment_changed
signal skills_changed
signal floor_changed(new_floor: int)
signal speed_changed(multiplier: float)
signal class_selected(class_id: String)
signal notification_requested(message: String, color: Color)
signal player_died
signal statistics_changed

enum GameState {
	CLASS_SELECTION,
	RUNNING,
	PAUSED,
}

const AVAILABLE_SPEED_MULTIPLIERS: Array[float] = [1.0, 2.0, 5.0]
const EQUIPMENT_SLOTS: Array[String] = [
	"weapon", "helmet", "armor", "gloves", "boots", "ring", "amulet"
]
const INVENTORY_CAPACITY: int = 60

var hp: int = 0
var max_hp: int = 100
var mp: int = 0
var max_mp: int = 50
var atk: int = 10
var def: int = 5
var spd: float = 1.0
var crit: float = 5.0
var vamp: float = 0.0
var xp_bonus: float = 0.0
var gold_bonus: float = 0.0
var penetration: float = 0.0
var level: int = 1
var xp: int = 0
var gold: int = 0
var floor: int = 1
var kills_on_floor: int = 0

var selected_class: String = ""
var selected_class_name: String = ""
var selected_skill_type: String = ""
var class_base_stats: Dictionary = {
	"max_hp": 100,
	"max_mp": 50,
	"atk": 10,
	"def": 5,
	"spd": 1.0,
	"crit": 5.0,
}

var inventory: Array[Dictionary] = []
var equipment: Dictionary = {}
const CLASS_SKILL_DEFINITIONS: Dictionary = {
	"warrior": [
		{"id": "warrior_fury", "name": "격노", "description": "모든 피해 +8% / Lv", "effect": "damage", "value": 0.08, "base_cost": 100, "cost_step": 80},
		{"id": "warrior_iron_skin", "name": "강철 피부", "description": "받는 피해 -5% / Lv", "effect": "reduction", "value": 0.05, "base_cost": 120, "cost_step": 90},
		{"id": "warrior_bloodlust", "name": "피의 갈증", "description": "처치 시 최대 HP 2% / Lv 회복", "effect": "heal_on_kill", "value": 2.0, "base_cost": 150, "cost_step": 100},
	],
	"mage": [
		{"id": "mage_spell_power", "name": "주문 증폭", "description": "모든 피해 +10% / Lv", "effect": "damage", "value": 0.10, "base_cost": 110, "cost_step": 85},
		{"id": "mage_arcane_focus", "name": "비전 집중", "description": "치명타 +2.5% / Lv", "effect": "crit", "value": 2.5, "base_cost": 130, "cost_step": 95},
		{"id": "mage_quick_cast", "name": "고속 시전", "description": "공격 속도 +0.05 / Lv", "effect": "speed", "value": 0.05, "base_cost": 150, "cost_step": 105},
	],
	"knight": [
		{"id": "knight_bulwark", "name": "철벽", "description": "받는 피해 -7% / Lv", "effect": "reduction", "value": 0.07, "base_cost": 120, "cost_step": 90},
		{"id": "knight_fortitude", "name": "불굴", "description": "최대 HP +6% / Lv", "effect": "max_hp", "value": 0.06, "base_cost": 140, "cost_step": 100},
		{"id": "knight_smite", "name": "심판의 일격", "description": "모든 피해 +6% / Lv", "effect": "damage", "value": 0.06, "base_cost": 160, "cost_step": 110},
	],
	"sage": [
		{"id": "sage_overload", "name": "마력 과부하", "description": "모든 피해 +9% / Lv", "effect": "damage", "value": 0.09, "base_cost": 130, "cost_step": 95},
		{"id": "sage_insight", "name": "통찰", "description": "치명타 +3% / Lv", "effect": "crit", "value": 3.0, "base_cost": 150, "cost_step": 105},
		{"id": "sage_flow", "name": "마력 순환", "description": "공격 속도 +0.04 / Lv", "effect": "speed", "value": 0.04, "base_cost": 170, "cost_step": 115},
	],
	"assassin": [
		{"id": "assassin_lethality", "name": "치명 숙련", "description": "치명타 +4% / Lv", "effect": "crit", "value": 4.0, "base_cost": 130, "cost_step": 95},
		{"id": "assassin_execution", "name": "처형", "description": "모든 피해 +8% / Lv", "effect": "damage", "value": 0.08, "base_cost": 150, "cost_step": 105},
		{"id": "assassin_momentum", "name": "가속", "description": "공격 속도 +0.07 / Lv", "effect": "speed", "value": 0.07, "base_cost": 170, "cost_step": 115},
	],
	"saint": [
		{"id": "saint_blessing", "name": "수호의 축복", "description": "받는 피해 -4% / Lv", "effect": "reduction", "value": 0.04, "base_cost": 130, "cost_step": 95},
		{"id": "saint_grace", "name": "은총", "description": "최대 HP +5% / Lv", "effect": "max_hp", "value": 0.05, "base_cost": 150, "cost_step": 105},
		{"id": "saint_recovery", "name": "성스러운 회복", "description": "처치 시 최대 HP 2.5% / Lv 회복", "effect": "heal_on_kill", "value": 2.5, "base_cost": 170, "cost_step": 115},
	],
}

var class_skill_levels: Dictionary = {
	"warrior": {}, "mage": {}, "knight": {}, "sage": {}, "assassin": {}, "saint": {},
}

var rebirth_count: int = 0
var rebirth_points: int = 0
var permanent_upgrades: Dictionary = {
	"atk": 0,
	"def": 0,
	"hp": 0,
	"spd": 0,
}
var unlocked_classes: Array[String] = ["warrior", "mage"]
var statistics: Dictionary = {
	"total_kills": 0,
	"total_gold_earned": 0,
	"highest_floor": 1,
	"total_drops": 0,
}

var game_state: GameState = GameState.CLASS_SELECTION
var speed_multiplier: float = 1.0
var loot_min_rarity_index: int = 0
var master_volume: float = 1.0
var fullscreen_enabled: bool = false
var autosave_enabled: bool = true


func _ready() -> void:
	_ensure_equipment_slots()
	hp = max_hp
	mp = max_mp


func select_class(class_data: ClassData) -> void:
	selected_class = class_data.id
	selected_class_name = class_data.display_name
	selected_skill_type = class_data.skill_type
	class_base_stats = {
		"max_hp": class_data.base_hp,
		"max_mp": class_data.base_mp,
		"atk": class_data.base_atk,
		"def": class_data.base_def,
		"spd": class_data.base_spd,
		"crit": class_data.base_crit,
	}
	recalculate_stats(false)
	hp = max_hp
	mp = max_mp
	set_game_state(GameState.RUNNING)
	class_selected.emit(selected_class)
	stats_changed.emit()


func set_game_state(next_state: GameState) -> void:
	game_state = next_state
	game_state_changed.emit(game_state)


func set_loot_min_rarity(index: int) -> void:
	loot_min_rarity_index = clampi(index, 0, 4)
	notification_requested.emit("자동 획득 등급: %s 이상" % ["일반", "마법", "희귀", "고유", "전설"][loot_min_rarity_index], Color("d9a441"))


func set_master_volume(value: float) -> void:
	master_volume = clampf(value, 0.0, 1.0)
	AudioManager.set_master_volume(master_volume)


func set_fullscreen(enabled: bool) -> void:
	fullscreen_enabled = enabled
	DisplayServer.window_set_mode(DisplayServer.WINDOW_MODE_FULLSCREEN if enabled else DisplayServer.WINDOW_MODE_WINDOWED)


func set_autosave(enabled: bool) -> void:
	autosave_enabled = enabled
	SaveManager.set_autosave_enabled(enabled)


func set_speed_multiplier(multiplier: float) -> void:
	if not AVAILABLE_SPEED_MULTIPLIERS.any(func(value: float) -> bool: return is_equal_approx(value, multiplier)):
		return
	speed_multiplier = multiplier
	Engine.time_scale = speed_multiplier
	speed_changed.emit(speed_multiplier)


func recalculate_stats(preserve_current: bool = true) -> void:
	var hp_ratio: float = float(hp) / float(maxi(max_hp, 1))
	var mp_ratio: float = float(mp) / float(maxi(max_mp, 1))
	max_hp = int(class_base_stats.get("max_hp", 100)) + ((level - 1) * 5) + int(permanent_upgrades.get("hp", 0)) * 10
	max_mp = int(class_base_stats.get("max_mp", 50))
	atk = int(class_base_stats.get("atk", 10)) + ((level - 1) * 2) + int(permanent_upgrades.get("atk", 0)) * 2
	def = int(class_base_stats.get("def", 5)) + (level - 1) + int(permanent_upgrades.get("def", 0)) * 2
	spd = float(class_base_stats.get("spd", 1.0)) + float(permanent_upgrades.get("spd", 0)) * 0.05
	crit = float(class_base_stats.get("crit", 5.0)) + class_skill_effect("crit")
	spd += class_skill_effect("speed")
	vamp = 0.0
	xp_bonus = 0.0
	gold_bonus = 0.0
	penetration = 0.0

	var hp_skill_multiplier: float = 1.0 + class_skill_effect("max_hp")
	max_hp = maxi(1, roundi(max_hp * hp_skill_multiplier))

	for slot: String in EQUIPMENT_SLOTS:
		var item: Dictionary = equipment.get(slot, {})
		if item.is_empty():
			continue
		_apply_item_stats(item)

	if preserve_current:
		hp = clampi(int(round(max_hp * hp_ratio)), 0, max_hp)
		mp = clampi(int(round(max_mp * mp_ratio)), 0, max_mp)
	else:
		hp = max_hp
		mp = max_mp
	stats_changed.emit()


func _apply_item_stats(item: Dictionary) -> void:
	var base_stats: Dictionary = item.get("base_stats", {})
	for stat_name: Variant in base_stats.keys():
		_add_stat(String(stat_name), float(base_stats[stat_name]))
	var affixes: Array = item.get("affixes", [])
	for affix_data: Variant in affixes:
		if affix_data is Dictionary:
			_add_stat(String(affix_data.get("stat", "")), float(affix_data.get("value", 0.0)))


func _add_stat(stat_name: String, value: float) -> void:
	match stat_name:
		"ATK": atk += int(round(value))
		"DEF": def += int(round(value))
		"HP": max_hp += int(round(value))
		"MP": max_mp += int(round(value))
		"SPD": spd += value
		"CRIT": crit += value
		"VAMP": vamp += value
		"XP_BONUS": xp_bonus += value
		"GOLD_BONUS": gold_bonus += value
		"PEN": penetration += value


func take_damage(raw_damage: float) -> int:
	var defense_reduction: float = clampf(class_skill_effect("reduction"), 0.0, 0.8)
	var damage: int = maxi(1, int(round(maxf(1.0, raw_damage - def) * (1.0 - defense_reduction))))
	hp = maxi(0, hp - damage)
	stats_changed.emit()
	if hp <= 0:
		player_died.emit()
	return damage


func heal_after_kill() -> void:
	var heal_percent: float = vamp + class_skill_effect("heal_on_kill")
	if heal_percent <= 0.0:
		return
	hp = mini(max_hp, hp + int(round(max_hp * heal_percent * 0.01)))
	stats_changed.emit()


func revive() -> void:
	hp = max_hp
	mp = max_mp
	stats_changed.emit()


func add_gold(amount: int) -> void:
	if amount <= 0:
		return
	gold += amount
	statistics["total_gold_earned"] = int(statistics.get("total_gold_earned", 0)) + amount
	stats_changed.emit()
	statistics_changed.emit()


func spend_gold(amount: int) -> bool:
	if amount < 0 or gold < amount:
		return false
	gold -= amount
	stats_changed.emit()
	return true


func add_inventory_item(item: Dictionary) -> bool:
	if inventory.size() >= INVENTORY_CAPACITY:
		notification_requested.emit("가방이 가득 차 아이템을 놓쳤습니다.", Color("ff6b6b"))
		return false
	inventory.append(item.duplicate(true))
	statistics["total_drops"] = int(statistics.get("total_drops", 0)) + 1
	inventory_changed.emit()
	statistics_changed.emit()
	return true


func equip_item(item_id: String) -> void:
	var inventory_index: int = _find_inventory_index(item_id)
	if inventory_index < 0:
		return
	var item: Dictionary = inventory.pop_at(inventory_index)
	var slot: String = String(item.get("slot", ""))
	if not EQUIPMENT_SLOTS.has(slot):
		inventory.append(item)
		return
	var old_item: Dictionary = equipment.get(slot, {})
	equipment[slot] = item
	if not old_item.is_empty():
		inventory.append(old_item)
	recalculate_stats()
	inventory_changed.emit()
	equipment_changed.emit()
	notification_requested.emit("%s 장착" % String(item.get("name", "아이템")), Color("75e68a"))


func unequip_item(slot: String) -> void:
	if inventory.size() >= INVENTORY_CAPACITY:
		notification_requested.emit("가방이 가득 차 장비를 해제할 수 없습니다.", Color("ff6b6b"))
		return
	var item: Dictionary = equipment.get(slot, {})
	if item.is_empty():
		return
	inventory.append(item)
	equipment[slot] = {}
	recalculate_stats()
	inventory_changed.emit()
	equipment_changed.emit()


func sell_item(item_id: String) -> void:
	var inventory_index: int = _find_inventory_index(item_id)
	if inventory_index < 0:
		return
	var item: Dictionary = inventory[inventory_index]
	if bool(item.get("locked", false)):
		notification_requested.emit("잠금 아이템은 판매할 수 없습니다.", Color("ffb86b"))
		return
	inventory.pop_at(inventory_index)
	add_gold(int(item.get("sell_value", 0)))
	inventory_changed.emit()


func toggle_item_lock(item_id: String) -> bool:
	var inventory_index: int = _find_inventory_index(item_id)
	if inventory_index < 0:
		return false
	var item: Dictionary = inventory[inventory_index]
	var locked: bool = not bool(item.get("locked", false))
	item["locked"] = locked
	inventory[inventory_index] = item
	inventory_changed.emit()
	notification_requested.emit("%s %s" % [String(item.get("name", "아이템")), "잠금" if locked else "잠금 해제"], Color("ffd166"))
	return locked


func sell_inventory_below_rarity(min_rarity_index: int) -> Dictionary:
	var kept_items: Array[Dictionary] = []
	var sale_total: int = 0
	var sold_count: int = 0
	var protected_count: int = 0
	for item: Dictionary in inventory:
		var below_filter: bool = int(item.get("rarity_index", 0)) < min_rarity_index
		if below_filter and not bool(item.get("locked", false)):
			sale_total += int(item.get("sell_value", 0))
			sold_count += 1
		else:
			if below_filter and bool(item.get("locked", false)):
				protected_count += 1
			kept_items.append(item)
	inventory = kept_items
	if sale_total > 0:
		add_gold(sale_total)
	if sold_count > 0 or protected_count > 0:
		inventory_changed.emit()
	return {
		"sold_count": sold_count,
		"sale_total": sale_total,
		"protected_count": protected_count,
	}


func sell_all_normal() -> void:
	var kept_items: Array[Dictionary] = []
	var sale_total: int = 0
	var protected_count: int = 0
	for item: Dictionary in inventory:
		if String(item.get("rarity_id", "")) == "normal" and not bool(item.get("locked", false)):
			sale_total += int(item.get("sell_value", 0))
		else:
			if String(item.get("rarity_id", "")) == "normal" and bool(item.get("locked", false)):
				protected_count += 1
			kept_items.append(item)
	inventory = kept_items
	if sale_total > 0:
		add_gold(sale_total)
		var protected_text: String = " · 잠금 %d개 보호" % protected_count if protected_count > 0 else ""
		notification_requested.emit("일반 장비 판매 +%dG%s" % [sale_total, protected_text], Color("f6c85f"))
	elif protected_count > 0:
		notification_requested.emit("판매할 일반 장비 없음 · 잠금 %d개 보호" % protected_count, Color("ffd166"))
	inventory_changed.emit()


func class_skill_definitions(class_id: String = selected_class) -> Array:
	return Array(CLASS_SKILL_DEFINITIONS.get(class_id, []))


func class_skill_level(skill_id: String, class_id: String = selected_class) -> int:
	var levels: Dictionary = Dictionary(class_skill_levels.get(class_id, {}))
	return int(levels.get(skill_id, 0))


func class_skill_effect(effect_name: String, class_id: String = selected_class) -> float:
	var total: float = 0.0
	for definition_value: Variant in class_skill_definitions(class_id):
		var definition: Dictionary = definition_value
		if String(definition.get("effect", "")) != effect_name:
			continue
		total += class_skill_level(String(definition.get("id", "")), class_id) * float(definition.get("value", 0.0))
	return total


func skill_damage_multiplier() -> float:
	return 1.0 + class_skill_effect("damage")


func buy_skill(skill_id: String, base_cost: int, cost_step: int) -> bool:
	if selected_class.is_empty():
		return false
	var levels: Dictionary = Dictionary(class_skill_levels.get(selected_class, {})).duplicate(true)
	var current_level: int = int(levels.get(skill_id, 0))
	var cost: int = base_cost + current_level * cost_step
	if not spend_gold(cost):
		return false
	levels[skill_id] = current_level + 1
	class_skill_levels[selected_class] = levels
	recalculate_stats()
	skills_changed.emit()
	return true


func record_kill() -> void:
	kills_on_floor += 1
	statistics["total_kills"] = int(statistics.get("total_kills", 0)) + 1
	heal_after_kill()
	statistics_changed.emit()


func advance_floor() -> void:
	floor += 1
	kills_on_floor = 0
	statistics["highest_floor"] = maxi(int(statistics.get("highest_floor", 1)), floor)
	floor_changed.emit(floor)
	statistics_changed.emit()


func retreat_floor() -> void:
	floor = maxi(1, floor - 1)
	kills_on_floor = 0
	floor_changed.emit(floor)


func reset_run_progress() -> void:
	level = 1
	xp = 0
	gold = 0
	floor = 1
	kills_on_floor = 0
	selected_class = ""
	selected_class_name = ""
	selected_skill_type = ""
	inventory.clear()
	_ensure_equipment_slots(true)
	class_skill_levels = {"warrior": {}, "mage": {}, "knight": {}, "sage": {}, "assassin": {}, "saint": {}}
	set_speed_multiplier(1.0)
	set_game_state(GameState.CLASS_SELECTION)
	inventory_changed.emit()
	equipment_changed.emit()
	skills_changed.emit()
	floor_changed.emit(floor)


func reset_for_rebirth() -> void:
	level = 1
	xp = 0
	floor = 1
	kills_on_floor = 0
	selected_class = ""
	selected_class_name = ""
	selected_skill_type = ""
	set_speed_multiplier(1.0)
	set_game_state(GameState.CLASS_SELECTION)
	stats_changed.emit()
	inventory_changed.emit()
	equipment_changed.emit()
	skills_changed.emit()
	floor_changed.emit(floor)


func to_save_dict() -> Dictionary:
	return {
		"version": 2,
		"hp": hp,
		"mp": mp,
		"level": level,
		"xp": xp,
		"gold": gold,
		"floor": floor,
		"kills_on_floor": kills_on_floor,
		"selected_class": selected_class,
		"selected_class_name": selected_class_name,
		"selected_skill_type": selected_skill_type,
		"class_base_stats": class_base_stats.duplicate(true),
		"inventory": inventory.duplicate(true),
		"equipment": equipment.duplicate(true),
		"class_skill_levels": class_skill_levels.duplicate(true),
		"rebirth_count": rebirth_count,
		"rebirth_points": rebirth_points,
		"permanent_upgrades": permanent_upgrades.duplicate(true),
		"unlocked_classes": unlocked_classes.duplicate(),
		"statistics": statistics.duplicate(true),
		"speed_multiplier": speed_multiplier,
		"loot_min_rarity_index": loot_min_rarity_index,
		"master_volume": master_volume,
		"fullscreen_enabled": fullscreen_enabled,
		"autosave_enabled": autosave_enabled,
		"saved_at": Time.get_unix_time_from_system(),
	}


func apply_save_dict(data: Dictionary) -> void:
	level = maxi(1, int(data.get("level", 1)))
	xp = maxi(0, int(data.get("xp", 0)))
	gold = maxi(0, int(data.get("gold", 0)))
	floor = maxi(1, int(data.get("floor", 1)))
	kills_on_floor = maxi(0, int(data.get("kills_on_floor", 0)))
	selected_class = String(data.get("selected_class", ""))
	selected_class_name = String(data.get("selected_class_name", ""))
	selected_skill_type = String(data.get("selected_skill_type", ""))
	class_base_stats = Dictionary(data.get("class_base_stats", class_base_stats)).duplicate(true)
	inventory.assign(Array(data.get("inventory", [])))
	for item: Dictionary in inventory:
		if not item.has("locked"):
			item["locked"] = false
	equipment = Dictionary(data.get("equipment", {})).duplicate(true)
	_ensure_equipment_slots()
	class_skill_levels = Dictionary(data.get("class_skill_levels", class_skill_levels)).duplicate(true)
	if data.has("skill_levels") and not data.has("class_skill_levels"):
		var legacy_levels: Dictionary = Dictionary(data.get("skill_levels", {}))
		class_skill_levels["warrior"] = {
			"warrior_fury": int(legacy_levels.get("attack_boost", 0)),
			"warrior_iron_skin": int(legacy_levels.get("defense_boost", 0)),
			"warrior_bloodlust": int(legacy_levels.get("life_steal", 0)),
		}
	for class_key: Variant in CLASS_SKILL_DEFINITIONS.keys():
		var class_id: String = String(class_key)
		if not class_skill_levels.has(class_id):
			class_skill_levels[class_id] = {}
	rebirth_count = maxi(0, int(data.get("rebirth_count", 0)))
	rebirth_points = maxi(0, int(data.get("rebirth_points", 0)))
	permanent_upgrades = Dictionary(data.get("permanent_upgrades", permanent_upgrades)).duplicate(true)
	unlocked_classes.assign(Array(data.get("unlocked_classes", ["warrior", "mage"])))
	if not unlocked_classes.has("warrior"):
		unlocked_classes.append("warrior")
	if not unlocked_classes.has("mage"):
		unlocked_classes.append("mage")
	statistics = Dictionary(data.get("statistics", statistics)).duplicate(true)
	recalculate_stats(false)
	hp = clampi(int(data.get("hp", max_hp)), 0, max_hp)
	mp = clampi(int(data.get("mp", max_mp)), 0, max_mp)
	set_speed_multiplier(float(data.get("speed_multiplier", 1.0)))
	loot_min_rarity_index = clampi(int(data.get("loot_min_rarity_index", 0)), 0, 4)
	set_master_volume(float(data.get("master_volume", 1.0)))
	set_fullscreen(bool(data.get("fullscreen_enabled", false)))
	set_autosave(bool(data.get("autosave_enabled", true)))
	set_game_state(GameState.RUNNING if not selected_class.is_empty() else GameState.CLASS_SELECTION)
	stats_changed.emit()
	inventory_changed.emit()
	equipment_changed.emit()
	skills_changed.emit()
	floor_changed.emit(floor)


func _find_inventory_index(item_id: String) -> int:
	for index: int in inventory.size():
		if String(inventory[index].get("id", "")) == item_id:
			return index
	return -1


func _ensure_equipment_slots(clear_existing: bool = false) -> void:
	if clear_existing:
		equipment.clear()
	for slot: String in EQUIPMENT_SLOTS:
		if not equipment.has(slot):
			equipment[slot] = {}
