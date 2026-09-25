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
	var radius: float = minf(size.x, size.y) * 0.37
	draw_circle(center, radius, Color("08111b"))
	if glow:
		draw_circle(center, radius * 0.92, Color(pet_color, 0.12))
		draw_arc(center, radius * 0.94, 0.0, TAU, 28, Color(pet_color, 0.45), 1.0)
	var scale: float = maxf(0.72, minf(size.x, size.y) / 58.0)
	draw_set_transform(center, 0.0, Vector2.ONE * scale)
	match pet_id:
		"spirit_fox": _draw_fox()
		"ember_drake": _draw_drake()
		"ghost_slime": _draw_slime()
		"stone_golem": _draw_golem()
		"night_bat": _draw_bat()
		"meadow_fairy": _draw_fairy()
		_: draw_circle(Vector2.ZERO, 8.0, pet_color)
	draw_set_transform(Vector2.ZERO, 0.0, Vector2.ONE)


func _draw_fox() -> void:
	var fur := pet_color
	var dark := fur.darkened(0.35)
	var light := fur.lightened(0.28)
	# Three spirit tails behind the body.
	for index: int in 3:
		var tail_y: float = 7.0 + index * 2.0
		draw_arc(Vector2(-7, tail_y), 14.0 + index * 2.0, -1.05 + index * 0.26, 0.10 + index * 0.28, 14, Color(fur, 0.62), 3.0)
	# Body and chest.
	draw_ellipse(Vector2(0, 6), Vector2(8.5, 6.5), dark)
	draw_circle(Vector2(0, -2), 8.8, fur)
	draw_colored_polygon(PackedVector2Array([Vector2(-7,-7),Vector2(-5,-17),Vector2(-1,-8)]), fur.lightened(0.08))
	draw_colored_polygon(PackedVector2Array([Vector2(7,-7),Vector2(5,-17),Vector2(1,-8)]), fur.lightened(0.08))
	draw_colored_polygon(PackedVector2Array([Vector2(-5,-8),Vector2(-4,-13),Vector2(-2,-8)]), dark)
	draw_colored_polygon(PackedVector2Array([Vector2(5,-8),Vector2(4,-13),Vector2(2,-8)]), dark)
	draw_ellipse(Vector2(0, 2), Vector2(4.2, 3.2), light)
	draw_circle(Vector2(-3.2, -3), 1.35, Color("eaffff"))
	draw_circle(Vector2(3.2, -3), 1.35, Color("eaffff"))
	draw_circle(Vector2(-3.2, -3), 0.6, Color("13243d"))
	draw_circle(Vector2(3.2, -3), 0.6, Color("13243d"))
	draw_circle(Vector2(0, 0.4), 0.9, dark)
	draw_circle(Vector2(0, -2), 12.0, Color(fur, 0.06))


func _draw_drake() -> void:
	var body := pet_color
	var dark := body.darkened(0.40)
	var hot := Color("ffb13b")
	# Wings.
	draw_colored_polygon(PackedVector2Array([Vector2(-5,1),Vector2(-19,-10),Vector2(-16,3),Vector2(-10,9)]), dark)
	draw_colored_polygon(PackedVector2Array([Vector2(5,1),Vector2(19,-10),Vector2(16,3),Vector2(10,9)]), dark)
	draw_colored_polygon(PackedVector2Array([Vector2(-7,1),Vector2(-16,-6),Vector2(-13,4)]), body)
	draw_colored_polygon(PackedVector2Array([Vector2(7,1),Vector2(16,-6),Vector2(13,4)]), body)
	# Body/head/horns.
	draw_ellipse(Vector2(0, 6), Vector2(8, 7), dark)
	draw_circle(Vector2(0, -3), 8.5, body)
	draw_colored_polygon(PackedVector2Array([Vector2(-5,-9),Vector2(-8,-16),Vector2(-2,-11)]), hot)
	draw_colored_polygon(PackedVector2Array([Vector2(5,-9),Vector2(8,-16),Vector2(2,-11)]), hot)
	draw_circle(Vector2(-3, -4), 1.2, Color("fff3b0"))
	draw_circle(Vector2(3, -4), 1.2, Color("fff3b0"))
	draw_circle(Vector2(-3, -4), 0.55, Color("4a120c"))
	draw_circle(Vector2(3, -4), 0.55, Color("4a120c"))
	draw_colored_polygon(PackedVector2Array([Vector2(-2,2),Vector2(0,5),Vector2(2,2)]), hot)
	draw_arc(Vector2(0, 7), 12.0, 0.05, 0.9, 10, Color(body, 0.72), 2.4)


func _draw_slime() -> void:
	var body := Color(pet_color, 0.90)
	var rim := pet_color.lightened(0.25)
	draw_circle(Vector2(0, 2), 11.0, body)
	draw_rect(Rect2(-10, 2, 20, 8), body, true)
	draw_circle(Vector2(-4, -1), 1.6, Color("e7f4ff"))
	draw_circle(Vector2(4, -1), 1.6, Color("e7f4ff"))
	draw_circle(Vector2(-4, -1), 0.75, Color("101b2b"))
	draw_circle(Vector2(4, -1), 0.75, Color("101b2b"))
	draw_arc(Vector2(0, 1), 4.0, 0.25, PI - 0.25, 10, Color("29425f"), 1.2)
	draw_arc(Vector2(0, 2), 12.0, PI, TAU, 18, Color(rim, 0.72), 1.6)
	draw_circle(Vector2(-4, -6), 3.2, Color("ffffff", 0.13))


func _draw_golem() -> void:
	var stone := pet_color
	var dark := stone.darkened(0.30)
	var rune := Color("66dcff")
	draw_rect(Rect2(-8, -9, 16, 18), stone, true)
	draw_rect(Rect2(-12, -4, 5, 13), dark, true)
	draw_rect(Rect2(7, -4, 5, 13), dark, true)
	draw_rect(Rect2(-7, 9, 6, 5), dark, true)
	draw_rect(Rect2(1, 9, 6, 5), dark, true)
	draw_colored_polygon(PackedVector2Array([Vector2(-7,-9),Vector2(-3,-14),Vector2(0,-9)]), stone.lightened(0.10))
	draw_colored_polygon(PackedVector2Array([Vector2(7,-9),Vector2(3,-14),Vector2(0,-9)]), stone.lightened(0.10))
	draw_rect(Rect2(-4, -3, 8, 4), Color("17222b"), true)
	draw_rect(Rect2(-3, -2, 2, 2), rune, true)
	draw_rect(Rect2(1, -2, 2, 2), rune, true)
	draw_arc(Vector2(0, 4), 4.5, 0.0, TAU, 12, Color(rune, 0.65), 1.2)


func _draw_bat() -> void:
	var body := pet_color
	var wing := body.darkened(0.24)
	draw_colored_polygon(PackedVector2Array([Vector2(-4,0),Vector2(-20,-10),Vector2(-16,1),Vector2(-19,9),Vector2(-7,5)]), wing)
	draw_colored_polygon(PackedVector2Array([Vector2(4,0),Vector2(20,-10),Vector2(16,1),Vector2(19,9),Vector2(7,5)]), wing)
	draw_circle(Vector2(0, 0), 6.5, body)
	draw_colored_polygon(PackedVector2Array([Vector2(-4,-4),Vector2(-7,-11),Vector2(-1,-6)]), body)
	draw_colored_polygon(PackedVector2Array([Vector2(4,-4),Vector2(7,-11),Vector2(1,-6)]), body)
	draw_circle(Vector2(-2.3, -1.5), 1.25, Color("ff6b83"))
	draw_circle(Vector2(2.3, -1.5), 1.25, Color("ff6b83"))
	draw_colored_polygon(PackedVector2Array([Vector2(-2,4),Vector2(0,7),Vector2(2,4)]), Color("e7dbff"))


func _draw_fairy() -> void:
	var body := pet_color
	var glow_color := body.lightened(0.30)
	# Four translucent wings.
	draw_ellipse(Vector2(-8, -4), Vector2(6, 9), Color(glow_color, 0.34))
	draw_ellipse(Vector2(8, -4), Vector2(6, 9), Color(glow_color, 0.34))
	draw_ellipse(Vector2(-7, 5), Vector2(5, 7), Color(glow_color, 0.26))
	draw_ellipse(Vector2(7, 5), Vector2(5, 7), Color(glow_color, 0.26))
	draw_circle(Vector2(0, -3), 5.5, Color("ffe5c7"))
	draw_ellipse(Vector2(0, 5), Vector2(4.5, 7.5), body)
	draw_colored_polygon(PackedVector2Array([Vector2(-6,-7),Vector2(0,-14),Vector2(6,-7),Vector2(3,-2),Vector2(-3,-2)]), body.darkened(0.18))
	draw_circle(Vector2(-2, -3), 0.8, Color("15331f"))
	draw_circle(Vector2(2, -3), 0.8, Color("15331f"))
	draw_circle(Vector2(0, 1), 15.0, Color(body, 0.07))


func draw_ellipse(center: Vector2, radii: Vector2, color: Color) -> void:
	var points := PackedVector2Array()
	for index: int in 24:
		var angle := TAU * float(index) / 24.0
		points.append(center + Vector2(cos(angle) * radii.x, sin(angle) * radii.y))
	draw_colored_polygon(points, color)
