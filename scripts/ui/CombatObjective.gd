extends Control
class_name CombatObjective

var floor_number: int = 1
var kills: int = 0
var kill_goal: int = 9
var pet_essence: int = 0
var theme_name: String = "붉은 성채"


func sync_from_game() -> void:
	floor_number = GameManager.floor
	kills = GameManager.kills_on_floor
	kill_goal = 8 + GameManager.floor
	pet_essence = PetManager.essence
	var theme_index: int = WorldLayout.dungeon_cycle_for_floor(floor_number) % 3
	theme_name = ["붉은 성채", "잿빛 납골당", "푸른 금고"][theme_index]
	queue_redraw()


func _draw() -> void:
	draw_rect(Rect2(Vector2.ZERO, size), Color(0.018, 0.026, 0.038, 0.92), true)
	draw_rect(Rect2(Vector2.ZERO, size), Color("4c5d73"), false, 1.0)

	var boss_floor: bool = floor_number % 10 == 0
	var title_color: Color = Color("ff6b6b") if boss_floor else Color("f0b84b")
	var title: String = "보스전 · %d층 · %s" % [floor_number, theme_name] if boss_floor else "%s · %d층 정리" % [theme_name, floor_number]
	draw_string(ThemeDB.fallback_font, Vector2(8, 14), title, HORIZONTAL_ALIGNMENT_LEFT, -1, 8, title_color)

	var progress: float = clampf(float(kills) / float(maxi(1, kill_goal)), 0.0, 1.0)
	draw_string(ThemeDB.fallback_font, Vector2(8, 29), "처치 %d / %d" % [kills, kill_goal], HORIZONTAL_ALIGNMENT_LEFT, -1, 7, Color("dce4ee"))
	draw_rect(Rect2(8, 33, size.x - 16, 5), Color("101722"), true)
	draw_rect(Rect2(8, 33, (size.x - 16) * progress, 5), title_color.darkened(0.10), true)

	var boss_in: int = 10 - (floor_number % 10)
	if boss_in == 10:
		boss_in = 0
	var footer: String
	if boss_in == 0:
		footer = "보스 보상 · 장비 + 펫 정수"
	else:
		footer = "보스 %d층 남음 · 펫 정수 %d/%d" % [boss_in, pet_essence, PetManager.SINGLE_SUMMON_COST]
	draw_string(ThemeDB.fallback_font, Vector2(8, 52), footer, HORIZONTAL_ALIGNMENT_LEFT, -1, 6, Color("a9b4c3"))
