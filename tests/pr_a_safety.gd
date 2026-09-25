extends Node

var _failures: Array[String] = []
var _checks: int = 0


func _ready() -> void:
	if not OS.get_cmdline_user_args().has("--affix-test-mode"):
		push_error("PR-A tests require --affix-test-mode BEFORE autoload startup")
		get_tree().quit(2)
		return
	call_deferred("_run")


func _check(condition: bool, message: String) -> void:
	_checks += 1
	if not condition:
		_failures.append(message)
		push_error("PR_A: " + message)


func _write(path: String, bytes: PackedByteArray) -> bool:
	var file := FileAccess.open(path, FileAccess.WRITE)
	if file == null:
		return false
	file.store_buffer(bytes)
	file.close()
	return true


func _run() -> void:
	_check(not SaveManager.persistence_enabled, "Test mode disables save loading/writing before the test scene runs")
	var fixture_text: String = FileAccess.get_file_as_string("res://tests/fixtures/pr_a_legacy_v2.json")
	var fixture: Dictionary = JSON.parse_string(fixture_text) as Dictionary
	GameManager.apply_save_dict(fixture)
	var before: Dictionary = GameManager.to_save_dict()
	var root_path: String = "user://pr_a_tests/%d-%d" % [OS.get_process_id(), Time.get_ticks_usec()]
	var setup_error: Error = DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(root_path))
	_check(setup_error == OK, "Synthetic test directory can be created")
	if setup_error != OK:
		_finish()
		return
	var source: String = root_path.path_join("input.json")
	var directory: String = root_path.path_join("backups")
	var raw: PackedByteArray = fixture_text.to_utf8_buffer()
	_check(_write(source, raw), "Synthetic fixture can be written without touching user://save.json")
	var first: Dictionary = SaveBackup.capture(source, directory)
	_check(bool(first.get("ok", false)), "Backup succeeds")
	_check(String(first.get("status", "")) == "created_verified", "First backup is verified before success")
	_check(String(first.get("sha256", "")) == fixture_text.sha256_text(), "Backup digest equals original UTF-8 bytes")
	var backup_path: String = String(first.get("path", ""))
	_check(not backup_path.is_empty() and FileAccess.get_file_as_bytes(backup_path) == raw, "Backup preserves exact formatting and bytes")
	_check(FileAccess.get_file_as_bytes(source) == raw, "Source file is unchanged")
	var second: Dictionary = SaveBackup.capture(source, directory)
	_check(String(second.get("status", "")) == "reused_verified", "Repeated backup reuses a verified identical file")
	_check(String(second.get("path", "")) == backup_path, "Repeated backup is idempotent for the same day and content")
	var changed: PackedByteArray = (fixture_text + "\n").to_utf8_buffer()
	_check(_write(source, changed), "Test can create a distinct source snapshot")
	var third: Dictionary = SaveBackup.capture(source, directory)
	_check(bool(third.get("ok", false)) and String(third.get("path", "")) != backup_path, "Changed bytes produce a distinct backup")
	_check(FileAccess.get_file_as_bytes(backup_path) == raw, "Earlier backup remains unchanged")
	var invalid_source: String = root_path.path_join("invalid.json")
	var invalid_bytes: PackedByteArray = "{broken json".to_utf8_buffer()
	_check(_write(invalid_source, invalid_bytes), "Malformed synthetic fixture is created")
	var invalid_backup: Dictionary = SaveBackup.capture(invalid_source, directory)
	_check(bool(invalid_backup.get("ok", false)) and FileAccess.get_file_as_bytes(String(invalid_backup.get("path", ""))) == invalid_bytes, "Even malformed original bytes are retained for recovery")
	_check(bool(SaveBackup.capture(root_path.path_join("missing.json"), directory).get("ok", false)), "Missing first-play save is not a backup failure")
	var blocking_file: String = root_path.path_join("not-a-directory")
	_check(_write(blocking_file, "blocker".to_utf8_buffer()), "Synthetic path conflict is created")
	var failed: Dictionary = SaveBackup.capture(source, blocking_file.path_join("backups"))
	_check(not bool(failed.get("ok", true)), "Unwritable backup destination is reported as failure")
	_check(FileAccess.get_file_as_bytes(source) == changed, "Backup failure does not alter the source")
	_check(_write(backup_path, "corrupt-backup".to_utf8_buffer()), "Synthetic existing backup is corrupted for a negative test")
	_check(_write(source, raw), "Original synthetic snapshot restored")
	var corrupt: Dictionary = SaveBackup.capture(source, directory)
	_check(not bool(corrupt.get("ok", true)), "Conflicting existing backup is rejected, never overwritten")
	_check(FileAccess.get_file_as_string(backup_path) == "corrupt-backup", "Rejected conflicting backup is retained")
	var guard_error: Error = SaveManager.prepare_backup(source, blocking_file.path_join("guard"))
	_check(guard_error != OK and SaveManager.write_guard_error != OK, "Backup failure latches the write guard")
	# The guard must reject BEFORE opening SAVE_PATH; restore test mode immediately.
	SaveManager.set_persistence_enabled(true)
	var rejected_save: Error = SaveManager.save_game(fixture)
	SaveManager.set_persistence_enabled(false)
	_check(rejected_save != OK, "Write guard prevents a subsequent save attempt")
	var after: Dictionary = GameManager.to_save_dict()
	for key: String in ["gold", "inventory", "equipment", "class_skill_levels", "selected_class", "level", "xp", "floor", "rebirth_count"]:
		_check(before[key] == after[key], "Backup operations preserve profile field: " + key)
	var info: Dictionary = BuildInfo.read_info()
	_check(String(info.get("version", "")) == "uiux-pr-a.1", "Build metadata contains the PR-A version")
	_check(String(info.get("baseline_commit", "")) == "cdcf080d2916c053e72e5c3c4ecc57a30213d0f2", "Build metadata pins the approved baseline")
	_check(int(ProjectSettings.get_setting("display/window/size/viewport_width")) == 640, "PR-A does not migrate the viewport prematurely")
	SaveManager.write_guard_error = OK
	SaveManager.write_guard_reason = ""
	_finish()


func _finish() -> void:
	SaveManager.set_persistence_enabled(false)
	var output_dir: String = ProjectSettings.globalize_path("res://build/pr-a")
	DirAccess.make_dir_recursive_absolute(output_dir)
	var report := FileAccess.open(output_dir.path_join("safety.json"), FileAccess.WRITE)
	if report != null:
		report.store_string(JSON.stringify({"checks": _checks, "failures": _failures, "status": "PASS" if _failures.is_empty() else "FAIL", "scope": "PR-A synthetic backup/identity only; full v1 acceptance NOT_RUN"}, "\t"))
		report.close()
	print("PR_A_SAFETY %s: %d checks, %d failures" % ["PASSED" if _failures.is_empty() else "FAILED", _checks, _failures.size()])
	get_tree().quit(0 if _failures.is_empty() else 1)
