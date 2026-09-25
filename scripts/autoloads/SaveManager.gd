extends Node

signal save_completed
signal load_completed(found_save: bool)
signal save_blocked(reason: String)

const SAVE_PATH: String = "user://save.json"
const AUTOSAVE_INTERVAL: float = 30.0

var _last_autosave_msec: int = 0
var persistence_enabled: bool = true
var autosave_enabled: bool = true
var backup_status: Dictionary = {}
var write_guard_error: Error = OK
var write_guard_reason: String = ""
var _initial_backup_checked: bool = false


func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	# This must happen BEFORE autoload startup reads a developer's save.
	persistence_enabled = not OS.get_cmdline_user_args().has("--affix-test-mode")
	if persistence_enabled:
		load_game()
	_last_autosave_msec = Time.get_ticks_msec()


func _process(_delta: float) -> void:
	var now_msec: int = Time.get_ticks_msec()
	if autosave_enabled and now_msec - _last_autosave_msec >= int(AUTOSAVE_INTERVAL * 1000.0):
		_last_autosave_msec = now_msec
		save_game()


func prepare_backup(source_path: String = SAVE_PATH, directory: String = SaveBackup.DEFAULT_DIRECTORY) -> Error:
	backup_status = SaveBackup.capture(source_path, directory)
	if not bool(backup_status.get("ok", false)):
		return _block_writes(ERR_CANT_CREATE, "Pre-rebuild backup failed: %s" % String(backup_status.get("status", "unknown")))
	_initial_backup_checked = true
	return OK


func save_game(data: Dictionary = {}) -> Error:
	if not persistence_enabled:
		return OK
	if write_guard_error != OK:
		return write_guard_error
	if not _initial_backup_checked:
		var backup_error: Error = prepare_backup()
		if backup_error != OK:
			return backup_error
	# Atomic tmp/main/bak transactions are a PR-B change, not claimed here.
	var save_data: Dictionary = data if not data.is_empty() else GameManager.to_save_dict()
	var save_file := FileAccess.open(SAVE_PATH, FileAccess.WRITE)
	if save_file == null:
		return FileAccess.get_open_error()
	save_file.store_string(JSON.stringify(save_data, "\t"))
	save_file.flush()
	var write_error: Error = save_file.get_error()
	save_file.close()
	if write_error != OK:
		return write_error
	save_completed.emit()
	return OK


func load_game() -> Dictionary:
	if not persistence_enabled:
		load_completed.emit(false)
		return {}
	if prepare_backup() != OK:
		load_completed.emit(false)
		return {}
	if not FileAccess.file_exists(SAVE_PATH):
		load_completed.emit(false)
		return {}
	var save_file := FileAccess.open(SAVE_PATH, FileAccess.READ)
	if save_file == null:
		_block_writes(ERR_FILE_CANT_READ, "Unable to read the existing save")
		load_completed.emit(false)
		return {}
	var source_text: String = save_file.get_as_text()
	save_file.close()
	var parsed_data: Variant = JSON.parse_string(source_text)
	if not parsed_data is Dictionary:
		_block_writes(ERR_FILE_CORRUPT, "Invalid save JSON; original file and raw backup are preserved")
		load_completed.emit(false)
		return {}
	var data: Dictionary = parsed_data as Dictionary
	var source_version: int = int(data.get("schema_version", data.get("version", 1)))
	if source_version < 1 or source_version > 2:
		_block_writes(ERR_FILE_UNRECOGNIZED, "Unsupported save version %d; refusing to downgrade or overwrite" % source_version)
		load_completed.emit(false)
		return {}
	LootManager.migrate_save_data(data)
	GameManager.apply_save_dict(data)
	RebirthManager.sync_unlocked_classes()
	write_guard_error = OK
	write_guard_reason = ""
	load_completed.emit(true)
	return data


func _block_writes(error: Error, reason: String) -> Error:
	write_guard_error = error
	write_guard_reason = reason
	save_blocked.emit(reason)
	push_warning(reason)
	return error


func _notification(what: int) -> void:
	if what == NOTIFICATION_WM_CLOSE_REQUEST:
		save_game()


func set_autosave_enabled(enabled: bool) -> void:
	autosave_enabled = enabled
	_last_autosave_msec = Time.get_ticks_msec()


func set_persistence_enabled(enabled: bool) -> void:
	persistence_enabled = enabled
	_last_autosave_msec = Time.get_ticks_msec()
