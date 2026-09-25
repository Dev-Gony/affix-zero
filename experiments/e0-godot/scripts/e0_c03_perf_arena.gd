class_name E0C03PerfArena
extends Node2D

signal benchmark_finished(summary: Dictionary)

const WarriorScene = preload("res://scenes/warrior.tscn")
const MeleeScene = preload("res://scenes/melee_enemy.tscn")
const RangedScene = preload("res://scenes/ranged_enemy.tscn")
const ProjectileScene = preload("res://scenes/enemy_projectile.tscn")

@export var melee_count: int = 32
@export var ranged_count: int = 8
@export var warmup_seconds: float = 10.0
@export var measure_seconds: float = 60.0
@export var repetitions: int = 3
@export var write_reports: bool = true
@export var exit_when_finished: bool = false

var warrior: E0Warrior
var enemies: Array[E0EnemyBase] = []
var active_projectiles: Dictionary = {}

var projectile_spawn_total: int = 0
var projectile_hit_total: int = 0
var projectile_expire_total: int = 0
var drop_count: int = 0

var _phase: StringName = &"warmup"
var _phase_started_usec: int = 0
var _last_frame_usec: int = 0
var _run_index: int = 0
var _samples_ms: Array[float] = []
var _all_run_samples: Array = []
var _run_summaries: Array[Dictionary] = []
var _finished: bool = false

var phase_label: Label
var stats_label: Label
var count_label: Label
var path_label: Label

func configure_for_ci(warmup: float = 0.25, measure: float = 1.5, repeat_count: int = 1) -> void:
	warmup_seconds = warmup
	measure_seconds = measure
	repetitions = repeat_count
	write_reports = false
	exit_when_finished = false

func _ready() -> void:
	Engine.time_scale = 1.0
	Engine.max_fps = 0
	if DisplayServer.get_name() != "headless":
		DisplayServer.window_set_vsync_mode(DisplayServer.VSYNC_DISABLED)
	_build_hud()
	_spawn_fixture()
	_phase_started_usec = Time.get_ticks_usec()
	_last_frame_usec = _phase_started_usec
	queue_redraw()

func _process(_delta: float) -> void:
	if _finished:
		return
	_retarget_warrior_if_needed()
	var now := Time.get_ticks_usec()
	var frame_ms := float(now - _last_frame_usec) / 1000.0
	_last_frame_usec = now
	var elapsed := float(now - _phase_started_usec) / 1000000.0

	if _phase == &"warmup":
		if elapsed >= warmup_seconds:
			_phase = &"measure"
			_phase_started_usec = now
			_last_frame_usec = now
			_samples_ms.clear()
	elif _phase == &"measure":
		if frame_ms > 0.0 and frame_ms < 1000.0:
			_samples_ms.append(frame_ms)
		if elapsed >= measure_seconds:
			_finish_measurement_run(now)

	_update_hud(elapsed)

func _spawn_fixture() -> void:
	warrior = WarriorScene.instantiate() as E0Warrior
	warrior.name = "PerfWarrior"
	add_child(warrior)
	warrior.position = Vector2(320, 192)
	warrior.max_hp = 1000000000
	warrior.hp = warrior.max_hp
	warrior.attack_damage = 20.0

	var total := melee_count + ranged_count
	for index in total:
		var angle := TAU * float(index) / float(total)
		var radius := 126.0 + float((index % 4) * 10)
		var spawn_position := Vector2(320, 192) + Vector2(cos(angle), sin(angle)) * radius
		if index < melee_count:
			var melee := MeleeScene.instantiate() as E0MeleeEnemy
			melee.name = "PerfMelee_%02d" % index
			add_child(melee)
			melee.position = spawn_position
			melee.max_hp = 1000000000
			melee.hp = melee.max_hp
			melee.defense = 4.0
			melee.set_target(warrior)
			enemies.append(melee)
		else:
			var ranged := RangedScene.instantiate() as E0RangedEnemy
			ranged.name = "PerfRanged_%02d" % index
			add_child(ranged)
			ranged.position = spawn_position
			ranged.max_hp = 1000000000
			ranged.hp = ranged.max_hp
			ranged.defense = 4.0
			ranged.movement_bounds = Rect2(40, 92, 560, 188)
			ranged.set_target(warrior)
			ranged.projectile_requested.connect(_on_projectile_requested)
			enemies.append(ranged)

	var target := _nearest_live_enemy()
	if target != null:
		warrior.set_target(target)

func _retarget_warrior_if_needed() -> void:
	if warrior == null or warrior.is_dead() or warrior.has_live_target():
		return
	var next := _nearest_live_enemy()
	if next != null:
		warrior.set_target(next)

func _nearest_live_enemy() -> E0EnemyBase:
	var best: E0EnemyBase
	var best_distance := INF
	for enemy in enemies:
		if enemy == null or not is_instance_valid(enemy) or enemy.is_dead():
			continue
		var distance := warrior.global_position.distance_to(enemy.global_position)
		if distance < best_distance:
			best_distance = distance
			best = enemy
	return best

func _on_projectile_requested(_enemy: E0RangedEnemy, origin: Vector2, direction: Vector2, damage: int, projectile_id: int) -> void:
	var projectile := ProjectileScene.instantiate() as E0EnemyProjectile
	projectile.name = "PerfProjectile_%06d" % projectile_spawn_total
	add_child(projectile)
	projectile.setup(projectile_id, origin, direction, warrior, damage)
	projectile.impacted.connect(_on_projectile_impacted)
	projectile.expired.connect(_on_projectile_expired)
	active_projectiles[projectile.get_instance_id()] = projectile
	projectile_spawn_total += 1

func _on_projectile_impacted(projectile: E0EnemyProjectile, _distance: float) -> void:
	projectile_hit_total += 1
	active_projectiles.erase(projectile.get_instance_id())

func _on_projectile_expired(projectile: E0EnemyProjectile) -> void:
	projectile_expire_total += 1
	active_projectiles.erase(projectile.get_instance_id())

func _finish_measurement_run(now_usec: int) -> void:
	var stats := E0PerfMetrics.summarize(_samples_ms)
	stats["run"] = _run_index + 1
	stats["warmup_seconds"] = warmup_seconds
	stats["measure_seconds"] = measure_seconds
	stats["enemy_count"] = enemies.size()
	stats["alive_enemy_count"] = _alive_enemy_count()
	stats["melee_count"] = melee_count
	stats["ranged_count"] = ranged_count
	stats["projectile_spawn_total"] = projectile_spawn_total
	stats["projectile_hit_total"] = projectile_hit_total
	stats["projectile_expire_total"] = projectile_expire_total
	stats["active_projectile_count"] = active_projectiles.size()
	stats["drop_count"] = drop_count
	stats["static_memory_bytes"] = OS.get_static_memory_usage()
	_run_summaries.append(stats)
	_all_run_samples.append(_samples_ms.duplicate())

	_run_index += 1
	if _run_index >= repetitions:
		_finish_benchmark()
		return
	_phase = &"warmup"
	_phase_started_usec = now_usec
	_last_frame_usec = now_usec
	_samples_ms.clear()

func _finish_benchmark() -> void:
	_finished = true
	var aggregate_samples: Array[float] = []
	for run_samples in _all_run_samples:
		for value in run_samples:
			aggregate_samples.append(float(value))
	var aggregate := E0PerfMetrics.summarize(aggregate_samples)
	var summary := {
		"schema": "affix-zero-e0-c03-v1",
		"engine": Engine.get_version_info(),
		"renderer": RenderingServer.get_current_rendering_method(),
		"display_server": DisplayServer.get_name(),
		"time_scale": Engine.time_scale,
		"max_fps": Engine.max_fps,
		"fixture": {
			"enemy_count": enemies.size(),
			"melee_count": melee_count,
			"ranged_count": ranged_count,
			"drop_count": drop_count,
		},
		"configuration": {
			"warmup_seconds": warmup_seconds,
			"measure_seconds": measure_seconds,
			"repetitions": repetitions,
			"world_viewport": [640, 360],
			"window_override": [1280, 720],
			"speed": "x1",
			"vsync_disabled_by_harness": DisplayServer.get_name() != "headless",
		},
		"aggregate": aggregate,
		"runs": _run_summaries,
		"projectiles": {
			"spawn_total": projectile_spawn_total,
			"hit_total": projectile_hit_total,
			"expire_total": projectile_expire_total,
			"active_at_end": active_projectiles.size(),
		},
		"memory": {
			"static_bytes_at_end": OS.get_static_memory_usage(),
		},
		"warning": "CI/headless metrics validate the harness only. Use the visible Windows run on the target PC for performance conclusions.",
	}
	var report_path := ""
	if write_reports:
		report_path = _write_reports(summary)
	summary["report_path"] = report_path
	_show_finished(summary)
	benchmark_finished.emit(summary)
	print("E0_C03_BENCHMARK_COMPLETE " + JSON.stringify(summary))
	if exit_when_finished:
		get_tree().quit(0)

func _write_reports(summary: Dictionary) -> String:
	var dir := "user://perf"
	DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(dir))
	var stamp := Time.get_datetime_string_from_system(false, true).replace(":", "").replace("-", "")
	var json_path := "%s/e0_c03_%s.json" % [dir, stamp]
	var csv_path := "%s/e0_c03_%s.csv" % [dir, stamp]
	var json_file := FileAccess.open(json_path, FileAccess.WRITE)
	if json_file != null:
		json_file.store_string(JSON.stringify(summary, "\t"))
	var csv_file := FileAccess.open(csv_path, FileAccess.WRITE)
	if csv_file != null:
		csv_file.store_line("run,sample,frame_ms")
		for run_idx in _all_run_samples.size():
			var run_samples = _all_run_samples[run_idx]
			for sample_idx in run_samples.size():
				csv_file.store_line("%d,%d,%.6f" % [run_idx + 1, sample_idx + 1, float(run_samples[sample_idx])])
	return ProjectSettings.globalize_path(json_path)

func _alive_enemy_count() -> int:
	var count := 0
	for enemy in enemies:
		if enemy != null and is_instance_valid(enemy) and not enemy.is_dead():
			count += 1
	return count

func _build_hud() -> void:
	var canvas := CanvasLayer.new()
	add_child(canvas)
	var title_label := Label.new()
	title_label.position = Vector2(20, 14)
	title_label.text = "AFFIX: ZERO  E0-C03  |  40 ENEMIES x1"
	title_label.add_theme_font_size_override("font_size", 20)
	title_label.modulate = Color("f2d083")
	canvas.add_child(title_label)

	phase_label = Label.new()
	phase_label.position = Vector2(20, 42)
	phase_label.add_theme_font_size_override("font_size", 13)
	canvas.add_child(phase_label)

	stats_label = Label.new()
	stats_label.position = Vector2(20, 62)
	stats_label.add_theme_font_size_override("font_size", 12)
	canvas.add_child(stats_label)

	count_label = Label.new()
	count_label.position = Vector2(20, 82)
	count_label.add_theme_font_size_override("font_size", 12)
	count_label.modulate = Color("aeb8c7")
	canvas.add_child(count_label)

	path_label = Label.new()
	path_label.position = Vector2(20, 330)
	path_label.add_theme_font_size_override("font_size", 10)
	path_label.modulate = Color("82e69b")
	canvas.add_child(path_label)

func _update_hud(elapsed: float) -> void:
	var target_seconds := warmup_seconds if _phase == &"warmup" else measure_seconds
	phase_label.text = "Run %d/%d   %s   %.1f / %.1fs" % [
		mini(_run_index + 1, repetitions),
		repetitions,
		String(_phase).to_upper(),
		minf(elapsed, target_seconds),
		target_seconds
	]
	var current := E0PerfMetrics.summarize(_samples_ms)
	stats_label.text = "samples %d   p50 %.2fms   p95 %.2fms   p99 %.2fms   max %.2fms" % [
		current["sample_count"], current["p50_ms"], current["p95_ms"], current["p99_ms"], current["max_ms"]
	]
	count_label.text = "enemies %d/%d   projectiles active %d   spawned %d   hits %d   drops %d" % [
		_alive_enemy_count(), enemies.size(), active_projectiles.size(), projectile_spawn_total, projectile_hit_total, drop_count
	]

func _show_finished(summary: Dictionary) -> void:
	var aggregate: Dictionary = summary["aggregate"]
	phase_label.text = "COMPLETE   %d runs" % repetitions
	stats_label.text = "p50 %.2fms   p95 %.2fms   p99 %.2fms   max %.2fms   avg %.1f FPS" % [
		aggregate["p50_ms"], aggregate["p95_ms"], aggregate["p99_ms"], aggregate["max_ms"], aggregate["avg_fps"]
	]
	count_label.text = "enemies %d/%d   projectiles spawned %d   hits %d   drops %d" % [
		_alive_enemy_count(), enemies.size(), projectile_spawn_total, projectile_hit_total, drop_count
	]
	path_label.text = "Report: %s" % String(summary["report_path"])

func _draw() -> void:
	draw_rect(Rect2(0, 0, 640, 360), Color("101219"))
	for x in range(0, 641, 32):
		draw_line(Vector2(x, 104), Vector2(x, 320), Color("222733"), 1.0)
	for y in range(104, 321, 32):
		draw_line(Vector2(0, y), Vector2(640, y), Color("222733"), 1.0)
	draw_rect(Rect2(16, 104, 608, 216), Color("687180"), false, 2.0)
