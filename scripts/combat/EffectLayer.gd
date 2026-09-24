extends Node2D
class_name EffectLayer

const ITEM_BASE_ATLAS: Texture2D = preload("res://assets/sprites/item_base_atlas_v2.png")

var _particles: Array[Dictionary] = []
var _texts: Array[Dictionary] = []
var _rings: Array[Dictionary] = []
var _lines: Array[Dictionary] = []
var _loot_icons: Array[Dictionary] = []
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


func show_drop(world_position: Vector2, item: Dictionary) -> void:
	var color := Color.from_string(String(item.get("rarity_color", "ffffff")), Color.WHITE)
	_texts.append({"position": world_position + Vector2(-30, -18), "text": String(item.get("name", "아이템")), "color": color, "life": 1.4, "duration": 1.4, "size": 11})
	var icon_index: int = int(item.get("icon_index", -1))
	if icon_index >= 0 and icon_index < 30:
		_loot_icons.append({
			"position": world_position,
			"icon_index": icon_index,
			"color": color,
			"life": 1.2,
			"duration": 1.2,
		})
	if String(item.get("rarity_id", "")) == "legend":
		_flash_alpha = 0.58
		spawn_fragments(Vector2(320, 105), Color("ffd700"), 28, 105.0)


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
	_flash_alpha = 0.0
	queue_redraw()


func _draw() -> void:
	if _flash_alpha > 0.0:
		draw_rect(Rect2(0, 0, 640, 356), Color(1.0, 0.76, 0.15, _flash_alpha), true)
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
	for loot_icon: Dictionary in _loot_icons:
		var life: float = float(loot_icon["life"])
		var duration: float = float(loot_icon["duration"])
		var alpha: float = clampf(life / duration, 0.0, 1.0)
		var progress: float = 1.0 - alpha
		var position: Vector2 = Vector2(loot_icon["position"]) + Vector2(0, -6.0 - sin(progress * PI) * 7.0)
		var rarity_color: Color = loot_icon["color"]
		rarity_color.a = alpha * 0.45
		draw_rect(Rect2(position + Vector2(-1, -29), Vector2(2, 31)), rarity_color, true)
		draw_circle(position, 11.0, Color(rarity_color, alpha * 0.16))
		var icon_index: int = int(loot_icon["icon_index"])
		var cell_size := Vector2(float(ITEM_BASE_ATLAS.get_width()) / 6.0, float(ITEM_BASE_ATLAS.get_height()) / 5.0)
		var atlas_cell := Vector2i(icon_index % 6, floori(float(icon_index) / 6.0))
		var source := Rect2(Vector2(atlas_cell) * cell_size, cell_size)
		draw_texture_rect_region(ITEM_BASE_ATLAS, Rect2(position - Vector2(9, 9), Vector2(18, 18)), source, Color(1, 1, 1, alpha))
	for text_data: Dictionary in _texts:
		var alpha: float = clampf(float(text_data["life"]) / float(text_data["duration"]), 0.0, 1.0)
		var color: Color = text_data["color"]
		color.a *= alpha
		draw_string(ThemeDB.fallback_font, Vector2(text_data["position"]), String(text_data["text"]), HORIZONTAL_ALIGNMENT_LEFT, -1, int(text_data["size"]), color)
