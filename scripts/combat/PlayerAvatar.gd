extends Node2D
class_name PlayerAvatar

var class_id: String = "warrior"
var body_color: Color = Color("dc3d33")
var pulse: float = 0.0


func configure(next_class_id: String, color: Color) -> void:
	class_id = next_class_id
	body_color = color
	visible = true
	queue_redraw()


func _process(delta: float) -> void:
	pulse += delta
	queue_redraw()


func _draw() -> void:
	draw_ellipse(Vector2(0, 9), Vector2(13, 5), Color(0, 0, 0, 0.42))
	var glow_alpha: float = 0.10 + sin(pulse * 3.0) * 0.03
	draw_circle(Vector2.ZERO, 15.0, Color(body_color, glow_alpha))
	draw_rect(Rect2(-8, -9, 16, 18), Color("151522"), true)
	draw_rect(Rect2(-7, -8, 14, 16), body_color, true)
	draw_rect(Rect2(-5, -13, 10, 7), body_color.lightened(0.18), true)
	draw_rect(Rect2(-3, -11, 2, 2), Color.WHITE, true)
	draw_rect(Rect2(2, -11, 2, 2), Color.WHITE, true)
	match class_id:
		"warrior":
			draw_line(Vector2(8, -5), Vector2(15, 5), Color("d9e2ec"), 3.0)
		"mage":
			draw_line(Vector2(9, -7), Vector2(12, 10), Color("8b5e34"), 2.0)
			draw_circle(Vector2(9, -8), 3.0, Color("ff8c42"))
		"knight":
			draw_rect(Rect2(8, -5, 6, 11), Color("9bd4e0"), true)
		"sage":
			draw_line(Vector2(9, -8), Vector2(13, 10), Color("b393e6"), 2.0)
		"assassin":
			draw_line(Vector2(-10, -3), Vector2(-15, 5), Color("eeeeee"), 2.0)
			draw_line(Vector2(10, -3), Vector2(15, 5), Color("eeeeee"), 2.0)
		"saint":
			draw_arc(Vector2(0, -11), 6.0, PI, TAU, 12, Color("ffe16b"), 2.0)


func draw_ellipse(center: Vector2, radius: Vector2, color: Color) -> void:
	var points := PackedVector2Array()
	for index: int in 16:
		var angle: float = TAU * index / 16.0
		points.append(center + Vector2(cos(angle) * radius.x, sin(angle) * radius.y))
	draw_colored_polygon(points, color)
