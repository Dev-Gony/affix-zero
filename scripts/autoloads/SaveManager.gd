extends Node

const SAVE_PATH: String = "user://save.json"


func save_game(data: Dictionary) -> Error:
	var save_file := FileAccess.open(SAVE_PATH, FileAccess.WRITE)
	if save_file == null:
		return FileAccess.get_open_error()

	save_file.store_string(JSON.stringify(data))
	return OK


func load_game() -> Dictionary:
	if not FileAccess.file_exists(SAVE_PATH):
		return {}

	var save_file := FileAccess.open(SAVE_PATH, FileAccess.READ)
	if save_file == null:
		push_warning("Unable to open save file: %s" % SAVE_PATH)
		return {}

	var parsed_data: Variant = JSON.parse_string(save_file.get_as_text())
	if parsed_data is Dictionary:
		return parsed_data as Dictionary

	push_warning("Save file does not contain a valid JSON object: %s" % SAVE_PATH)
	return {}
