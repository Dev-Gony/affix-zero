extends Control
class_name BossStatusBar

var boss_name: String = ""
var hp_ratio: float = 1.0
var active: bool = false
var pulse: float = 0.0


func set_boss(name: String, ratio: float, visible_state: bool) -> void:
	boss_name = name
	hp_ratio = clampf(ratio, 0.0, 1.0)
	active = visible_state
	visible = active
	queue_redraw()


func _process(delta: float) -> void:
	if not active:
		return
	pulse += delta
	queue_redraw()


func _draw() -> void:
	if not active:
		return
	var panel := Rect2(Vector2.ZERO, size)
	draw_rect(panel, Color(0.02, 0.025, 0.035, 0.95), true)
	draw_rect(panel, Color("8f2f3d"), false, 2.0)
	draw_string(ThemeDB.fallback_font, Vector2(10, 15), "BOSS  " + boss_name, HORIZONTAL_ALIGNMENT_LEFT, -1, 9, Color("ffbac2"))

	var bar_rect := Rect2(10, 22, size.x - 20, 9)
	draw_rect(bar_rect, Color("260b12"), true)
	var fill := Rect2(bar_rect.position, Vector2(bar_rect.size.x * hp_ratio, bar_rect.size.y))
	draw_rect(fill, Color("d94655"), true)

	var pulse_alpha: float = 0.18 + sin(pulse * 4.0) * 0.06
	if hp_ratio <= 0.30:
		draw_rect(panel.grow(-2), Color("ff5b67", pulse_alpha), false, 1.0)

	draw_string(
		ThemeDB.fallback_font,
		Vector2(size.x - 58, 15),
		"%d%%" % roundi(hp_ratio * 100.0),
		HORIZONTAL_ALIGNMENT_RIGHT,
		48,
		8,
		Color("f4d9dc")
	)
