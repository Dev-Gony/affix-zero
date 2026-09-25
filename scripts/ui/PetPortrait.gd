extends Control
class_name PetPortrait

var pet_id: String = ""
var pet_color: Color = Color.WHITE
var glow: bool = true


func configure(next_pet_id: String, color: Color) -> void:
	pet_id = next_pet_id
	pet_color = color
	queue_redraw()


func _draw() -> void:
	var center := size * 0.5
	var radius: float = minf(size.x, size.y) * 0.36
	var data: PetData = PetManager.get_pet_data(pet_id)
	var rarity_color: Color = data.rarity_color if data != null else pet_color
	if glow:
		draw_circle(center, radius + 4.0, Color(rarity_color, 0.05))
		draw_arc(center, radius + 2.0, 0.0, TAU, 28, Color(rarity_color, 0.34), 1.2)
		draw_arc(center, radius - 2.0, -0.9, 2.4, 20, Color(pet_color, 0.18), 1.0)
		for index: int in 4:
			var angle: float = TAU * float(index) / 4.0 + 0.35
			var mark: Vector2 = center + Vector2.RIGHT.rotated(angle) * (radius - 3.0)
			draw_circle(mark, 1.4, Color(rarity_color, 0.55))
	draw_circle(center + Vector2(0, radius * 0.48), radius * 0.52, Color(0, 0, 0, 0.42))
	match pet_id:
		"spirit_fox": _draw_fox(center)
		"ember_drake": _draw_drake(center)
		"ghost_slime": _draw_slime(center)
		"stone_golem": _draw_golem(center)
		"night_bat": _draw_bat(center)
		"meadow_fairy": _draw_fairy(center)
		_: draw_circle(center, 7.0, pet_color)


func _draw_fox(c: Vector2) -> void:
	var shadow := pet_color.darkened(0.48)
	var light := pet_color.lightened(0.28)
	draw_colored_polygon(PackedVector2Array([
		c + Vector2(-13, 8), c + Vector2(-9, -8), c + Vector2(0, -15),
		c + Vector2(9, -8), c + Vector2(13, 8), c + Vector2(0, 14),
	]), Color(shadow, 0.96))
	draw_colored_polygon(PackedVector2Array([
		c + Vector2(-9, -6), c + Vector2(-7, -19), c + Vector2(-1, -9)
	]), light)
	draw_colored_polygon(PackedVector2Array([
		c + Vector2(9, -6), c + Vector2(7, -19), c + Vector2(1, -9)
	]), light)
	draw_colored_polygon(PackedVector2Array([
		c + Vector2(-7, 2), c + Vector2(0, -6), c + Vector2(7, 2), c + Vector2(0, 10)
	]), pet_color)
	draw_circle(c + Vector2(-3, -1), 1.4, Color("dff8ff"))
	draw_circle(c + Vector2(3, -1), 1.4, Color("dff8ff"))
	for i: int in 3:
		var tail_center := c + Vector2(-8 + i * 3, 10 + i)
		draw_arc(tail_center, 13.0 + i * 2.0, -1.2 + i * 0.28, 0.35 + i * 0.32, 12, Color(light, 0.72), 2.6)


func _draw_drake(c: Vector2) -> void:
	var dark := pet_color.darkened(0.50)
	var hot := pet_color.lightened(0.24)
	draw_colored_polygon(PackedVector2Array([
		c+Vector2(-7,4), c+Vector2(-24,-13), c+Vector2(-19,8), c+Vector2(-9,12)
	]), dark)
	draw_colored_polygon(PackedVector2Array([
		c+Vector2(7,4), c+Vector2(24,-13), c+Vector2(19,8), c+Vector2(9,12)
	]), dark)
	draw_colored_polygon(PackedVector2Array([
		c+Vector2(-9,-4), c+Vector2(0,-16), c+Vector2(9,-4), c+Vector2(7,12), c+Vector2(-7,12)
	]), pet_color)
	draw_colored_polygon(PackedVector2Array([
		c+Vector2(-6,-10), c+Vector2(-2,-23), c+Vector2(1,-11)
	]), hot)
	draw_colored_polygon(PackedVector2Array([
		c+Vector2(2,-11), c+Vector2(7,-20), c+Vector2(7,-8)
	]), hot)
	draw_circle(c + Vector2(-3,-3), 1.5, Color("fff0a0"))
	draw_circle(c + Vector2(3,-3), 1.5, Color("fff0a0"))
	draw_line(c+Vector2(-4,8), c+Vector2(0,14), Color("ffca68"), 2.0)
	draw_line(c+Vector2(4,8), c+Vector2(0,14), Color("ffca68"), 2.0)


func _draw_slime(c: Vector2) -> void:
	var body := Color(pet_color, 0.88)
	draw_circle(c + Vector2(0, 2), 14.0, Color(body.darkened(0.26), 0.68))
	draw_circle(c + Vector2(0, -1), 12.0, body)
	draw_rect(Rect2(c + Vector2(-12, 0), Vector2(24, 10)), body, true)
	draw_circle(c + Vector2(-5, 0), 2.0, Color("dff5ff"))
	draw_circle(c + Vector2(5, 0), 2.0, Color("dff5ff"))
	draw_circle(c + Vector2(-5, 0), 0.9, Color("07111f"))
	draw_circle(c + Vector2(5, 0), 0.9, Color("07111f"))
	draw_arc(c + Vector2(0, 4), 5.0, 0.2, PI-0.2, 10, Color(pet_color.darkened(0.55)), 1.3)
	draw_circle(c + Vector2(-7,-7), 3.0, Color(pet_color.lightened(0.35), 0.25))


func _draw_golem(c: Vector2) -> void:
	var stone := pet_color.darkened(0.22)
	var edge := pet_color.lightened(0.16)
	draw_rect(Rect2(c+Vector2(-11,-11),Vector2(22,22)),stone,true)
	draw_colored_polygon(PackedVector2Array([
		c+Vector2(-11,-11),c+Vector2(-5,-17),c+Vector2(6,-15),c+Vector2(11,-11)
	]), edge)
	draw_rect(Rect2(c+Vector2(-19,-6),Vector2(8,18)),stone.darkened(0.14),true)
	draw_rect(Rect2(c+Vector2(11,-6),Vector2(8,18)),stone.darkened(0.14),true)
	draw_rect(Rect2(c+Vector2(-6,-5),Vector2(12,8)),Color("15202a"),true)
	draw_circle(c+Vector2(0,-1),4.0,Color("42d8ff",0.45))
	draw_circle(c+Vector2(0,-1),2.0,Color("b8f5ff"))
	draw_line(c+Vector2(-8,9),c+Vector2(-2,4),edge,1.5)
	draw_line(c+Vector2(8,9),c+Vector2(2,4),edge,1.5)


func _draw_bat(c: Vector2) -> void:
	var dark := pet_color.darkened(0.45)
	var wing := pet_color.darkened(0.16)
	draw_colored_polygon(PackedVector2Array([
		c+Vector2(-4,0),c+Vector2(-25,-13),c+Vector2(-19,2),c+Vector2(-23,14),c+Vector2(-8,8)
	]),wing)
	draw_colored_polygon(PackedVector2Array([
		c+Vector2(4,0),c+Vector2(25,-13),c+Vector2(19,2),c+Vector2(23,14),c+Vector2(8,8)
	]),wing)
	draw_colored_polygon(PackedVector2Array([
		c+Vector2(-7,-7),c+Vector2(0,-15),c+Vector2(7,-7),c+Vector2(8,9),c+Vector2(0,14),c+Vector2(-8,9)
	]),dark)
	draw_colored_polygon(PackedVector2Array([c+Vector2(-5,-9),c+Vector2(-9,-18),c+Vector2(-1,-11)]),pet_color)
	draw_colored_polygon(PackedVector2Array([c+Vector2(5,-9),c+Vector2(9,-18),c+Vector2(1,-11)]),pet_color)
	draw_circle(c+Vector2(-3,-4),1.6,Color("ff555d"))
	draw_circle(c+Vector2(3,-4),1.6,Color("ff555d"))


func _draw_fairy(c: Vector2) -> void:
	var wing := pet_color.lightened(0.28)
	draw_circle(c+Vector2(-11,-2),8.0,Color(wing,0.28))
	draw_circle(c+Vector2(11,-2),8.0,Color(wing,0.28))
	draw_circle(c+Vector2(-8,8),6.0,Color(wing,0.20))
	draw_circle(c+Vector2(8,8),6.0,Color(wing,0.20))
	draw_colored_polygon(PackedVector2Array([
		c+Vector2(-5,-7),c+Vector2(0,-14),c+Vector2(5,-7),c+Vector2(4,9),c+Vector2(0,15),c+Vector2(-4,9)
	]),pet_color.darkened(0.16))
	draw_circle(c+Vector2(0,-5),4.0,pet_color.lightened(0.20))
	draw_circle(c+Vector2(-1,-6),1.1,Color("ffffff"))
	draw_line(c+Vector2(0,8),c+Vector2(-4,14),Color("d7ffd9",0.7),1.3)
	draw_line(c+Vector2(0,8),c+Vector2(4,14),Color("d7ffd9",0.7),1.3)
