extends Node2D
class_name PlayerAvatar

const CLASS_ATLAS: Texture2D = preload("res://assets/sprites/class_atlas_alpha.png")
const CLASS_REGIONS: Dictionary = {
	"warrior": Vector2i(0, 0), "mage": Vector2i(1, 0), "knight": Vector2i(2, 0),
	"sage": Vector2i(0, 1), "assassin": Vector2i(1, 1), "saint": Vector2i(2, 1),
}

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
	var bob: float = -1.0 if fmod(pulse, 0.8) < 0.4 else 0.0
	draw_ellipse(Vector2(0, 10), Vector2(14, 5), Color(0, 0, 0, 0.58))
	var glow_alpha: float = 0.10 + sin(pulse * 3.0) * 0.03
	draw_circle(Vector2.ZERO, 17.0, Color(body_color, glow_alpha))
	draw_arc(Vector2.ZERO, 15.0, 0.15, PI - 0.15, 18, Color(body_color, 0.72), 1.0)
	var atlas_cell: Vector2i = CLASS_REGIONS.get(class_id, Vector2i.ZERO)
	var source := Rect2(atlas_cell.x * 512, atlas_cell.y * 512, 512, 512)
	draw_texture_rect_region(CLASS_ATLAS, Rect2(-23, -29 + bob, 46, 46), source)


func draw_ellipse(center: Vector2, radius: Vector2, color: Color) -> void:
	var points := PackedVector2Array()
	for index: int in 16:
		var angle: float = TAU * index / 16.0
		points.append(center + Vector2(cos(angle) * radius.x, sin(angle) * radius.y))
	draw_colored_polygon(points, color)
