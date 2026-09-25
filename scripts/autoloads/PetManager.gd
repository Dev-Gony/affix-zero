extends Node

signal pet_state_changed
signal active_pet_changed(pet_id: String)
signal pet_unlocked(pet_id: String)

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

var _catalog: Dictionary = {}
var owned_pets: Dictionary = {}
var active_pet_id: String = ""
var essence: int = 0


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
	owned_pets[pet_id] = {"level": 1, "xp": 0, "stars": 1}
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


func add_essence(amount: int) -> int:
	if amount <= 0:
		return 0
	essence += amount
	pet_state_changed.emit()
	return amount


func evolution_essence_cost(pet_id: String) -> int:
	var stars: int = stars_for(pet_id)
	match stars:
		1: return 3
		2: return 8
		3: return 16
		4: return 30
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
		and essence >= evolution_essence_cost(pet_id)
	)


func evolve_pet(pet_id: String) -> bool:
	if not can_evolve(pet_id):
		return false
	var cost: int = evolution_cost(pet_id)
	var essence_cost: int = evolution_essence_cost(pet_id)
	if essence < essence_cost:
		return false
	if not GameManager.spend_gold(cost):
		return false
	essence -= essence_cost
	var state: Dictionary = pet_state(pet_id)
	state["stars"] = mini(MAX_STARS, int(state.get("stars", 1)) + 1)
	owned_pets[pet_id] = state
	pet_state_changed.emit()
	return true


func try_unlock_for_floor(floor_number: int) -> Array[String]:
	var unlocked: Array[String] = []
	for pet_id: String in _catalog.keys():
		if is_owned(pet_id):
			continue
		var data: PetData = get_pet_data(pet_id)
		if data != null and floor_number >= data.unlock_floor:
			owned_pets[pet_id] = {"level": 1, "xp": 0, "stars": 1}
			unlocked.append(pet_id)
			pet_unlocked.emit(pet_id)
	if not unlocked.is_empty():
		pet_state_changed.emit()
	return unlocked


func to_save_dict() -> Dictionary:
	return {
		"active_pet_id": active_pet_id,
		"owned_pets": owned_pets.duplicate(true),
		"essence": essence,
	}


func apply_save_dict(data: Dictionary) -> void:
	owned_pets = Dictionary(data.get("owned_pets", {})).duplicate(true)
	active_pet_id = String(data.get("active_pet_id", ""))
	essence = maxi(0, int(data.get("essence", 0)))
	_ensure_starter_pet()
	pet_state_changed.emit()
	active_pet_changed.emit(active_pet_id)
