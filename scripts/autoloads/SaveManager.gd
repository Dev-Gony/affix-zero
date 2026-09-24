extends Node

signal save_completed
signal load_completed(found_save: bool)

const SAVE_PATH: String = "user://save.json"
const AUTOSAVE_INTERVAL: float = 30.0

var _last_autosave_msec: int = 0
var persistence_enabled: bool = true
var autosave_enabled: bool = true


func _ready() -> void:
	load_game()
	process_mode = Node.PROCESS_MODE_ALWAYS
	_last_autosave_msec = Time.get_ticks_msec()


func _process(_delta: float) -> void:
	var now_msec: int = Time.get_ticks_msec()
	if autosave_enabled and now_msec - _last_autosave_msec >= int(AUTOSAVE_INTERVAL * 1000.0):
		_last_autosave_msec = now_msec
		save_game()


func save_game(data: Dictionary = {}) -> Error:
	if not persistence_enabled:
		return OK
	var save_data: Dictionary = data if not data.is_empty() else GameManager.to_save_dict()
	var save_file := FileAccess.open(SAVE_PATH, FileAccess.WRITE)
	if save_file == null:
		return FileAccess.get_open_error()

	save_file.store_string(JSON.stringify(save_data, "\t"))
	save_completed.emit()
	return OK


func load_game() -> Dictionary:
	if not FileAccess.file_exists(SAVE_PATH):
		load_completed.emit(false)
		return {}

	var save_file := FileAccess.open(SAVE_PATH, FileAccess.READ)
	if save_file == null:
		push_warning("Unable to open save file: %s" % SAVE_PATH)
		return {}

	var parsed_data: Variant = JSON.parse_string(save_file.get_as_text())
	if parsed_data is Dictionary:
		var data: Dictionary = parsed_data as Dictionary
		LootManager.migrate_save_data(data)
		GameManager.apply_save_dict(data)
		RebirthManager.sync_unlocked_classes()
		load_completed.emit(true)
		return data

	push_warning("Save file does not contain a valid JSON object: %s" % SAVE_PATH)
	load_completed.emit(false)
	return {}


func _notification(what: int) -> void:
	if what == NOTIFICATION_WM_CLOSE_REQUEST:
		save_game()


func set_autosave_enabled(enabled: bool) -> void:
	autosave_enabled = enabled
	_last_autosave_msec = Time.get_ticks_msec()


func set_persistence_enabled(enabled: bool) -> void:
	persistence_enabled = enabled
	_last_autosave_msec = Time.get_ticks_msec()
