extends Control
class_name CombatObjective

var floor_number: int = 1
var kills: int = 0
var kill_goal: int = 9
var next_pet_floor: int = 0
var next_pet_name: String = ""


func sync_from_game() -> void:
	floor_number = GameManager.floor
	kills = GameManager.kills_on_floor
	kill_goal = 8 + GameManager.floor
	_resolve_next_pet()
	queue_redraw()


func _resolve_next_pet() -> void:
	next_pet_floor = 0
	next_pet_name = ""
	for pet_id: String in PetManager.all_pet_ids():
		var data: PetData = PetManager.get_pet_data(pet_id)
		if data == null or PetManager.is_owned(pet_id):
			continue
		if data.unlock_floor <= floor_number:
			continue
		if next_pet_floor == 0 or data.unlock_floor < next_pet_floor:
			next_pet_floor = data.unlock_floor
			next_pet_name = data.display_name


func _draw() -> void:
	draw_rect(Rect2(Vector2.ZERO, size), Color(0.025, 0.035, 0.05, 0.88), true)
	draw_rect(Rect2(Vector2.ZERO, size), Color("405168"), false, 1.0)

	var boss_floor: bool = floor_number % 10 == 0
	var title_color: Color = Color("ff6b6b") if boss_floor else Color("f0b84b")
	var title: String = "보스전 · %d층" % floor_number if boss_floor else "현재 목표 · %d층 정리" % floor_number
	draw_string(ThemeDB.fallback_font, Vector2(8, 14), title, HORIZONTAL_ALIGNMENT_LEFT, -1, 8, title_color)

	var progress: float = clampf(float(kills) / float(maxi(1, kill_goal)), 0.0, 1.0)
	draw_string(ThemeDB.fallback_font, Vector2(8, 29), "처치 %d / %d" % [kills, kill_goal], HORIZONTAL_ALIGNMENT_LEFT, -1, 7, Color("dce4ee"))
	draw_rect(Rect2(8, 33, size.x - 16, 5), Color("101722"), true)
	draw_rect(Rect2(8, 33, (size.x - 16) * progress, 5), title_color.darkened(0.10), true)

	var boss_in: int = 10 - (floor_number % 10)
	if boss_in == 10:
		boss_in = 0
	var footer: String = ""
	if boss_in == 0:
		footer = "보스 처치 시 보상 장비 확정"
	elif next_pet_floor > 0:
		footer = "보스 %d층 남음 · %s %d층 해금" % [boss_in, next_pet_name, next_pet_floor]
	else:
		footer = "보스 %d층 남음 · 모든 펫 해금 완료" % boss_in
	draw_string(ThemeDB.fallback_font, Vector2(8, 51), footer, HORIZONTAL_ALIGNMENT_LEFT, -1, 6, Color("95a3b5"))
