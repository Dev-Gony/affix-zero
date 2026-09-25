extends Node2D
class_name EffectLayer

signal resource_collected(kind: String, amount: int)

const ITEM_BASE_ATLAS: Texture2D = preload("res://assets/sprites/item_base_atlas_v2.png")
const FLASH_WORLD_RECT := Rect2(-512, -512, 3072, 2304)

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
	var loot_delta: float = delta / maxf(1.0, GameManager.speed_multiplier)
	for loot_icon: Dictionary in _loot_icons:
		loot_icon["life"] = float(loot_icon["life"]) - loot_delta
		loot_icon["age"] = float(loot_icon.get("age", 0.0)) + loot_delta
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


func set_pickup_target(world_position: Vector2) -> void:
	_pickup_target_position = world_position


func spawn_resource_pickup(world_position: Vector2, kind: String, amount: int) -> void:
	var color := Color("61e58b") if kind == "xp" else Color("ffd45c")
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


static func loot_visual_profile(rarity_index: int, special_reward: bool = false) -> Dictionary:
	var tier: int = maxi(0, rarity_index)
	var beam_height: float = 0.0 if tier == 0 else minf(118.0, 18.0 + float(tier - 1) * 15.0)
	var beam_width: float = 0.0 if tier == 0 else minf(9.0, 1.5 + float(tier) * 0.9)
	var duration: float = minf(2.0, 0.90 + float(tier) * 0.13)
	var ring_count: int = 0 if tier < 2 else mini(3, tier - 1)
	var flash_alpha: float = 0.0 if tier < 4 else minf(0.42, 0.16 + float(tier - 4) * 0.10)
	var fragment_count: int = 2 + tier * 3
	if special_reward:
		duration += 0.16
		ring_count = mini(4, ring_count + 1)
		fragment_count += 4
	return {
		"beam_height": beam_height,
		"beam_width": beam_width,
		"glow_radius": minf(28.0, 8.0 + float(tier) * 2.5),
		"duration": duration,
		"ring_count": ring_count,
		"flash_alpha": flash_alpha,
		"fragment_count": fragment_count,
		"label_size": mini(12, 8 + tier / 2),
	}


static func loot_pickup_delay_for_rarity(rarity_index: int) -> float:
	var tier: int = maxi(0, rarity_index)
	if tier <= 0:
		return 0.18
	if tier == 1:
		return 0.26
	return minf(0.90, 0.46 + float(tier - 2) * 0.11)


func show_drop(world_position: Vector2, item: Dictionary) -> void:
	var color := Color.from_string(String(item.get("rarity_color", "ffffff")), Color.WHITE)
	var rarity_index: int = int(item.get("rarity_index", 0))
	var special_reward: bool = bool(item.get("boss_reward", false)) or bool(item.get("elite_reward", false))
	var profile: Dictionary = loot_visual_profile(rarity_index, special_reward)
	var duration: float = float(profile.get("duration", 1.0))
	_loot_icons.append({
		"item_id": String(item.get("id", "")),
		"position": world_position,
		"icon_index": int(item.get("icon_index", -1)),
		"color": color,
		"label": "[%s] %s" % [String(item.get("rarity_name", "")), String(item.get("name", "아이템"))],
		"life": duration,
		"duration": duration,
		"age": 0.0,
		"beam_height": float(profile.get("beam_height", 0.0)),
		"beam_width": float(profile.get("beam_width", 0.0)),
		"glow_radius": float(profile.get("glow_radius", 8.0)),
		"ring_count": int(profile.get("ring_count", 0)),
		"label_size": int(profile.get("label_size", 8)),
		"special_reward": special_reward,
	})
	var flash_alpha: float = float(profile.get("flash_alpha", 0.0))
	if flash_alpha > 0.0:
		_flash_alpha = maxf(_flash_alpha, flash_alpha)
	spawn_fragments(world_position, color, int(profile.get("fragment_count", 2)), 58.0 + rarity_index * 9.0)


func show_pickup(from: Vector2, to: Vector2, item: Dictionary) -> void:
	var color := Color.from_string(String(item.get("rarity_color", "ffffff")), Color.WHITE)
	var item_id: String = String(item.get("id", ""))
	for loot_icon: Dictionary in _loot_icons:
		if not item_id.is_empty() and String(loot_icon.get("item_id", "")) == item_id:
			loot_icon["life"] = minf(float(loot_icon.get("life", 0.0)), 0.28)
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


func show_attack(from: Vector2, to: Vector2, critical: bool) -> void:
	var direction: Vector2 = from.direction_to(to)
	var tangent := Vector2(-direction.y, direction.x)
	var color := Color("ffd84d") if critical else Color("dbeafe")
	_lines.append({"points": PackedVector2Array([from + tangent * 6.0, from + direction * 17.0, from + direction * 25.0 - tangent * 7.0]), "color": color, "life": 0.16, "duration": 0.16, "width": 2.0})
	_lines.append({"points": PackedVector2Array([to - direction * 14.0 - tangent * 5.0, to + direction * 7.0 + tangent * 5.0]), "color": color, "life": 0.13, "duration": 0.13, "width": 2.5})
	spawn_fragments(to, color, 3 if not critical else 6, 48.0)


func show_enemy_attack(from: Vector2, to: Vector2, ranged: bool = false) -> void:
	var direction: Vector2 = from.direction_to(to)
	var tangent := Vector2(-direction.y, direction.x)
	if ranged:
		_lines.append({"points": PackedVector2Array([from, to]), "color": Color("b978ff"), "life": 0.24, "duration": 0.24, "width": 2.0})
		spawn_fragments(to, Color("b978ff"), 5, 42.0)
	else:
		_lines.append({"points": PackedVector2Array([to - direction * 8.0 - tangent * 7.0, to + direction * 5.0 + tangent * 7.0]), "color": Color("ff5b61"), "life": 0.16, "duration": 0.16, "width": 3.0})


func show_melee_spin(center: Vector2) -> void:
	_rings.append({"center": center, "radius": 18.0, "speed": 150.0, "color": Color("ff8066"), "life": 0.34, "duration": 0.34, "width": 5.0})


func show_fireball_explosion(center: Vector2) -> void:
	_rings.append({"center": center, "radius": 4.0, "speed": 180.0, "color": Color("ff7b2e"), "life": 0.35, "duration": 0.35, "width": 6.0})
	spawn_fragments(center, Color("ff9f43"), 12, 90.0)


func show_shield_charge(from: Vector2, to: Vector2) -> void:
	_lines.append({"points": PackedVector2Array([from, to]), "color": Color("8be0f1"), "life": 0.38, "duration": 0.38, "width": 8.0})
	_rings.append({"center": to, "radius": 5.0, "speed": 110.0, "color": Color("b8f0ff"), "life": 0.35, "duration": 0.35, "width": 4.0})


func show_chain_lightning(points: PackedVector2Array) -> void:
	_lines.append({"points": points, "color": Color("c9a7ff"), "life": 0.32, "duration": 0.32, "width": 4.0})


func show_multi_slash(center: Vector2) -> void:
	for index: int in 6:
		var offset: Vector2 = Vector2(randf_range(-26, 26), randf_range(-18, 18))
		_lines.append({"points": PackedVector2Array([center + offset - Vector2(13, 8), center + offset + Vector2(13, 8)]), "color": Color("ff72b6"), "life": 0.24 + index * 0.035, "duration": 0.42, "width": 2.0})


func show_holy_nova(center: Vector2) -> void:
	_rings.append({"center": center, "radius": 4.0, "speed": 240.0, "color": Color("fff2a1"), "life": 0.52, "duration": 0.52, "width": 7.0})
	spawn_fragments(center, Color("fff2a1"), 18, 72.0)


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
		draw_rect(FLASH_WORLD_RECT, Color(1.0, 0.76, 0.15, _flash_alpha), true)
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
		if String(pickup["kind"]) == "xp":
			var points := PackedVector2Array([
				position + Vector2(0, -5) * pulse,
				position + Vector2(4, 0) * pulse,
				position + Vector2(0, 5) * pulse,
				position + Vector2(-4, 0) * pulse,
			])
			draw_colored_polygon(points, color)
			draw_polyline(points + PackedVector2Array([points[0]]), color.lightened(0.35), 1.0)
		else:
			draw_circle(position, 4.5 * pulse, color)
			draw_circle(position, 2.0 * pulse, color.lightened(0.30))

	for loot_icon: Dictionary in _loot_icons:
		var life: float = float(loot_icon["life"])
		var duration: float = maxf(0.001, float(loot_icon["duration"]))
		var alpha: float = clampf(life / duration, 0.0, 1.0)
		var age: float = float(loot_icon.get("age", 0.0))
		var pulse: float = 1.0 + sin(age * 7.0) * 0.08
		var position: Vector2 = Vector2(loot_icon["position"]) + Vector2(0, -4.0 + sin(age * 5.0) * 2.0)
		var rarity_color: Color = loot_icon["color"]
		var beam_height: float = float(loot_icon.get("beam_height", 0.0))
		var beam_width: float = float(loot_icon.get("beam_width", 0.0))
		var glow_radius: float = float(loot_icon.get("glow_radius", 8.0)) * pulse
		if beam_height > 0.0 and beam_width > 0.0:
			draw_rect(Rect2(position + Vector2(-beam_width * 1.6, -beam_height), Vector2(beam_width * 3.2, beam_height + 5.0)), Color(rarity_color, alpha * 0.10), true)
			draw_rect(Rect2(position + Vector2(-beam_width * 0.5, -beam_height), Vector2(beam_width, beam_height + 5.0)), Color(rarity_color.lightened(0.18), alpha * 0.54), true)
		draw_circle(position, glow_radius, Color(rarity_color, alpha * 0.14))
		var ring_count: int = int(loot_icon.get("ring_count", 0))
		for ring_index: int in ring_count:
			var ring_radius: float = 12.0 + float(ring_index) * 5.0 + sin(age * 6.0 + ring_index) * 1.5
			draw_arc(position, ring_radius, 0.0, TAU, 32, Color(rarity_color.lightened(0.22), alpha * (0.72 - ring_index * 0.12)), 1.4 + ring_index * 0.45, true)
		if bool(loot_icon.get("special_reward", false)):
			draw_arc(position, glow_radius + 5.0, 0.0, TAU, 36, Color(Color("ffe6a3"), alpha * 0.72), 1.5, true)
		var icon_index: int = int(loot_icon["icon_index"])
		if icon_index >= 0 and icon_index < 30:
			var cell_size := Vector2(float(ITEM_BASE_ATLAS.get_width()) / 6.0, float(ITEM_BASE_ATLAS.get_height()) / 5.0)
			var atlas_cell := Vector2i(icon_index % 6, floori(float(icon_index) / 6.0))
			var source := Rect2(Vector2(atlas_cell) * cell_size, cell_size)
			var icon_size: float = 18.0 + minf(4.0, float(ring_count))
			draw_texture_rect_region(ITEM_BASE_ATLAS, Rect2(position - Vector2.ONE * icon_size * 0.5, Vector2.ONE * icon_size), source, Color(1, 1, 1, alpha))
		var label_position: Vector2 = position + Vector2(-34.0, -26.0)
		draw_string(ThemeDB.fallback_font, label_position, String(loot_icon.get("label", "")), HORIZONTAL_ALIGNMENT_LEFT, -1, int(loot_icon.get("label_size", 8)), Color(rarity_color.lightened(0.18), alpha))
	for text_data: Dictionary in _texts:
		var alpha: float = clampf(float(text_data["life"]) / float(text_data["duration"]), 0.0, 1.0)
		var color: Color = text_data["color"]
		color.a *= alpha
		draw_string(ThemeDB.fallback_font, Vector2(text_data["position"]), String(text_data["text"]), HORIZONTAL_ALIGNMENT_LEFT, -1, int(text_data["size"]), color)
