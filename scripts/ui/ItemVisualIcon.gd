extends Control
class_name ItemVisualIcon

var item_data: Dictionary = {}
var placeholder_slot: String = ""
var rarity_color: Color = Color("64748b")
var accent_color: Color = Color("cbd5e1")


func configure(item: Dictionary) -> void:
	item_data = item.duplicate(true)
	placeholder_slot = ""
	rarity_color = Color.from_string(String(item.get("rarity_color", "64748b")), Color("64748b"))
	accent_color = _base_accent(String(item.get("base_id", "")), String(item.get("slot", "weapon")))
	queue_redraw()


func configure_placeholder(slot: String) -> void:
	item_data = {}
	placeholder_slot = slot
	rarity_color = Color("334155")
	accent_color = Color("64748b")
	queue_redraw()


func _draw() -> void:
	var rect := Rect2(Vector2.ZERO, size)
	var center := rect.get_center()
	var radius: float = minf(size.x, size.y) * 0.44
	draw_circle(center, radius, Color(rarity_color, 0.06))
	draw_rect(Rect2(2, 2, size.x - 4, size.y - 4), Color(0.03, 0.04, 0.06, 0.55), true)
	draw_rect(Rect2(2, 2, size.x - 4, size.y - 4), Color(rarity_color, 0.72), false, 1.0)
	if item_data.is_empty():
		_draw_slot_placeholder(center, placeholder_slot)
		return
	var slot: String = String(item_data.get("slot", "weapon"))
	var base_id: String = String(item_data.get("base_id", ""))
	match slot:
		"weapon": _draw_weapon(center, base_id)
		"helmet": _draw_helmet(center, base_id)
		"armor": _draw_armor(center, base_id)
		"gloves": _draw_gloves(center, base_id)
		"boots": _draw_boots(center, base_id)
		"ring": _draw_ring(center, base_id)
		"amulet": _draw_amulet(center, base_id)
		_: _draw_slot_placeholder(center, slot)
	var enhancement: int = int(item_data.get("enhancement_level", 0))
	if enhancement > 0:
		draw_circle(center + Vector2(size.x * 0.30, -size.y * 0.30), 5.0, Color("0a0f16"))
		draw_string(ThemeDB.fallback_font, center + Vector2(size.x * 0.22, -size.y * 0.22), "+%d" % enhancement, HORIZONTAL_ALIGNMENT_LEFT, -1, 6, Color("f8d66d"))


func _draw_weapon(c: Vector2, base_id: String) -> void:
	if base_id.contains("axe"):
		draw_rect(Rect2(c.x - 2, c.y - 10, 4, 22), Color("8b5a2b"), true)
		draw_colored_polygon(PackedVector2Array([
			c + Vector2(-2, -10), c + Vector2(10, -7), c + Vector2(8, 1), c + Vector2(-2, -1)
		]), accent_color)
	elif base_id.contains("dagger"):
		draw_colored_polygon(PackedVector2Array([
			c + Vector2(-2, 9), c + Vector2(-5, -4), c + Vector2(0, -12), c + Vector2(4, -3), c + Vector2(2, 9)
		]), accent_color)
		draw_rect(Rect2(c.x - 6, c.y + 7, 12, 3), Color("d6a84b"), true)
	else:
		var glow := accent_color if not base_id.contains("longsword") else Color("dce6ef")
		draw_colored_polygon(PackedVector2Array([
			c + Vector2(-2, 10), c + Vector2(-4, -9), c + Vector2(0, -14), c + Vector2(4, -9), c + Vector2(2, 10)
		]), glow)
		draw_rect(Rect2(c.x - 7, c.y + 7, 14, 3), Color("d6a84b"), true)
		draw_rect(Rect2(c.x - 2, c.y + 10, 4, 5), Color("7a4a24"), true)
		if base_id.contains("magic"):
			draw_circle(c + Vector2(0, -3), 3.0, Color("9d6cff"))
		elif base_id.contains("divine"):
			draw_circle(c + Vector2(0, -3), 3.0, Color("fff2a1"))


func _draw_helmet(c: Vector2, base_id: String) -> void:
	var shell: Color = accent_color
	draw_circle(c + Vector2(0, -1), 11.0, shell)
	draw_rect(Rect2(c.x - 11, c.y - 1, 22, 11), shell, true)
	draw_rect(Rect2(c.x - 7, c.y + 1, 14, 4), Color("111827"), true)
	draw_rect(Rect2(c.x - 1, c.y + 1, 2, 7), shell.lightened(0.15), true)
	if base_id.contains("dragon"):
		draw_colored_polygon(PackedVector2Array([c+Vector2(-9,-7),c+Vector2(-15,-13),c+Vector2(-6,-10)]),shell)
		draw_colored_polygon(PackedVector2Array([c+Vector2(9,-7),c+Vector2(15,-13),c+Vector2(6,-10)]),shell)


func _draw_armor(c: Vector2, base_id: String) -> void:
	var body := accent_color
	draw_colored_polygon(PackedVector2Array([
		c+Vector2(-11,-9), c+Vector2(-5,-14), c+Vector2(5,-14), c+Vector2(11,-9),
		c+Vector2(9,13), c+Vector2(-9,13)
	]), body)
	draw_rect(Rect2(c.x - 3, c.y - 12, 6, 24), body.lightened(0.12), true)
	draw_rect(Rect2(c.x - 8, c.y - 5, 16, 3), Color(body.darkened(0.35), 0.9), true)
	if base_id.contains("dragonscale"):
		for y: int in 3:
			for x: int in 3:
				draw_circle(c + Vector2(-6 + x*6, -1 + y*5), 1.8, body.lightened(0.2))


func _draw_gloves(c: Vector2, base_id: String) -> void:
	var body := accent_color
	draw_rect(Rect2(c.x - 7, c.y - 10, 12, 18), body, true)
	for i: int in 4:
		draw_rect(Rect2(c.x - 8 + i*4, c.y - 14, 3, 8), body.lightened(float(i)*0.04), true)
	draw_rect(Rect2(c.x - 5, c.y + 6, 12, 5), body.darkened(0.18), true)
	if base_id.contains("dragon"):
		draw_colored_polygon(PackedVector2Array([c+Vector2(4,-8),c+Vector2(13,-4),c+Vector2(5,0)]),body)


func _draw_boots(c: Vector2, base_id: String) -> void:
	var body := accent_color
	draw_rect(Rect2(c.x - 10, c.y - 12, 7, 17), body, true)
	draw_rect(Rect2(c.x + 2, c.y - 12, 7, 17), body, true)
	draw_colored_polygon(PackedVector2Array([c+Vector2(-10,3),c+Vector2(-2,3),c+Vector2(2,10),c+Vector2(-11,10)]),body.darkened(0.10))
	draw_colored_polygon(PackedVector2Array([c+Vector2(2,3),c+Vector2(9,3),c+Vector2(12,10),c+Vector2(0,10)]),body.darkened(0.10))
	if base_id.contains("gale"):
		draw_colored_polygon(PackedVector2Array([c+Vector2(-9,-4),c+Vector2(-17,-9),c+Vector2(-10,1)]),Color("f8e08e"))
		draw_colored_polygon(PackedVector2Array([c+Vector2(8,-4),c+Vector2(17,-9),c+Vector2(10,1)]),Color("f8e08e"))


func _draw_ring(c: Vector2, base_id: String) -> void:
	draw_arc(c, 10.0, 0.0, TAU, 24, accent_color, 5.0, true)
	if base_id.contains("diamond"):
		draw_colored_polygon(PackedVector2Array([c+Vector2(-5,-10),c+Vector2(0,-16),c+Vector2(5,-10),c+Vector2(0,-5)]),Color("9ff7ff"))
	else:
		draw_circle(c + Vector2(0,-10), 4.0, accent_color.lightened(0.22))


func _draw_amulet(c: Vector2, base_id: String) -> void:
	draw_arc(c + Vector2(0,-3), 11.0, PI*0.15, PI*0.85, 20, accent_color.darkened(0.18), 2.0, true)
	if base_id.contains("bone"):
		draw_colored_polygon(PackedVector2Array([c+Vector2(-3,-1),c+Vector2(0,-9),c+Vector2(4,-1),c+Vector2(0,11)]),Color("e8dfc4"))
	elif base_id.contains("ruby"):
		draw_colored_polygon(PackedVector2Array([c+Vector2(-6,-2),c+Vector2(0,-10),c+Vector2(6,-2),c+Vector2(0,10)]),Color("ff4d5a"))
	elif base_id.contains("dragon_tear"):
		draw_colored_polygon(PackedVector2Array([c+Vector2(0,-12),c+Vector2(7,1),c+Vector2(0,11),c+Vector2(-7,1)]),Color("62b7ff"))
	else:
		draw_colored_polygon(PackedVector2Array([c+Vector2(-6,-3),c+Vector2(0,-10),c+Vector2(6,-3),c+Vector2(0,9)]),accent_color)


func _draw_slot_placeholder(c: Vector2, slot: String) -> void:
	var col := Color("4b5563")
	match slot:
		"weapon": draw_rect(Rect2(c.x - 2, c.y - 12, 4, 24), col, true)
		"helmet": draw_arc(c, 10.0, PI, TAU, 16, col, 3.0)
		"armor": draw_rect(Rect2(c.x - 8, c.y - 10, 16, 20), col, false, 2.0)
		"gloves": draw_rect(Rect2(c.x - 6, c.y - 8, 12, 16), col, false, 2.0)
		"boots": draw_rect(Rect2(c.x - 8, c.y - 9, 6, 18), col, false, 2.0)
		"ring": draw_arc(c, 9.0, 0.0, TAU, 20, col, 2.0)
		"amulet": draw_circle(c, 7.0, Color(col, 0.25))
		_: draw_circle(c, 6.0, Color(col, 0.35))


func _base_accent(base_id: String, slot: String) -> Color:
	var exact: Dictionary = {
		"weapon_dagger": Color("d8e2ea"),
		"weapon_longsword": Color("cbd5e1"),
		"weapon_axe": Color("d98b45"),
		"weapon_magic_sword": Color("a96dff"),
		"weapon_divine_sword": Color("ffd86b"),
		"helmet_leather_hat": Color("9a6a48"),
		"helmet_iron_helm": Color("9ca8b7"),
		"helmet_mithril_helm": Color("70d6ff"),
		"helmet_dragon_helm": Color("e3493f"),
		"armor_cloth": Color("8b6da8"),
		"armor_leather": Color("9a6849"),
		"armor_plate": Color("9eabb8"),
		"armor_dragonscale": Color("d9443f"),
		"gloves_cloth": Color("7586a8"),
		"gloves_leather": Color("9c6a48"),
		"gloves_battle": Color("d7a449"),
		"gloves_dragon": Color("e14b42"),
		"boots_sandals": Color("b99462"),
		"boots_leather": Color("8f6245"),
		"boots_swift": Color("61c7e8"),
		"boots_gale": Color("f0cf61"),
		"ring_copper": Color("d67b43"),
		"ring_silver": Color("c8d1dc"),
		"ring_gold": Color("f0be45"),
		"ring_diamond": Color("91ecff"),
		"amulet_bone": Color("e8dfc4"),
		"amulet_crystal": Color("73d7ff"),
		"amulet_ruby": Color("ff5c68"),
		"amulet_dragon_tear": Color("5da9ff"),
	}
	if exact.has(base_id):
		return exact[base_id]
	match slot:
		"weapon": return Color("d8e2ea")
		"helmet", "armor": return Color("9ca8b7")
		"gloves", "boots": return Color("b38a63")
		"ring", "amulet": return Color("d8c36a")
	return Color("cbd5e1")
