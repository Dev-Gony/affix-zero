extends Node2D
class_name EffectLayer

signal resource_collected(kind: String, amount: int)

const ITEM_BASE_ATLAS: Texture2D = preload("res://assets/sprites/item_base_atlas_v2.png")

var _particles: Array[Dictionary] = []
var _texts: Array[Dictionary] = []
var _rings: Array[Dictionary] = []
var _lines: Array[Dictionary] = []
var _loot_icons: Array[Dictionary] = []
var _resource_pickups: Array[Dictionary] = []
var _pickup_target_position: Vector2 = Vector2.ZERO
var _flash_alpha: float = 0.0


func _process(delta: float) -> void:
	for particle: Dictionary in _particles:
		particle["life"] = float(particle["life"]) - delta
		particle["position"] = Vector2(particle["position"]) + Vector2(particle["velocity"]) * delta
		particle["velocity"] = Vector2(particle["velocity"]) * 0.94
	for index: int in range(_particles.size() - 1, -1, -1):
		if float(_particles[index]["life"]) <= 0.0:
			_particles.remove_at(index)
	for text_data: Dictionary in _texts:
		text_data["life"] = float(text_data["life"]) - delta
		text_data["position"] = Vector2(text_data["position"]) + Vector2(0, -18) * delta
	for index: int in range(_texts.size() - 1, -1, -1):
		if float(_texts[index]["life"]) <= 0.0:
			_texts.remove_at(index)
	for ring: Dictionary in _rings:
		ring["life"] = float(ring["life"]) - delta
		ring["radius"] = float(ring["radius"]) + float(ring["speed"]) * delta
	for index: int in range(_rings.size() - 1, -1, -1):
		if float(_rings[index]["life"]) <= 0.0:
			_rings.remove_at(index)
	for line_data: Dictionary in _lines:
		line_data["life"] = float(line_data["life"]) - delta
	for index: int in range(_lines.size() - 1, -1, -1):
		if float(_lines[index]["life"]) <= 0.0:
			_lines.remove_at(index)
	for loot_icon: Dictionary in _loot_icons:
		loot_icon["life"] = float(loot_icon["life"]) - delta
	for index: int in range(_loot_icons.size() - 1, -1, -1):
		if float(_loot_icons[index]["life"]) <= 0.0:
			_loot_icons.remove_at(index)
	_update_resource_pickups(delta)
	_flash_alpha = maxf(0.0, _flash_alpha - delta * 1.8)
	queue_redraw()


func spawn_fragments(world_position: Vector2, color: Color, count: int, speed: float = 55.0) -> void:
	for index: int in count:
		var direction := Vector2.RIGHT.rotated(randf() * TAU)
		_particles.append({
			"position": world_position,
			"velocity": direction * randf_range(speed * 0.45, speed),
			"color": color,
			"life": randf_range(0.35, 0.7),
			"duration": 0.7,
			"size": randi_range(2, 4),
		})


func show_damage(world_position: Vector2, damage: int, critical: bool) -> void:
	var message: String = "%d" % damage
	var color := Color.WHITE
	var size: int = 11
	if critical:
		message = "치명! %d" % damage
		color = Color("ffd84d")
		size = 14
		spawn_fragments(world_position, color, 7, 70.0)
	_texts.append({"position": world_position + Vector2(-12, -8), "text": message, "color": color, "life": 0.75, "duration": 0.75, "size": size})


func show_gold(world_position: Vector2, amount: int) -> void:
	_texts.append({"position": world_position + Vector2(-8, 8), "text": "+%dG" % amount, "color": Color("f6c85f"), "life": 0.85, "duration": 0.85, "size": 10})


func show_pet_essence(world_position: Vector2, amount: int) -> void:
	if amount <= 0:
		return
	_texts.append({
		"position": world_position + Vector2(-20, 2),
		"text": "펫 정수 +%d" % amount,
		"color": Color("c084fc"),
		"life": 1.05,
		"duration": 1.05,
		"size": 9,
	})
	spawn_fragments(world_position, Color("c084fc"), 7, 52.0)


func show_pet_attack(from: Vector2, to: Vector2, color: Color) -> void:
	var midpoint: Vector2 = from.lerp(to, 0.52) + Vector2(0, -8)
	_lines.append({
		"points": PackedVector2Array([from, midpoint, to]),
		"color": Color(color, 0.92),
		"life": 0.22,
		"duration": 0.22,
		"width": 2.0,
	})
	spawn_fragments(to, color, 4, 36.0)


func show_pet_heal(world_position: Vector2, amount: int, color: Color) -> void:
	if amount <= 0:
		return
	_texts.append({
		"position": world_position + Vector2(-16, -24),
		"text": "펫 회복 +%d" % amount,
		"color": color,
		"life": 0.95,
		"duration": 0.95,
		"size": 9,
	})
	_rings.append({
		"center": world_position,
		"radius": 7.0,
		"speed": 24.0,
		"color": Color(color, 0.72),
		"life": 0.55,
		"duration": 0.55,
		"width": 1.5,
	})


func set_pickup_target(world_position: Vector2) -> void:
	_pickup_target_position = world_position


func spawn_resource_pickup(world_position: Vector2, kind: String, amount: int) -> void:
	var color := Color("61e58b")
	if kind == "gold":
		color = Color("ffd45c")
	elif kind == "pet_essence":
		color = Color("c084fc")
	var launch := Vector2(randf_range(-22.0, 22.0), randf_range(-28.0, -12.0))
	_resource_pickups.append({
		"kind": kind,
		"amount": amount,
		"position": world_position + Vector2(randf_range(-8.0, 8.0), randf_range(-5.0, 5.0)),
		"velocity": launch,
		"delay": randf_range(0.28, 0.52),
		"color": color,
		"age": 0.0,
	})


func _update_resource_pickups(delta: float) -> void:
	for pickup: Dictionary in _resource_pickups:
		pickup["age"] = float(pickup["age"]) + delta
		var delay: float = float(pickup["delay"])
		var position: Vector2 = Vector2(pickup["position"])
		var velocity: Vector2 = Vector2(pickup["velocity"])
		if float(pickup["age"]) < delay:
			velocity.y += 70.0 * delta
			velocity *= 0.96
			position += velocity * delta
		else:
			var direction: Vector2 = position.direction_to(_pickup_target_position)
			var age_after_delay: float = float(pickup["age"]) - delay
			var magnet_speed: float = minf(520.0, 150.0 + age_after_delay * 460.0)
			velocity = velocity.move_toward(direction * magnet_speed, 900.0 * delta)
			position += velocity * delta
		pickup["velocity"] = velocity
		pickup["position"] = position

	for index: int in range(_resource_pickups.size() - 1, -1, -1):
		var pickup: Dictionary = _resource_pickups[index]
		if float(pickup["age"]) < float(pickup["delay"]):
			continue
		if Vector2(pickup["position"]).distance_to(_pickup_target_position) <= 13.0:
			resource_collected.emit(String(pickup["kind"]), int(pickup["amount"]))
			_resource_pickups.remove_at(index)


static func loot_beam_height(rarity_index: int) -> float:
	match clampi(rarity_index, 0, 5):
		0: return 16.0
		1: return 26.0
		2: return 42.0
		3: return 60.0
		4: return 86.0
		5: return 112.0
	return 16.0


static func loot_effect_duration(rarity_index: int) -> float:
	match clampi(rarity_index, 0, 5):
		0: return 0.85
		1: return 1.00
		2: return 1.20
		3: return 1.45
		4: return 1.90
		5: return 2.35
	return 0.85


static func loot_beam_width(rarity_index: int) -> float:
	return 1.0 + float(clampi(rarity_index, 0, 5)) * 0.65


static func loot_screen_flash_alpha(rarity_index: int) -> float:
	if rarity_index >= 5:
		return 0.68
	if rarity_index >= 4:
		return 0.48
	return 0.0


func show_drop(world_position: Vector2, item: Dictionary) -> void:
	var color := Color.from_string(String(item.get("rarity_color", "ffffff")), Color.WHITE)
	var rarity_index: int = clampi(int(item.get("rarity_index", 0)), 0, 5)
	var duration: float = loot_effect_duration(rarity_index)
	var rarity_name: String = String(item.get("rarity_name", ""))
	var display_name: String = String(item.get("name", "아이템"))
	var label: String = "[%s] %s" % [rarity_name, display_name] if not rarity_name.is_empty() else display_name
	_texts.append({
		"position": world_position + Vector2(-34, -22),
		"text": label,
		"color": color,
		"life": duration,
		"duration": duration,
		"size": 10 + mini(rarity_index, 3),
	})
	var icon_index: int = int(item.get("icon_index", -1))
	if icon_index >= 0 and icon_index < 30:
		_loot_icons.append({
			"position": world_position,
			"icon_index": icon_index,
			"slot": String(item.get("slot", "")),
			"base_id": String(item.get("base_id", "")),
			"color": color,
			"rarity_index": rarity_index,
			"life": duration,
			"duration": duration,
			"beam_height": loot_beam_height(rarity_index),
			"beam_width": loot_beam_width(rarity_index),
		})
	if rarity_index >= 2:
		_rings.append({
			"center": world_position,
			"radius": 5.0,
			"speed": 32.0 + rarity_index * 9.0,
			"color": Color(color, 0.90),
			"life": minf(duration, 0.85 + rarity_index * 0.10),
			"duration": minf(duration, 0.85 + rarity_index * 0.10),
			"width": 1.0 + rarity_index * 0.45,
		})
	if rarity_index >= 3:
		spawn_fragments(world_position, color, 4 + rarity_index * 2, 42.0 + rarity_index * 10.0)
	var flash_alpha: float = loot_screen_flash_alpha(rarity_index)
	if flash_alpha > 0.0:
		_flash_alpha = maxf(_flash_alpha, flash_alpha)
		spawn_fragments(world_position, color, 20 + rarity_index * 4, 92.0 + rarity_index * 7.0)


func show_pickup(from: Vector2, to: Vector2, item: Dictionary) -> void:
	var color := Color.from_string(String(item.get("rarity_color", "ffffff")), Color.WHITE)
	_lines.append({
		"points": PackedVector2Array([from, from.lerp(to, 0.45) + Vector2(0, -18), to]),
		"color": Color(color, 0.85),
		"life": 0.24,
		"duration": 0.24,
		"width": 2.0,
	})
	spawn_fragments(from, color, 5, 42.0)


func show_level_up(world_position: Vector2) -> void:
	_texts.append({"position": world_position + Vector2(-25, -24), "text": "LEVEL UP!", "color": Color("ffd84d"), "life": 1.5, "duration": 1.5, "size": 14})
	spawn_fragments(world_position, Color("ffd84d"), 15, 85.0)


func show_boss_arrival(world_position: Vector2, boss_name: String) -> void:
	_texts.append({
		"position": world_position + Vector2(-42, -36),
		"text": "BOSS · %s" % boss_name,
		"color": Color("ff7480"),
		"life": 1.55,
		"duration": 1.55,
		"size": 14,
	})
	for index: int in 3:
		_rings.append({
			"center": world_position,
			"radius": 12.0 + index * 10.0,
			"speed": 70.0 + index * 28.0,
			"color": Color("ff5260", 0.82 - index * 0.16),
			"life": 0.80 + index * 0.12,
			"duration": 0.80 + index * 0.12,
			"width": 3.8 - index * 0.65,
		})
	spawn_fragments(world_position, Color("ff5b67"), 26, 110.0)
	_flash_alpha = maxf(_flash_alpha, 0.32)


func show_elite_arrival(world_position: Vector2, elite_name: String, color: Color) -> void:
	_texts.append({
		"position": world_position + Vector2(-30, -28),
		"text": "ELITE · %s" % elite_name,
		"color": color,
		"life": 1.15,
		"duration": 1.15,
		"size": 11,
	})
	_rings.append({
		"center": world_position,
		"radius": 8.0,
		"speed": 70.0,
		"color": Color(color, 0.80),
		"life": 0.62,
		"duration": 0.62,
		"width": 2.4,
	})
	spawn_fragments(world_position, color, 12, 76.0)


func show_attack(from: Vector2, to: Vector2, class_id: String, critical: bool) -> void:
	var direction: Vector2 = from.direction_to(to)
	if direction.is_zero_approx():
		direction = Vector2.RIGHT
	var tangent := Vector2(-direction.y, direction.x)
	var impact_color := Color("ffd84d") if critical else Color("e6edf7")
	match class_id:
		"mage":
			impact_color = Color("c084fc") if not critical else Color("ffe06b")
			_lines.append({"points": PackedVector2Array([from, from.lerp(to, 0.42) + tangent * 7.0, from.lerp(to, 0.72) - tangent * 5.0, to]), "color": impact_color, "life": 0.20, "duration": 0.20, "width": 2.8})
			_rings.append({"center": to, "radius": 4.0, "speed": 74.0, "color": Color(impact_color, 0.78), "life": 0.24, "duration": 0.24, "width": 2.0})
		"sage":
			impact_color = Color("79e7ff") if not critical else Color("fff08a")
			var points := PackedVector2Array([from])
			for index: int in 3:
				var t: float = float(index + 1) / 4.0
				points.append(from.lerp(to, t) + tangent * (6.0 if index % 2 == 0 else -6.0))
			points.append(to)
			_lines.append({"points": points, "color": impact_color, "life": 0.16, "duration": 0.16, "width": 2.4})
			_lines.append({"points": points, "color": Color("f4fbff", 0.64), "life": 0.09, "duration": 0.09, "width": 1.0})
		"saint":
			impact_color = Color("fff2a1") if not critical else Color("ffffff")
			_lines.append({"points": PackedVector2Array([from, to]), "color": impact_color, "life": 0.20, "duration": 0.20, "width": 2.5})
			_rings.append({"center": to, "radius": 3.0, "speed": 62.0, "color": Color("fff7cc"), "life": 0.28, "duration": 0.28, "width": 2.2})
			for index: int in 4:
				var ray := Vector2.UP.rotated(TAU * float(index) / 4.0)
				_lines.append({"points": PackedVector2Array([to + ray * 3.0, to + ray * 12.0]), "color": Color("fff7cc", 0.82), "life": 0.16, "duration": 0.16, "width": 1.5})
		"assassin":
			impact_color = Color("ff6fbd") if not critical else Color("ffe06b")
			_lines.append({"points": PackedVector2Array([to - direction * 11.0 - tangent * 9.0, to + direction * 10.0 + tangent * 9.0]), "color": impact_color, "life": 0.13, "duration": 0.13, "width": 2.8})
			_lines.append({"points": PackedVector2Array([to - direction * 8.0 + tangent * 10.0, to + direction * 9.0 - tangent * 10.0]), "color": Color(impact_color, 0.78), "life": 0.16, "duration": 0.16, "width": 1.8})
		"knight":
			impact_color = Color("8be0f1") if not critical else Color("ffe06b")
			_lines.append({"points": PackedVector2Array([from + tangent * 5.0, from + direction * 18.0, to - tangent * 8.0]), "color": impact_color, "life": 0.17, "duration": 0.17, "width": 3.2})
			_rings.append({"center": to, "radius": 3.0, "speed": 54.0, "color": Color(impact_color, 0.65), "life": 0.20, "duration": 0.20, "width": 1.8})
		_:
			impact_color = Color("ff9b6b") if not critical else Color("ffe06b")
			_lines.append({"points": PackedVector2Array([from + tangent * 5.0, from + direction * 18.0, to - tangent * 8.0]), "color": impact_color, "life": 0.16, "duration": 0.16, "width": 3.0})
			_lines.append({"points": PackedVector2Array([to - direction * 12.0 - tangent * 7.0, to + direction * 7.0 + tangent * 7.0]), "color": Color(impact_color, 0.76), "life": 0.12, "duration": 0.12, "width": 2.0})
	spawn_fragments(to, impact_color, 5 if not critical else 9, 58.0)


func show_enemy_telegraph(from: Vector2, to: Vector2, attack_kind: String, windup_duration: float) -> void:
	var duration: float = maxf(0.12, windup_duration)
	var direction: Vector2 = from.direction_to(to)
	if direction.is_zero_approx():
		direction = Vector2.RIGHT
	var tangent := Vector2(-direction.y, direction.x)
	match attack_kind:
		"slam":
			_rings.append({"center": from, "radius": 8.0, "speed": 28.0, "color": Color("ff765f", 0.52), "life": duration, "duration": duration, "width": 2.0})
		"dive":
			_lines.append({"points": PackedVector2Array([from, to]), "color": Color("ff6b6b", 0.40), "life": duration, "duration": duration, "width": 1.5})
		"shadow_bolt":
			_lines.append({"points": PackedVector2Array([from, from.lerp(to, 0.5) + tangent * 5.0, to]), "color": Color("a96dff", 0.42), "life": duration, "duration": duration, "width": 1.8})
			_rings.append({"center": from, "radius": 5.0, "speed": 18.0, "color": Color("c084fc", 0.42), "life": duration, "duration": duration, "width": 1.6})
		"flame":
			for spread: float in [-0.18, 0.0, 0.18]:
				var ray: Vector2 = direction.rotated(spread)
				_lines.append({"points": PackedVector2Array([from, from + ray * minf(82.0, from.distance_to(to))]), "color": Color("ff7b45", 0.34), "life": duration, "duration": duration, "width": 1.8})
		"hellfire":
			_rings.append({"center": to, "radius": 12.0, "speed": 24.0, "color": Color("ff3b55", 0.52), "life": duration, "duration": duration, "width": 2.4})
			for index: int in 6:
				var ray := Vector2.RIGHT.rotated(TAU * float(index) / 6.0)
				_lines.append({"points": PackedVector2Array([to + ray * 7.0, to + ray * 22.0]), "color": Color("ff725f", 0.34), "life": duration, "duration": duration, "width": 1.4})
		_:
			_lines.append({"points": PackedVector2Array([from + tangent * 7.0, to - tangent * 7.0]), "color": Color("ff5b61", 0.34), "life": duration, "duration": duration, "width": 1.5})


func show_enemy_attack(from: Vector2, to: Vector2, attack_kind: String) -> void:
	var direction: Vector2 = from.direction_to(to)
	if direction.is_zero_approx():
		direction = Vector2.RIGHT
	var tangent := Vector2(-direction.y, direction.x)
	match attack_kind:
		"slam":
			_rings.append({"center": from, "radius": 5.0, "speed": 120.0, "color": Color("ff765f"), "life": 0.28, "duration": 0.28, "width": 4.0})
			spawn_fragments(to, Color("ff765f"), 7, 58.0)
		"dive":
			_lines.append({"points": PackedVector2Array([from, from.lerp(to, 0.6) - tangent * 8.0, to]), "color": Color("ff6b6b"), "life": 0.18, "duration": 0.18, "width": 3.0})
			_lines.append({"points": PackedVector2Array([to - tangent * 10.0, to + tangent * 10.0]), "color": Color("ffd0d0", 0.72), "life": 0.12, "duration": 0.12, "width": 2.0})
		"shadow_bolt":
			var points := PackedVector2Array([from, from.lerp(to, 0.33) + tangent * 7.0, from.lerp(to, 0.66) - tangent * 7.0, to])
			_lines.append({"points": points, "color": Color("b978ff"), "life": 0.26, "duration": 0.26, "width": 3.4})
			_rings.append({"center": to, "radius": 3.0, "speed": 76.0, "color": Color("d7a7ff"), "life": 0.25, "duration": 0.25, "width": 2.0})
			spawn_fragments(to, Color("b978ff"), 8, 56.0)
		"flame":
			for spread: float in [-0.22, -0.10, 0.0, 0.10, 0.22]:
				var ray: Vector2 = direction.rotated(spread)
				_lines.append({"points": PackedVector2Array([from + ray * 6.0, to + ray * 10.0]), "color": Color("ff7a38", 0.82), "life": 0.22, "duration": 0.22, "width": 2.2})
			spawn_fragments(to, Color("ff9f43"), 12, 76.0)
		"hellfire":
			_rings.append({"center": to, "radius": 4.0, "speed": 150.0, "color": Color("ff415c"), "life": 0.34, "duration": 0.34, "width": 5.0})
			_rings.append({"center": from, "radius": 9.0, "speed": 85.0, "color": Color("9f2cff", 0.72), "life": 0.30, "duration": 0.30, "width": 2.5})
			for index: int in 8:
				var ray := Vector2.RIGHT.rotated(TAU * float(index) / 8.0)
				_lines.append({"points": PackedVector2Array([to + ray * 5.0, to + ray * 30.0]), "color": Color("ff725f", 0.78), "life": 0.20, "duration": 0.20, "width": 2.0})
			spawn_fragments(to, Color("ff4f65"), 16, 86.0)
		_:
			_lines.append({"points": PackedVector2Array([to - direction * 9.0 - tangent * 9.0, to + direction * 7.0 + tangent * 9.0]), "color": Color("ff5b61"), "life": 0.16, "duration": 0.16, "width": 3.4})
			_lines.append({"points": PackedVector2Array([to - direction * 5.0 + tangent * 7.0, to + direction * 6.0 - tangent * 7.0]), "color": Color("ffc4c6", 0.58), "life": 0.10, "duration": 0.10, "width": 1.6})


func show_melee_spin(center: Vector2) -> void:
	_rings.append({"center": center, "radius": 15.0, "speed": 170.0, "color": Color("ff8066"), "life": 0.36, "duration": 0.36, "width": 5.5})
	_rings.append({"center": center, "radius": 24.0, "speed": 120.0, "color": Color("ffd2b8"), "life": 0.28, "duration": 0.28, "width": 2.0})
	for index: int in 6:
		var angle: float = TAU * float(index) / 6.0
		var tangent := Vector2.RIGHT.rotated(angle)
		_lines.append({
			"points": PackedVector2Array([center + tangent * 8.0, center + tangent * 34.0]),
			"color": Color("ff9a76", 0.88),
			"life": 0.18 + index * 0.015,
			"duration": 0.28,
			"width": 2.2,
		})
	spawn_fragments(center, Color("ff8066"), 10, 72.0)


func show_fireball_explosion(center: Vector2) -> void:
	_rings.append({"center": center, "radius": 4.0, "speed": 210.0, "color": Color("ff7b2e"), "life": 0.38, "duration": 0.38, "width": 6.5})
	_rings.append({"center": center, "radius": 9.0, "speed": 135.0, "color": Color("ffd166"), "life": 0.31, "duration": 0.31, "width": 3.0})
	for index: int in 8:
		var direction := Vector2.RIGHT.rotated(TAU * float(index) / 8.0)
		_lines.append({
			"points": PackedVector2Array([center + direction * 6.0, center + direction * 30.0]),
			"color": Color("ffb347"),
			"life": 0.20,
			"duration": 0.20,
			"width": 2.2,
		})
	spawn_fragments(center, Color("ff9f43"), 18, 105.0)


func show_shield_charge(from: Vector2, to: Vector2) -> void:
	var direction: Vector2 = from.direction_to(to)
	var normal := Vector2(-direction.y, direction.x)
	_lines.append({"points": PackedVector2Array([from, to]), "color": Color("8be0f1"), "life": 0.40, "duration": 0.40, "width": 8.0})
	_lines.append({"points": PackedVector2Array([from + normal * 7.0, to + normal * 7.0]), "color": Color("d2f7ff", 0.72), "life": 0.28, "duration": 0.28, "width": 2.0})
	_lines.append({"points": PackedVector2Array([from - normal * 7.0, to - normal * 7.0]), "color": Color("d2f7ff", 0.72), "life": 0.28, "duration": 0.28, "width": 2.0})
	_rings.append({"center": to, "radius": 5.0, "speed": 135.0, "color": Color("b8f0ff"), "life": 0.38, "duration": 0.38, "width": 4.5})
	spawn_fragments(to, Color("8be0f1"), 10, 68.0)


func show_chain_lightning(points: PackedVector2Array) -> void:
	if points.size() < 2:
		return
	var jagged := PackedVector2Array([points[0]])
	for index: int in range(points.size() - 1):
		var start: Vector2 = points[index]
		var finish: Vector2 = points[index + 1]
		var direction: Vector2 = start.direction_to(finish)
		var normal := Vector2(-direction.y, direction.x)
		jagged.append(start.lerp(finish, 0.33) + normal * (6.0 if index % 2 == 0 else -6.0))
		jagged.append(start.lerp(finish, 0.66) - normal * (5.0 if index % 2 == 0 else -5.0))
		jagged.append(finish)
	_lines.append({"points": jagged, "color": Color("c9a7ff"), "life": 0.34, "duration": 0.34, "width": 4.2})
	_lines.append({"points": jagged, "color": Color("f0e2ff", 0.72), "life": 0.18, "duration": 0.18, "width": 1.4})
	for point: Vector2 in points:
		spawn_fragments(point, Color("b98cff"), 4, 42.0)


func show_multi_slash(center: Vector2) -> void:
	_rings.append({"center": center, "radius": 9.0, "speed": 70.0, "color": Color("ff72b6", 0.62), "life": 0.34, "duration": 0.34, "width": 1.4})
	for index: int in 9:
		var offset: Vector2 = Vector2(randf_range(-30, 30), randf_range(-21, 21))
		var slash_dir := Vector2(15, 9).rotated(randf_range(-0.55, 0.55))
		_lines.append({
			"points": PackedVector2Array([center + offset - slash_dir, center + offset + slash_dir]),
			"color": Color("ff72b6") if index % 2 == 0 else Color("ffd0ea"),
			"life": 0.22 + index * 0.025,
			"duration": 0.44,
			"width": 2.4 if index % 3 == 0 else 1.7,
		})
	spawn_fragments(center, Color("ff72b6"), 8, 64.0)


func show_holy_nova(center: Vector2) -> void:
	_rings.append({"center": center, "radius": 4.0, "speed": 250.0, "color": Color("fff2a1"), "life": 0.54, "duration": 0.54, "width": 7.0})
	_rings.append({"center": center, "radius": 18.0, "speed": 145.0, "color": Color("ffffff", 0.72), "life": 0.42, "duration": 0.42, "width": 2.4})
	for index: int in 10:
		var direction := Vector2.UP.rotated(TAU * float(index) / 10.0)
		_lines.append({
			"points": PackedVector2Array([center + direction * 10.0, center + direction * 48.0]),
			"color": Color("fff8c9", 0.75),
			"life": 0.30,
			"duration": 0.30,
			"width": 1.8,
		})
	spawn_fragments(center, Color("fff2a1"), 24, 82.0)


func clear_effects() -> void:
	_particles.clear()
	_texts.clear()
	_rings.clear()
	_lines.clear()
	_loot_icons.clear()
	_resource_pickups.clear()
	_flash_alpha = 0.0
	queue_redraw()


func _draw() -> void:
	if _flash_alpha > 0.0:
		draw_rect(Rect2(-128, -128, 2176, 1456), Color(1.0, 1.0, 1.0, _flash_alpha * 0.32), true)
	for particle: Dictionary in _particles:
		var alpha: float = clampf(float(particle["life"]) / float(particle["duration"]), 0.0, 1.0)
		var color: Color = particle["color"]
		color.a *= alpha
		var size: float = float(particle["size"])
		draw_rect(Rect2(Vector2(particle["position"]) - Vector2.ONE * size * 0.5, Vector2.ONE * size), color, true)
	for ring: Dictionary in _rings:
		var alpha: float = clampf(float(ring["life"]) / float(ring["duration"]), 0.0, 1.0)
		var color: Color = ring["color"]
		color.a *= alpha
		draw_arc(Vector2(ring["center"]), float(ring["radius"]), 0.0, TAU, 40, color, float(ring["width"]), true)
	for line_data: Dictionary in _lines:
		var alpha: float = clampf(float(line_data["life"]) / float(line_data["duration"]), 0.0, 1.0)
		var color: Color = line_data["color"]
		color.a *= alpha
		draw_polyline(PackedVector2Array(line_data["points"]), color, float(line_data["width"]), true)
	for pickup: Dictionary in _resource_pickups:
		var position: Vector2 = Vector2(pickup["position"])
		var color: Color = pickup["color"]
		var pulse: float = 1.0 + sin(float(pickup["age"]) * 10.0) * 0.12
		var pickup_kind: String = String(pickup["kind"])
		if pickup_kind == "xp":
			# XP now reads as crystal shards instead of anonymous green dots.
			for shard_index: int in 3:
				var offset := Vector2((shard_index - 1) * 4.0, absf(shard_index - 1) * 2.0)
				var shard_center: Vector2 = position + offset
				var shard_scale: float = pulse * (1.0 if shard_index == 1 else 0.72)
				var points := PackedVector2Array([
					shard_center + Vector2(0, -7) * shard_scale,
					shard_center + Vector2(3.2, -1) * shard_scale,
					shard_center + Vector2(1.5, 5) * shard_scale,
					shard_center + Vector2(-2.8, 2) * shard_scale,
				])
				draw_colored_polygon(points, Color(color, 0.92))
				draw_polyline(points + PackedVector2Array([points[0]]), color.lightened(0.40), 1.0)
			draw_circle(position, 9.0 * pulse, Color(color, 0.07))
		elif pickup_kind == "pet_essence":
			var star := PackedVector2Array()
			for index: int in 10:
				var angle: float = -PI * 0.5 + index * PI / 5.0
				var radius: float = (6.5 if index % 2 == 0 else 2.8) * pulse
				star.append(position + Vector2(cos(angle), sin(angle)) * radius)
			draw_colored_polygon(star, color)
			draw_polyline(star + PackedVector2Array([star[0]]), color.lightened(0.30), 1.2)
			draw_circle(position, 9.0 * pulse, Color(color, 0.10))
		else:
			# Gold is rendered as a tiny coin stack, not a yellow point.
			for coin_index: int in 3:
				var coin_center := position + Vector2((coin_index - 1) * 3.4, -coin_index * 1.7)
				draw_ellipse(coin_center, Vector2(4.2, 2.5) * pulse, Color("d69b22"))
				draw_ellipse(coin_center + Vector2(0, -0.6), Vector2(3.2, 1.7) * pulse, Color("ffd65a"))
				draw_circle(coin_center + Vector2(0, -0.7), 0.9 * pulse, Color("8f6113"))

	for loot_icon: Dictionary in _loot_icons:
		var life: float = float(loot_icon["life"])
		var duration: float = float(loot_icon["duration"])
		var alpha: float = clampf(life / duration, 0.0, 1.0)
		var progress: float = 1.0 - alpha
		var rarity_index: int = int(loot_icon.get("rarity_index", 0))
		var position: Vector2 = Vector2(loot_icon["position"]) + Vector2(0, -6.0 - sin(progress * PI) * 7.0)
		var rarity_color: Color = loot_icon["color"]
		var beam_height: float = float(loot_icon.get("beam_height", 24.0))
		var beam_width: float = float(loot_icon.get("beam_width", 2.0))
		var pulse: float = 0.82 + sin(Time.get_ticks_msec() * 0.012 + rarity_index) * 0.18
		var beam_color := Color(rarity_color, alpha * (0.24 + rarity_index * 0.08) * pulse)
		draw_rect(Rect2(position + Vector2(-beam_width * 0.5, -beam_height), Vector2(beam_width, beam_height + 2.0)), beam_color, true)
		if rarity_index >= 2:
			draw_rect(Rect2(position + Vector2(-beam_width * 1.8, -beam_height * 0.72), Vector2(beam_width * 3.6, beam_height * 0.72)), Color(rarity_color, alpha * 0.06), true)
		if rarity_index >= 4:
			draw_rect(Rect2(position + Vector2(-beam_width * 3.0, -beam_height * 0.48), Vector2(beam_width * 6.0, beam_height * 0.48)), Color(rarity_color, alpha * 0.04), true)
		draw_circle(position, 9.0 + rarity_index * 1.4, Color(rarity_color, alpha * (0.10 + rarity_index * 0.025)))
		var icon_size: float = 18.0 + minf(4.0, float(rarity_index))
		var icon_index: int = int(loot_icon.get("icon_index", -1))
		if icon_index >= 0 and icon_index < 30:
			var cell_size := Vector2(float(ITEM_BASE_ATLAS.get_width()) / 6.0, float(ITEM_BASE_ATLAS.get_height()) / 5.0)
			var atlas_cell := Vector2i(icon_index % 6, floori(float(icon_index) / 6.0))
			var source := Rect2(Vector2(atlas_cell) * cell_size, cell_size)
			draw_texture_rect_region(
				ITEM_BASE_ATLAS,
				Rect2(position - Vector2.ONE * icon_size * 0.5, Vector2.ONE * icon_size),
				source,
				Color(1, 1, 1, alpha)
			)
		else:
			_draw_loot_symbol(
				position,
				String(loot_icon.get("slot", "")),
				String(loot_icon.get("base_id", "")),
				rarity_color,
				alpha,
				icon_size
			)
	for text_data: Dictionary in _texts:
		var alpha: float = clampf(float(text_data["life"]) / float(text_data["duration"]), 0.0, 1.0)
		var color: Color = text_data["color"]
		color.a *= alpha
		draw_string(ThemeDB.fallback_font, Vector2(text_data["position"]), String(text_data["text"]), HORIZONTAL_ALIGNMENT_LEFT, -1, int(text_data["size"]), color)


func _draw_loot_symbol(position: Vector2, slot: String, base_id: String, rarity_color: Color, alpha: float, icon_size: float) -> void:
	var accent: Color = Color("dbe5ef")
	if base_id.contains("dragon") or base_id.contains("ruby"):
		accent = Color("ff654f")
	elif base_id.contains("magic"):
		accent = Color("b076ff")
	elif base_id.contains("divine") or base_id.contains("gold") or base_id.contains("gale"):
		accent = Color("ffd86b")
	elif base_id.contains("diamond") or base_id.contains("crystal") or base_id.contains("mithril"):
		accent = Color("8eeaff")
	elif base_id.contains("leather") or base_id.contains("sandals"):
		accent = Color("b98255")
	accent.a = alpha
	var scale: float = icon_size / 20.0
	match slot:
		"weapon":
			if base_id.contains("axe"):
				draw_line(position + Vector2(-2, 7) * scale, position + Vector2(2, -8) * scale, Color("8b5a2b", alpha), 2.2 * scale)
				draw_colored_polygon(PackedVector2Array([
					position + Vector2(1, -8) * scale,
					position + Vector2(9, -6) * scale,
					position + Vector2(7, 1) * scale,
					position + Vector2(0, -1) * scale,
				]), accent)
			else:
				draw_line(position + Vector2(0, 8) * scale, position + Vector2(0, -9) * scale, accent, 3.0 * scale)
				draw_line(position + Vector2(-5, 5) * scale, position + Vector2(5, 5) * scale, Color(rarity_color, alpha), 2.0 * scale)
		"helmet":
			draw_arc(position, 8.0 * scale, PI, TAU, 16, accent, 3.0 * scale, true)
			draw_line(position + Vector2(-8, 1) * scale, position + Vector2(8, 1) * scale, accent, 3.0 * scale)
		"armor":
			draw_rect(Rect2(position + Vector2(-7, -8) * scale, Vector2(14, 16) * scale), Color(accent, alpha * 0.9), true)
			draw_rect(Rect2(position + Vector2(-3, -8) * scale, Vector2(6, 16) * scale), Color(accent.lightened(0.15), alpha), true)
		"gloves":
			draw_rect(Rect2(position + Vector2(-6, -7) * scale, Vector2(11, 14) * scale), accent, true)
			for i: int in 4:
				draw_line(position + Vector2(-6 + i * 3, -7) * scale, position + Vector2(-6 + i * 3, -10) * scale, accent, 1.5 * scale)
		"boots":
			draw_rect(Rect2(position + Vector2(-8, -8) * scale, Vector2(6, 13) * scale), accent, true)
			draw_rect(Rect2(position + Vector2(2, -8) * scale, Vector2(6, 13) * scale), accent, true)
			draw_line(position + Vector2(-8, 5) * scale, position + Vector2(-1, 7) * scale, accent, 3.0 * scale)
			draw_line(position + Vector2(2, 5) * scale, position + Vector2(9, 7) * scale, accent, 3.0 * scale)
		"ring":
			draw_arc(position, 7.0 * scale, 0.0, TAU, 18, accent, 3.0 * scale, true)
			draw_circle(position + Vector2(0, -7) * scale, 2.5 * scale, Color(rarity_color, alpha))
		"amulet":
			draw_arc(position + Vector2(0, -2) * scale, 8.0 * scale, PI * 0.15, PI * 0.85, 16, Color(rarity_color, alpha), 1.5 * scale, true)
			draw_colored_polygon(PackedVector2Array([
				position + Vector2(-4, -1) * scale,
				position + Vector2(0, -8) * scale,
				position + Vector2(4, -1) * scale,
				position + Vector2(0, 7) * scale,
			]), accent)
		_:
			draw_circle(position, 5.0 * scale, accent)
	draw_circle(position, 10.0 * scale, Color(rarity_color, alpha * 0.10))


func draw_ellipse(center: Vector2, radii: Vector2, color: Color) -> void:
	var points := PackedVector2Array()
	for index: int in 20:
		var angle: float = TAU * float(index) / 20.0
		points.append(center + Vector2(cos(angle) * radii.x, sin(angle) * radii.y))
	draw_colored_polygon(points, color)
