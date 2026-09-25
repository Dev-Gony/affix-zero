extends CanvasLayer

var _info: Dictionary = {}
var _badge: Button
var _panel: PanelContainer
var _details: Label


func _ready() -> void:
	layer = 100
	process_mode = Node.PROCESS_MODE_ALWAYS
	_info = BuildInfo.read_info()
	_badge = Button.new()
	_badge.position = Vector2(6, 378)
	_badge.size = Vector2(130, 18)
	_badge.add_theme_font_size_override("font_size", 7)
	_badge.tooltip_text = "빌드 / 백업 진단 · Ctrl+Shift+9 (진단 표시는 전투를 정지하지 않습니다)"
	_badge.pressed.connect(toggle_details)
	add_child(_badge)
	_panel = PanelContainer.new()
	_panel.position = Vector2(6, 204)
	_panel.custom_minimum_size = Vector2(308, 162)
	_panel.mouse_filter = Control.MOUSE_FILTER_IGNORE
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.04, 0.06, 0.09, 0.97)
	style.border_color = Color("8fcbff")
	style.set_border_width_all(1)
	style.content_margin_left = 8
	style.content_margin_right = 8
	style.content_margin_top = 6
	style.content_margin_bottom = 6
	_panel.add_theme_stylebox_override("panel", style)
	add_child(_panel)
	_details = Label.new()
	_details.custom_minimum_size = Vector2(292, 144)
	_details.autowrap_mode = TextServer.AUTOWRAP_ARBITRARY
	_details.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_details.add_theme_font_size_override("font_size", 8)
	_panel.add_child(_details)
	_panel.visible = false
	SaveManager.save_blocked.connect(_on_save_blocked)
	_refresh()
	print("AFFIX_BUILD_INFO " + JSON.stringify(_info))


func _unhandled_key_input(event: InputEvent) -> void:
	if event is InputEventKey and event.pressed and not event.echo:
		var diagnostic_shortcut: bool = event.ctrl_pressed and event.shift_pressed and event.keycode == KEY_9
		if diagnostic_shortcut:
			toggle_details()
			get_viewport().set_input_as_handled()


func toggle_details() -> void:
	_refresh()
	_panel.visible = not _panel.visible


func _on_save_blocked(_reason: String) -> void:
	_refresh()
	_panel.visible = true


func _refresh() -> void:
	_info = BuildInfo.read_info()
	var source: String = String(_info.get("source_commit", "unidentified"))
	var short_id: String = source.substr(0, 7) if source.length() == 40 else "ID 없음"
	var dirty: String = " *" if bool(_info.get("dirty", false)) else ""
	var version_label: String = String(_info.get("version", "uiux-unknown")).replace("uiux-pr-", "").to_upper()
	_badge.text = "%s  %s%s  Ctrl+Shift+9" % [version_label, short_id, dirty]
	var status: String = String(SaveManager.backup_status.get("status", "test_mode" if not SaveManager.persistence_enabled else "not_checked"))
	if SaveManager.write_guard_error != OK:
		_badge.text = "저장 잠김 · Ctrl+Shift+9"
		_badge.add_theme_color_override("font_color", Color("ff9c9c"))
	_details.text = "%s 진단 (전투 계속) · Ctrl+Shift+9 닫기\n버전: %s\n커밋: %s%s\n브랜치: %s\n빌드 UTC: %s\n백업: %s\n저장 위치: %s\n백업 위치: %s\n%s" % [
		String(_info.get("stage", "UIUX")),
		String(_info.get("version", "")), source, dirty, String(_info.get("branch", "")),
		String(_info.get("built_at_utc", "")), status,
		ProjectSettings.globalize_path(SaveManager.SAVE_PATH),
		ProjectSettings.globalize_path(String(SaveManager.backup_status.get("path", SaveBackup.DEFAULT_DIRECTORY))),
		SaveManager.write_guard_reason
	]
