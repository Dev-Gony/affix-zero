extends Node

var _failures: Array[String] = []
var _checks: int = 0


func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	if not OS.get_cmdline_user_args().has("--affix-test-mode"):
		push_error("Gameplay V4 loot-feedback tests require --affix-test-mode")
		get_tree().quit(2)
		return
	call_deferred("_run")


func _check(condition: bool, message: String) -> void:
	_checks += 1
	if not condition:
		_failures.append(message)
		push_error("GAMEPLAY_V4_LOOT: " + message)


func _run() -> void:
	var heights: Array[float] = []
	var durations: Array[float] = []
	for rarity_index: int in 6:
		heights.append(EffectLayer.loot_beam_height(rarity_index))
		durations.append(EffectLayer.loot_effect_duration(rarity_index))

	for index: int in range(1, heights.size()):
		_check(heights[index] > heights[index - 1], "Higher rarity always has a taller loot beam")
		_check(durations[index] >= durations[index - 1], "Higher rarity never has a shorter drop celebration")

	_check(EffectLayer.loot_beam_height(0) <= 20.0, "Normal drops stay visually restrained")
	_check(EffectLayer.loot_beam_height(5) >= 100.0, "Epic drops have a clearly premium light pillar")
	_check(EffectLayer.loot_screen_flash_alpha(3) == 0.0, "Unique and below do not flash the whole screen")
	_check(EffectLayer.loot_screen_flash_alpha(4) > 0.0, "Legendary triggers a screen flash")
	_check(EffectLayer.loot_screen_flash_alpha(5) > EffectLayer.loot_screen_flash_alpha(4), "Epic flash is stronger than Legendary")
	_check(EffectLayer.loot_beam_width(5) > EffectLayer.loot_beam_width(2), "Epic beam is materially wider than Rare")

	var effect := EffectLayer.new()
	add_child(effect)
	var epic_item := {
		"name": "테스트 에픽",
		"rarity_name": "에픽",
		"rarity_id": "epic",
		"rarity_index": 5,
		"rarity_color": "d138ff",
		"icon_index": 0,
	}
	effect.show_drop(Vector2(200, 200), epic_item)
	_check(effect._loot_icons.size() == 1, "A dropped equipment item registers one world icon")
	if effect._loot_icons.size() == 1:
		var icon_data: Dictionary = effect._loot_icons[0]
		_check(int(icon_data.get("rarity_index", -1)) == 5, "Loot effect preserves the Epic rarity index")
		_check(float(icon_data.get("beam_height", 0.0)) >= 100.0, "Epic world icon carries its premium beam height")
	_check(effect._flash_alpha > 0.0, "Epic drop immediately primes the screen flash")
	_check(effect._texts.any(func(data: Dictionary) -> bool: return String(data.get("text", "")).contains("[에픽]")), "Drop label includes the player-facing rarity name")

	_finish()


func _finish() -> void:
	var output_dir: String = ProjectSettings.globalize_path("res://build/gameplay-v4")
	DirAccess.make_dir_recursive_absolute(output_dir)
	var report := FileAccess.open(output_dir.path_join("loot_feedback.json"), FileAccess.WRITE)
	if report != null:
		report.store_string(JSON.stringify({
			"checks": _checks,
			"failures": _failures,
			"status": "PASS" if _failures.is_empty() else "FAIL",
			"scope": "rarity-scaled light pillars, duration hierarchy, premium flashes, rarity labels"
		}, "\t"))
		report.close()
	print("GAMEPLAY_V4_LOOT %s: %d checks, %d failures" % ["PASSED" if _failures.is_empty() else "FAILED", _checks, _failures.size()])
	get_tree().quit(0 if _failures.is_empty() else 1)
