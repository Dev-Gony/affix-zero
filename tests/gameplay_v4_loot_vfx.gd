extends Node

var _failures: Array[String] = []
var _checks: int = 0


func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	if not OS.get_cmdline_user_args().has("--affix-test-mode"):
		push_error("Gameplay V4 loot-VFX tests require --affix-test-mode")
		get_tree().quit(2)
		return
	call_deferred("_run")


func _check(condition: bool, message: String) -> void:
	_checks += 1
	if not condition:
		_failures.append(message)
		push_error("GAMEPLAY_V4_LOOT_VFX: " + message)


func _run() -> void:
	var normal: Dictionary = EffectLayer.loot_visual_profile(0)
	var magic: Dictionary = EffectLayer.loot_visual_profile(1)
	var rare: Dictionary = EffectLayer.loot_visual_profile(2)
	var unique: Dictionary = EffectLayer.loot_visual_profile(3)
	var legend: Dictionary = EffectLayer.loot_visual_profile(4)
	var epic: Dictionary = EffectLayer.loot_visual_profile(5)
	var future: Dictionary = EffectLayer.loot_visual_profile(11)

	_check(is_zero_approx(float(normal.get("beam_height", -1.0))), "Normal drops stay visually quiet without a light pillar")
	_check(float(magic.get("beam_height", 0.0)) > 0.0, "Magic introduces a small rarity beam")
	_check(float(rare.get("beam_height", 0.0)) > float(magic.get("beam_height", 0.0)), "Rare beam is stronger than Magic")
	_check(int(rare.get("ring_count", 0)) >= 1, "Rare introduces a readable ground-ring accent")
	_check(float(unique.get("beam_width", 0.0)) > float(rare.get("beam_width", 0.0)), "Unique beam has greater visual weight than Rare")
	_check(float(legend.get("flash_alpha", 0.0)) > 0.0, "Legendary triggers a restrained global flash")
	_check(float(epic.get("beam_height", 0.0)) > float(legend.get("beam_height", 0.0)), "Epic remains more dramatic than Legendary")
	_check(float(epic.get("duration", 0.0)) >= float(legend.get("duration", 0.0)), "Epic is not shorter-lived than Legendary")
	_check(float(future.get("beam_height", 999.0)) <= 118.0, "Future rarity tiers scale without unbounded pillar height")

	var normal_delay: float = EffectLayer.loot_pickup_delay_for_rarity(0)
	var rare_delay: float = EffectLayer.loot_pickup_delay_for_rarity(2)
	var epic_delay: float = EffectLayer.loot_pickup_delay_for_rarity(5)
	_check(normal_delay < rare_delay, "Common drops auto-collect faster than Rare drops")
	_check(rare_delay < epic_delay, "Epic remains visible longer than Rare before auto-pickup")
	_check(epic_delay <= 0.90, "Even chase drops do not stall the idle loop for a full second")

	var effect := EffectLayer.new()
	add_child(effect)
	var item := {
		"id": "loot-vfx-test",
		"name": "테스트 장검",
		"rarity_name": "에픽",
		"rarity_index": 5,
		"rarity_color": "ff4fd8",
		"icon_index": 1,
		"boss_reward": true,
	}
	effect.show_drop(Vector2(200, 180), item)
	_check(effect._loot_icons.size() == 1, "show_drop registers exactly one loot visual")
	if effect._loot_icons.size() == 1:
		var visual: Dictionary = effect._loot_icons[0]
		_check(String(visual.get("label", "")).contains("[에픽]"), "Loot label includes the rarity name")
		_check(bool(visual.get("special_reward", false)), "Boss/elite reward receives a special reward accent")
		_check(float(visual.get("beam_height", 0.0)) >= float(epic.get("beam_height", 0.0)), "Rendered Epic uses the expected rarity beam profile")
	_check(effect._flash_alpha > 0.0, "Epic drop primes the rare-drop flash feedback")

	effect.show_pickup(Vector2(200, 180), Vector2(240, 180), item)
	if effect._loot_icons.size() == 1:
		_check(float(effect._loot_icons[0].get("life", 1.0)) <= 0.28, "Pickup collapses the remaining beam instead of leaving a ghost pillar")

	_finish()


func _finish() -> void:
	var output_dir: String = ProjectSettings.globalize_path("res://build/gameplay-v4")
	DirAccess.make_dir_recursive_absolute(output_dir)
	var report := FileAccess.open(output_dir.path_join("loot_vfx.json"), FileAccess.WRITE)
	if report != null:
		report.store_string(JSON.stringify({
			"checks": _checks,
			"failures": _failures,
			"status": "PASS" if _failures.is_empty() else "FAIL",
			"scope": "rarity beam escalation, x5-safe pickup visibility, special-reward accent, beam collapse on pickup"
		}, "\t"))
		report.close()
	print("GAMEPLAY_V4_LOOT_VFX %s: %d checks, %d failures" % ["PASSED" if _failures.is_empty() else "FAILED", _checks, _failures.size()])
	get_tree().quit(0 if _failures.is_empty() else 1)
