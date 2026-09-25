extends Node

signal pause_changed(paused: bool, reasons: Array[String])

var _reasons: Dictionary = {}


func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS


func acquire(reason: String) -> void:
	if reason.is_empty():
		return
	_reasons[reason] = int(_reasons.get(reason, 0)) + 1
	_apply()


func release(reason: String) -> void:
	if not _reasons.has(reason):
		return
	var count: int = int(_reasons[reason]) - 1
	if count <= 0:
		_reasons.erase(reason)
	else:
		_reasons[reason] = count
	_apply()


func release_all(reason: String) -> void:
	if _reasons.erase(reason):
		_apply()


func clear_all() -> void:
	if _reasons.is_empty() and not get_tree().paused:
		return
	_reasons.clear()
	_apply()


func is_paused() -> bool:
	return not _reasons.is_empty()


func has_reason(reason: String) -> bool:
	return _reasons.has(reason)


func reasons() -> Array[String]:
	var result: Array[String] = []
	for key: Variant in _reasons.keys():
		result.append(String(key))
	result.sort()
	return result


func enforce() -> void:
	_apply()


func _apply() -> void:
	var should_pause: bool = not _reasons.is_empty()
	get_tree().paused = should_pause
	if should_pause:
		if GameManager.game_state == GameManager.GameState.RUNNING:
			GameManager.set_game_state(GameManager.GameState.PAUSED)
	else:
		if GameManager.game_state == GameManager.GameState.PAUSED:
			GameManager.set_game_state(GameManager.GameState.RUNNING)
	pause_changed.emit(should_pause, reasons())
