extends Node

var _failures: Array[String] = []
var _checks: int = 0


func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	if not OS.get_cmdline_user_args().has("--affix-test-mode"):
		push_error("Gameplay V4 growth UX tests require --affix-test-mode")
		get_tree().quit(2)
		return
	call_deferred("_run")


func _check(condition: bool, message: String) -> void:
	_checks += 1
	if not condition:
		_failures.append(message)
		push_error("GAMEPLAY_V4_GROWTH: " + message)


func _run() -> void:
	var weapon := {
		"id": "growth_test_weapon",
		"name": "성장 테스트 검",
		"slot": "weapon",
		"item_level": 20,
		"enhancement_level": 0,
		"base_stats": {"ATK": 100.0},
		"affixes": [],
	}
	var preview: Dictionary = GameManager.equipment_enhancement_preview(weapon)
	_check(String(preview.get("status", "")) == "ready", "Fresh equipment exposes a ready enhancement preview")
	_check(int(preview.get("target_level", 0)) == 1, "Preview points to the next enhancement level")
	_check(is_equal_approx(float(preview.get("current_multiplier", 0.0)), 1.0), "Preview preserves the current base-stat multiplier")
	_check(is_equal_approx(float(preview.get("next_multiplier", 0.0)), 1.05), "Preview exposes the next base-stat multiplier")
	var gains: Dictionary = Dictionary(preview.get("stat_gains", {}))
	_check(is_equal_approx(float(gains.get("ATK", 0.0)), 5.0), "A +1 preview shows the real ATK gain from base stats")
	_check(float(preview.get("success_rate", 0.0)) == 100.0, "Early enhancement preview includes its success rate")
	_check(int(preview.get("cost", 0)) > 0, "Enhancement preview includes the gold cost")

	var max_weapon: Dictionary = weapon.duplicate(true)
	max_weapon["enhancement_level"] = GameManager.EQUIPMENT_ENHANCEMENT_MAX_LEVEL
	var max_preview: Dictionary = GameManager.equipment_enhancement_preview(max_weapon)
	_check(String(max_preview.get("status", "")) == "max", "Max enhancement is reported as a terminal state")

	var original_gold: int = GameManager.gold
	var original_class: String = GameManager.selected_class
	var original_class_name: String = GameManager.selected_class_name
	var original_equipment: Dictionary = GameManager.equipment.duplicate(true)
	var original_skills: Dictionary = GameManager.class_skill_levels.duplicate(true)

	GameManager.selected_class = "warrior"
	GameManager.selected_class_name = "전사"
	GameManager.gold = 100000000
	GameManager.equipment["weapon"] = weapon.duplicate(true)
	GameManager.class_skill_levels["warrior"] = {}
	var summary: Dictionary = GameManager.growth_opportunity_summary()
	_check(int(summary.get("equipment_count", 0)) >= 1, "Growth summary detects an affordable equipment enhancement")
	_check(Array(summary.get("equipment_slots", [])).has("weapon"), "Growth summary identifies the affordable equipment slot")
	_check(int(summary.get("skill_count", 0)) >= 1, "Growth summary detects affordable class-skill growth")

	var ui := GameUI.new()
	_check(String(GameUI.RARITY_BADGES.get("epic", "")) == "에픽", "Epic inventory badge is player-facing instead of falling back to Normal")
	_check(ui._format_probability(0.3) == "0.30%", "Very low enhancement odds remain readable instead of rounding to zero")
	_check(ui._format_probability(35.0) == "35%", "Common enhancement odds stay compact")
	var gain_text: String = ui._enhancement_gain_text({"ATK": 5.0})
	_check(gain_text.contains("공격 +5"), "Enhancement tooltip translates actual stat gain into player-facing text")
	ui.queue_free()

	GameManager.gold = original_gold
	GameManager.selected_class = original_class
	GameManager.selected_class_name = original_class_name
	GameManager.equipment = original_equipment
	GameManager.class_skill_levels = original_skills

	_finish()


func _finish() -> void:
	var output_dir: String = ProjectSettings.globalize_path("res://build/gameplay-v4")
	DirAccess.make_dir_recursive_absolute(output_dir)
	var report := FileAccess.open(output_dir.path_join("growth_ux.json"), FileAccess.WRITE)
	if report != null:
		report.store_string(JSON.stringify({
			"checks": _checks,
			"failures": _failures,
			"status": "PASS" if _failures.is_empty() else "FAIL",
			"scope": "enhancement preview, affordable growth summary, epic badge, probability formatting"
		}, "\t"))
		report.close()
	print("GAMEPLAY_V4_GROWTH %s: %d checks, %d failures" % ["PASSED" if _failures.is_empty() else "FAILED", _checks, _failures.size()])
	get_tree().quit(0 if _failures.is_empty() else 1)
