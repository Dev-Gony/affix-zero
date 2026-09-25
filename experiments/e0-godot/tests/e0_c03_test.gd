extends Node

const PerfScene = preload("res://perf/e0_c03_perf.tscn")

func _ready() -> void:
	var arena := PerfScene.instantiate() as E0C03PerfArena
	arena.configure_for_ci(0.20, 1.20, 1)
	arena.benchmark_finished.connect(_on_finished)
	add_child(arena)

func _on_finished(summary: Dictionary) -> void:
	var aggregate: Dictionary = summary["aggregate"]
	var fixture: Dictionary = summary["fixture"]
	var projectiles: Dictionary = summary["projectiles"]
	if not _check(fixture["enemy_count"] == 40, "fixture must contain exactly 40 enemies"):
		return
	if not _check(fixture["melee_count"] == 32 and fixture["ranged_count"] == 8, "fixture mix must be 32 melee + 8 ranged"):
		return
	if not _check(aggregate["sample_count"] > 10, "performance recorder must collect frame samples"):
		return
	if not _check(aggregate["p50_ms"] > 0.0, "p50 must be positive"):
		return
	if not _check(aggregate["p95_ms"] >= aggregate["p50_ms"], "p95 must be >= p50"):
		return
	if not _check(aggregate["p99_ms"] >= aggregate["p95_ms"], "p99 must be >= p95"):
		return
	if not _check(aggregate["max_ms"] >= aggregate["p99_ms"], "max must be >= p99"):
		return
	if not _check(projectiles["spawn_total"] > 0, "ranged fixture must spawn projectiles"):
		return
	if not _check(summary["time_scale"] == 1.0, "benchmark must run at x1 time scale"):
		return
	if not _check(String(summary["renderer"]) == "gl_compatibility", "benchmark must use compatibility renderer"):
		return
	print("E0_C03_TEST PASSED")
	get_tree().quit(0)

func _check(condition: bool, message: String) -> bool:
	if condition:
		return true
	push_error("E0_C03_TEST FAILED: %s" % message)
	print("E0_C03_TEST FAILED: %s" % message)
	get_tree().quit(1)
	return false
