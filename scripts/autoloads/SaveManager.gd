extends Node

signal save_completed
signal load_completed(found_save: bool)
signal save_blocked(reason: String)

const SAVE_PATH: String = "user://save.json"
const AUTOSAVE_INTERVAL: float = 30.0
const TRANSACTION_BACKUP_SUFFIX: String = ".bak"
const TRANSACTION_TEMP_SUFFIX: String = ".tmp"

var _last_autosave_msec: int = 0
var persistence_enabled: bool = true
var autosave_enabled: bool = true
var backup_status: Dictionary = {}
var write_guard_error: Error = OK
var write_guard_reason: String = ""
var last_save_error: String = ""
var _initial_backup_checked: bool = false


func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	get_tree().auto_accept_quit = false
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
	last_save_error = ""
	if not persistence_enabled:
		return OK
	if write_guard_error != OK:
		last_save_error = write_guard_reason
		return write_guard_error
	if not _initial_backup_checked:
		var backup_error: Error = prepare_backup()
		if backup_error != OK:
			last_save_error = write_guard_reason
			return backup_error
	var save_data: Dictionary = data if not data.is_empty() else GameManager.to_save_dict()
	if data.is_empty():
		save_data["pets"] = PetManager.to_save_dict()
	var error: Error = write_atomic(SAVE_PATH, save_data)
	if error != OK:
		last_save_error = "저장 트랜잭션 실패 (%d)" % int(error)
		push_warning(last_save_error)
		return error
	save_completed.emit()
	return OK


func write_atomic(target_path: String, data: Dictionary) -> Error:
	var temp_path: String = target_path + TRANSACTION_TEMP_SUFFIX
	var rollback_path: String = target_path + TRANSACTION_BACKUP_SUFFIX
	_remove_if_exists(temp_path)

	var serialized: String = JSON.stringify(data, "	")
	var temp_file := FileAccess.open(temp_path, FileAccess.WRITE)
	if temp_file == null:
		return FileAccess.get_open_error()
	temp_file.store_string(serialized)
	temp_file.flush()
	var write_error: Error = temp_file.get_error()
	temp_file.close()
	if write_error != OK:
		_remove_if_exists(temp_path)
		return write_error
	if not _is_valid_save_file(temp_path):
		_remove_if_exists(temp_path)
		return ERR_FILE_CORRUPT

	var had_previous: bool = FileAccess.file_exists(target_path)
	if had_previous:
		_remove_if_exists(rollback_path)
		var rotate_error: Error = DirAccess.rename_absolute(
			ProjectSettings.globalize_path(target_path),
			ProjectSettings.globalize_path(rollback_path)
		)
		if rotate_error != OK:
			_remove_if_exists(temp_path)
			return rotate_error

	var publish_error: Error = DirAccess.rename_absolute(
		ProjectSettings.globalize_path(temp_path),
		ProjectSettings.globalize_path(target_path)
	)
	if publish_error != OK:
		if had_previous and FileAccess.file_exists(rollback_path):
			DirAccess.rename_absolute(
				ProjectSettings.globalize_path(rollback_path),
				ProjectSettings.globalize_path(target_path)
			)
		_remove_if_exists(temp_path)
		return publish_error

	if not _is_valid_save_file(target_path):
		_remove_if_exists(target_path)
		if had_previous and FileAccess.file_exists(rollback_path):
			DirAccess.rename_absolute(
				ProjectSettings.globalize_path(rollback_path),
				ProjectSettings.globalize_path(target_path)
			)
		return ERR_FILE_CORRUPT
	return OK


func load_game() -> Dictionary:
	if not persistence_enabled:
		load_completed.emit(false)
		return {}
	if not _initial_backup_checked and prepare_backup() != OK:
		load_completed.emit(false)
		return {}
	if not FileAccess.file_exists(SAVE_PATH):
		load_completed.emit(false)
		return {}
	var data: Dictionary = _read_save_dictionary(SAVE_PATH)
	if data.is_empty():
		_block_writes(ERR_FILE_CORRUPT, "Invalid save JSON; original file and raw backup are preserved")
		load_completed.emit(false)
		return {}
	var source_version: int = int(data.get("schema_version", data.get("version", 1)))
	if source_version < 1 or source_version > 2:
		_block_writes(ERR_FILE_UNRECOGNIZED, "Unsupported save version %d; refusing to downgrade or overwrite" % source_version)
		load_completed.emit(false)
		return {}
	LootManager.migrate_save_data(data)
	GameManager.apply_save_dict(data)
	PetManager.apply_save_dict(Dictionary(data.get("pets", {})))
	RebirthManager.sync_unlocked_classes()
	write_guard_error = OK
	write_guard_reason = ""
	load_completed.emit(true)
	return data


func _read_save_dictionary(path: String) -> Dictionary:
	if not FileAccess.file_exists(path):
		return {}
	var save_file := FileAccess.open(path, FileAccess.READ)
	if save_file == null:
		return {}
	var source_text: String = save_file.get_as_text()
	var read_error: Error = save_file.get_error()
	save_file.close()
	if read_error != OK:
		return {}
	var parsed_data: Variant = JSON.parse_string(source_text)
	if not parsed_data is Dictionary:
		return {}
	return (parsed_data as Dictionary).duplicate(true)


func _is_valid_save_file(path: String) -> bool:
	return not _read_save_dictionary(path).is_empty()


func _remove_if_exists(path: String) -> void:
	if FileAccess.file_exists(path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(path))


func _block_writes(error: Error, reason: String) -> Error:
	write_guard_error = error
	write_guard_reason = reason
	last_save_error = reason
	save_blocked.emit(reason)
	push_warning(reason)
	return error


func _notification(what: int) -> void:
	if what == NOTIFICATION_WM_CLOSE_REQUEST:
		var error: Error = save_game()
		if error == OK:
			get_tree().paused = false
			get_tree().quit()
		else:
			save_blocked.emit("종료 취소: 저장에 실패했습니다. %s" % last_save_error)


func set_autosave_enabled(enabled: bool) -> void:
	autosave_enabled = enabled
	_last_autosave_msec = Time.get_ticks_msec()


func set_persistence_enabled(enabled: bool) -> void:
	persistence_enabled = enabled
	_last_autosave_msec = Time.get_ticks_msec()
