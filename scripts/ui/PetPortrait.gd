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
	if glow:
		draw_circle(center, minf(size.x, size.y) * 0.28, Color(pet_color, 0.08))
	match pet_id:
		"spirit_fox": _draw_fox(center)
		"ember_drake": _draw_drake(center)
		"ghost_slime": _draw_slime(center)
		"stone_golem": _draw_golem(center)
		"night_bat": _draw_bat(center)
		"meadow_fairy": _draw_fairy(center)
		_: draw_circle(center, 7.0, pet_color)


func _draw_fox(c: Vector2) -> void:
	draw_circle(c, 8.0, pet_color)
	draw_colored_polygon(PackedVector2Array([c+Vector2(-7,-4), c+Vector2(-4,-14), c+Vector2(0,-6)]), pet_color.lightened(0.08))
	draw_colored_polygon(PackedVector2Array([c+Vector2(7,-4), c+Vector2(4,-14), c+Vector2(0,-6)]), pet_color.lightened(0.08))
	for i: int in 3:
		draw_arc(c+Vector2(-3,7), 11.0+i*2.0, -1.2+i*0.65, -0.35+i*0.65, 8, Color(pet_color,0.65), 2.0)


func _draw_drake(c: Vector2) -> void:
	draw_circle(c, 8.0, pet_color)
	draw_colored_polygon(PackedVector2Array([c+Vector2(-6,0),c+Vector2(-18,-9),c+Vector2(-14,8)]),pet_color.darkened(0.10))
	draw_colored_polygon(PackedVector2Array([c+Vector2(6,0),c+Vector2(18,-9),c+Vector2(14,8)]),pet_color.darkened(0.10))
	draw_colored_polygon(PackedVector2Array([c+Vector2(-3,-6),c+Vector2(0,-16),c+Vector2(3,-6)]),pet_color.lightened(0.18))


func _draw_slime(c: Vector2) -> void:
	draw_circle(c+Vector2(0,2), 10.0, Color(pet_color,0.90))
	draw_rect(Rect2(c+Vector2(-10,2),Vector2(20,8)),Color(pet_color,0.90),true)
	draw_circle(c+Vector2(-3,0),1.3,Color("06101b"))
	draw_circle(c+Vector2(3,0),1.3,Color("06101b"))


func _draw_golem(c: Vector2) -> void:
	draw_rect(Rect2(c+Vector2(-8,-9),Vector2(16,18)),pet_color,true)
	draw_rect(Rect2(c+Vector2(-13,-3),Vector2(5,12)),pet_color.darkened(0.12),true)
	draw_rect(Rect2(c+Vector2(8,-3),Vector2(5,12)),pet_color.darkened(0.12),true)
	draw_rect(Rect2(c+Vector2(-3,-2),Vector2(6,3)),Color("70d6ff"),true)


func _draw_bat(c: Vector2) -> void:
	draw_circle(c,6.0,pet_color)
	draw_colored_polygon(PackedVector2Array([c+Vector2(-4,0),c+Vector2(-18,-9),c+Vector2(-13,8)]),pet_color)
	draw_colored_polygon(PackedVector2Array([c+Vector2(4,0),c+Vector2(18,-9),c+Vector2(13,8)]),pet_color)
	draw_circle(c+Vector2(0,-2),1.2,Color("ff6b6b"))


func _draw_fairy(c: Vector2) -> void:
	draw_circle(c,5.0,pet_color)
	draw_circle(c+Vector2(-8,-2),5.0,Color(pet_color.lightened(0.22),0.52))
	draw_circle(c+Vector2(8,-2),5.0,Color(pet_color.lightened(0.22),0.52))
	draw_circle(c,13.0,Color(pet_color,0.07))
