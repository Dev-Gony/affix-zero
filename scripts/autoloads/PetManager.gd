extends Node

signal pet_state_changed
signal active_pet_changed(pet_id: String)
signal pet_unlocked(pet_id: String)
signal pet_summoned(results: Array)

const PET_RESOURCE_PATHS: Array[String] = [
	"res://resources/pets/spirit_fox.tres",
	"res://resources/pets/ember_drake.tres",
	"res://resources/pets/ghost_slime.tres",
	"res://resources/pets/stone_golem.tres",
	"res://resources/pets/night_bat.tres",
	"res://resources/pets/meadow_fairy.tres",
]
const STARTER_PET_ID: String = "spirit_fox"
const MAX_LEVEL: int = 75
const MAX_STARS: int = 5
const SINGLE_SUMMON_COST: int = 30
const TEN_SUMMON_COST: int = 270
const TEN_PULL_GUARANTEE_RARITY: int = 3

var _catalog: Dictionary = {}
var owned_pets: Dictionary = {}
var active_pet_id: String = ""
var essence: int = 0
var summon_count: int = 0
var last_summon_results: Array[Dictionary] = []


func _ready() -> void:
	_load_catalog()
	_ensure_starter_pet()


func _load_catalog() -> void:
	_catalog.clear()
	for path: String in PET_RESOURCE_PATHS:
		var resource: Resource = load(path)
		if resource is PetData:
			var pet := resource as PetData
			_catalog[pet.id] = pet
	if _catalog.size() != PET_RESOURCE_PATHS.size():
		push_error("Pet catalog incomplete: %d/%d" % [_catalog.size(), PET_RESOURCE_PATHS.size()])


func _ensure_starter_pet() -> void:
	if not owned_pets.has(STARTER_PET_ID):
		owned_pets[STARTER_PET_ID] = {
			"level": 1,
			"xp": 0,
			"stars": 1,
			"fragments": 0,
		}
	if active_pet_id.is_empty() or not owned_pets.has(active_pet_id):
		active_pet_id = STARTER_PET_ID


func all_pet_ids() -> Array[String]:
	var ids: Array[String] = []
	for path: String in PET_RESOURCE_PATHS:
		var resource: Resource = load(path)
		if resource is PetData:
			ids.append((resource as PetData).id)
	return ids


func get_pet_data(pet_id: String) -> PetData:
	return _catalog.get(pet_id, null) as PetData


static func rarity_color(rarity_index: int) -> Color:
	match clampi(rarity_index, 0, 5):
		0: return Color("b7c0ca")
		1: return Color("59d27d")
		2: return Color("4ba3ff")
		3: return Color("b869ff")
		4: return Color("ffb83d")
		5: return Color("ff4f5f")
	return Color.WHITE


static func rarity_weight(rarity_index: int) -> float:
	match clampi(rarity_index, 0, 5):
		0: return 48.0
		1: return 30.0
		2: return 15.0
		3: return 5.5
		4: return 1.4
		5: return 0.1
	return 1.0


func summon_cost(count: int) -> int:
	return TEN_SUMMON_COST if count >= 10 else SINGLE_SUMMON_COST


func can_summon(count: int = 1) -> bool:
	return essence >= summon_cost(count)


func _roll_pet_id(min_rarity: int = 0) -> String:
	var candidates: Array[PetData] = []
	var total_weight: float = 0.0
	for pet_id: String in all_pet_ids():
		var data: PetData = get_pet_data(pet_id)
		if data == null or data.rarity_index < min_rarity:
			continue
		candidates.append(data)
		total_weight += rarity_weight(data.rarity_index)
	if candidates.is_empty():
		return STARTER_PET_ID
	var roll: float = randf() * total_weight
	for data: PetData in candidates:
		roll -= rarity_weight(data.rarity_index)
		if roll <= 0.0:
			return data.id
	return candidates.back().id


func summon_pets(count: int = 1) -> Array[Dictionary]:
	var pull_count: int = 10 if count >= 10 else 1
	var cost: int = summon_cost(pull_count)
	if essence < cost:
		return []
	essence -= cost
	var results: Array[Dictionary] = []
	var has_guaranteed_rarity: bool = false
	for index: int in pull_count:
		var pet_id: String = _roll_pet_id()
		var data: PetData = get_pet_data(pet_id)
		if data != null and data.rarity_index >= TEN_PULL_GUARANTEE_RARITY:
			has_guaranteed_rarity = true
		results.append(_apply_summon_result(pet_id))
	if pull_count == 10 and not has_guaranteed_rarity:
		var forced_id: String = _roll_pet_id(TEN_PULL_GUARANTEE_RARITY)
		var replaced: Dictionary = results[results.size() - 1]
		var replaced_id: String = String(replaced.get("pet_id", ""))
		if bool(replaced.get("new", false)):
			owned_pets.erase(replaced_id)
		else:
			var rollback: Dictionary = pet_state(replaced_id)
			rollback["fragments"] = maxi(0, int(rollback.get("fragments", 0)) - int(replaced.get("fragments", 0)))
			owned_pets[replaced_id] = rollback
		results[results.size() - 1] = _apply_summon_result(forced_id)
	summon_count += pull_count
	last_summon_results = results.duplicate(true)
	pet_summoned.emit(results)
	pet_state_changed.emit()
	return results


func _apply_summon_result(pet_id: String) -> Dictionary:
	var data: PetData = get_pet_data(pet_id)
	if data == null:
		return {}
	var is_new: bool = not is_owned(pet_id)
	var fragment_gain: int = 0
	if is_new:
		owned_pets[pet_id] = {"level": 1, "xp": 0, "stars": 1, "fragments": 0}
		pet_unlocked.emit(pet_id)
	else:
		fragment_gain = fragment_reward_for_rarity(data.rarity_index)
		var state: Dictionary = pet_state(pet_id)
		state["fragments"] = int(state.get("fragments", 0)) + fragment_gain
		owned_pets[pet_id] = state
	return {
		"pet_id": pet_id,
		"new": is_new,
		"rarity_index": data.rarity_index,
		"rarity_name": data.rarity_name,
		"fragments": fragment_gain,
	}


static func fragment_reward_for_rarity(rarity_index: int) -> int:
	match clampi(rarity_index, 0, 5):
		0: return 1
		1: return 1
		2: return 2
		3: return 4
		4: return 8
		5: return 16
	return 1


func fragments_for(pet_id: String) -> int:
	return maxi(0, int(pet_state(pet_id).get("fragments", 0)))


func active_pet_data() -> PetData:
	return get_pet_data(active_pet_id)


func pet_state(pet_id: String) -> Dictionary:
	return Dictionary(owned_pets.get(pet_id, {})).duplicate(true)


func is_owned(pet_id: String) -> bool:
	return owned_pets.has(pet_id)


func set_active_pet(pet_id: String) -> bool:
	if not is_owned(pet_id) or get_pet_data(pet_id) == null:
		return false
	if active_pet_id == pet_id:
		return true
	active_pet_id = pet_id
	active_pet_changed.emit(active_pet_id)
	pet_state_changed.emit()
	return true


func unlock_pet(pet_id: String) -> bool:
	if get_pet_data(pet_id) == null:
		return false
	if is_owned(pet_id):
		return false
	owned_pets[pet_id] = {"level": 1, "xp": 0, "stars": 1, "fragments": 0}
	pet_unlocked.emit(pet_id)
	pet_state_changed.emit()
	return true


func level_for(pet_id: String) -> int:
	return clampi(int(pet_state(pet_id).get("level", 1)), 1, MAX_LEVEL)


func stars_for(pet_id: String) -> int:
	return clampi(int(pet_state(pet_id).get("stars", 1)), 1, MAX_STARS)


func xp_needed(level: int) -> int:
	var safe_level: int = clampi(level, 1, MAX_LEVEL)
	return 40 + safe_level * 18 + safe_level * safe_level * 3


func add_xp(pet_id: String, amount: int) -> int:
	if amount <= 0 or not is_owned(pet_id):
		return 0
	var state: Dictionary = pet_state(pet_id)
	var level: int = clampi(int(state.get("level", 1)), 1, MAX_LEVEL)
	var xp: int = maxi(0, int(state.get("xp", 0))) + amount
	var gained: int = 0
	while level < MAX_LEVEL:
		var needed: int = xp_needed(level)
		if xp < needed:
			break
		xp -= needed
		level += 1
		gained += 1
	state["level"] = level
	state["xp"] = 0 if level >= MAX_LEVEL else xp
	owned_pets[pet_id] = state
	if gained > 0:
		pet_state_changed.emit()
	return gained


func active_attack_power(player_attack: float) -> float:
	var data: PetData = active_pet_data()
	if data == null:
		return 0.0
	var level: int = level_for(active_pet_id)
	var stars: int = stars_for(active_pet_id)
	var pet_scale: float = 1.0 + float(level - 1) * 0.07 + float(stars - 1) * 0.16
	return maxf(1.0, data.base_attack * pet_scale + player_attack * 0.18)


func active_attack_interval() -> float:
	var data: PetData = active_pet_data()
	if data == null:
		return 999.0
	var level_bonus: float = maxf(0.72, 1.0 - float(level_for(active_pet_id) - 1) * 0.003)
	return maxf(0.45, data.attack_interval * level_bonus)


func active_support_heal_percent() -> float:
	var data: PetData = active_pet_data()
	if data == null:
		return 0.0
	return data.support_heal_percent * (1.0 + float(stars_for(active_pet_id) - 1) * 0.12)


func active_support_interval() -> float:
	var data: PetData = active_pet_data()
	return data.support_interval if data != null else 999.0


func _active_passive_scale() -> float:
	return 1.0 + float(stars_for(active_pet_id) - 1) * 0.15


func active_player_damage_bonus_percent() -> float:
	var data: PetData = active_pet_data()
	return data.player_damage_bonus_percent * _active_passive_scale() if data != null else 0.0


func active_player_damage_reduction_percent() -> float:
	var data: PetData = active_pet_data()
	return data.player_damage_reduction_percent * _active_passive_scale() if data != null else 0.0


func active_player_crit_bonus_percent() -> float:
	var data: PetData = active_pet_data()
	return data.player_crit_bonus_percent * _active_passive_scale() if data != null else 0.0


func active_player_xp_bonus_percent() -> float:
	var data: PetData = active_pet_data()
	return data.player_xp_bonus_percent * _active_passive_scale() if data != null else 0.0


func active_player_gold_bonus_percent() -> float:
	var data: PetData = active_pet_data()
	return data.player_gold_bonus_percent * _active_passive_scale() if data != null else 0.0


func active_passive_text() -> String:
	var data: PetData = active_pet_data()
	if data == null:
		return ""
	var parts: Array[String] = []
	var damage_bonus: float = active_player_damage_bonus_percent()
	var reduction: float = active_player_damage_reduction_percent()
	var crit_bonus: float = active_player_crit_bonus_percent()
	var xp_bonus: float = active_player_xp_bonus_percent()
	var gold_bonus: float = active_player_gold_bonus_percent()
	if damage_bonus > 0.0:
		parts.append("주인 피해 +%.1f%%" % damage_bonus)
	if reduction > 0.0:
		parts.append("받는 피해 -%.1f%%" % reduction)
	if crit_bonus > 0.0:
		parts.append("치명 +%.1f%%" % crit_bonus)
	if xp_bonus > 0.0:
		parts.append("경험치 +%.1f%%" % xp_bonus)
	if gold_bonus > 0.0:
		parts.append("골드 +%.1f%%" % gold_bonus)
	return " · ".join(parts)


func add_essence(amount: int) -> int:
	if amount <= 0:
		return 0
	essence += amount
	pet_state_changed.emit()
	return amount


func evolution_fragment_cost(pet_id: String) -> int:
	var stars: int = stars_for(pet_id)
	match stars:
		1: return 1
		2: return 2
		3: return 4
		4: return 8
	return 0


func training_cost(pet_id: String) -> int:
	var level: int = level_for(pet_id)
	var stars: int = stars_for(pet_id)
	return maxi(500, roundi((700.0 + level * 420.0) * (1.0 + float(stars - 1) * 0.45)))


func train_pet(pet_id: String) -> bool:
	if not is_owned(pet_id):
		return false
	var level: int = level_for(pet_id)
	if level >= MAX_LEVEL:
		return false
	var cost: int = training_cost(pet_id)
	if not GameManager.spend_gold(cost):
		return false
	var state: Dictionary = pet_state(pet_id)
	var missing_xp: int = maxi(1, xp_needed(level) - int(state.get("xp", 0)))
	add_xp(pet_id, missing_xp)
	pet_state_changed.emit()
	return true


func evolution_required_level(pet_id: String) -> int:
	var stars: int = stars_for(pet_id)
	if stars >= MAX_STARS:
		return MAX_LEVEL
	return mini(MAX_LEVEL, 5 + stars * 12)


func evolution_cost(pet_id: String) -> int:
	var stars: int = stars_for(pet_id)
	return roundi(12000.0 * pow(5.0, float(maxi(0, stars - 1))))


func can_evolve(pet_id: String) -> bool:
	if not is_owned(pet_id):
		return false
	var stars: int = stars_for(pet_id)
	if stars >= MAX_STARS:
		return false
	return (
		level_for(pet_id) >= evolution_required_level(pet_id)
		and GameManager.gold >= evolution_cost(pet_id)
		and fragments_for(pet_id) >= evolution_fragment_cost(pet_id)
	)


func evolve_pet(pet_id: String) -> bool:
	if not can_evolve(pet_id):
		return false
	var cost: int = evolution_cost(pet_id)
	var fragment_cost: int = evolution_fragment_cost(pet_id)
	if fragments_for(pet_id) < fragment_cost:
		return false
	if not GameManager.spend_gold(cost):
		return false
	var state: Dictionary = pet_state(pet_id)
	state["fragments"] = maxi(0, int(state.get("fragments", 0)) - fragment_cost)
	state["stars"] = mini(MAX_STARS, int(state.get("stars", 1)) + 1)
	owned_pets[pet_id] = state
	pet_state_changed.emit()
	return true


func try_unlock_for_floor(_floor_number: int) -> Array[String]:
	# V6: pets are no longer granted automatically by floor. Floor progression
	# feeds summon currency through elite/boss farming; collection comes from gacha.
	return []


func to_save_dict() -> Dictionary:
	return {
		"active_pet_id": active_pet_id,
		"owned_pets": owned_pets.duplicate(true),
		"essence": essence,
		"summon_count": summon_count,
		"last_summon_results": last_summon_results.duplicate(true),
	}


func apply_save_dict(data: Dictionary) -> void:
	owned_pets = Dictionary(data.get("owned_pets", {})).duplicate(true)
	active_pet_id = String(data.get("active_pet_id", ""))
	essence = maxi(0, int(data.get("essence", 0)))
	summon_count = maxi(0, int(data.get("summon_count", 0)))
	last_summon_results = Array(data.get("last_summon_results", [])).duplicate(true)
	_ensure_starter_pet()
	pet_state_changed.emit()
	active_pet_changed.emit(active_pet_id)
