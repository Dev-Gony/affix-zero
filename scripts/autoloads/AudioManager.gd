extends Node

# Audio files are intentionally optional. Add OGG/WAV files at these paths to enable playback.
const BGM_PATHS: Dictionary = {
	"entrance": "res://assets/bgm/floors_01_05.ogg",
	"depths": "res://assets/bgm/floors_06_15.ogg",
	"abyss": "res://assets/bgm/floors_16_plus.ogg",
}
const SFX_PATHS: Dictionary = {
	"basic_attack": "res://assets/sfx/basic_attack.ogg",
	"critical_hit": "res://assets/sfx/critical_hit.ogg",
	"melee_spin": "res://assets/sfx/melee_spin.ogg",
	"fireball": "res://assets/sfx/fireball.ogg",
	"shield_charge": "res://assets/sfx/shield_charge.ogg",
	"chain_lightning": "res://assets/sfx/chain_lightning.ogg",
	"multi_slash": "res://assets/sfx/multi_slash.ogg",
	"holy_nova": "res://assets/sfx/holy_nova.ogg",
	"monster_death": "res://assets/sfx/monster_death.ogg",
	"item_drop": "res://assets/sfx/item_drop.ogg",
	"legend_drop": "res://assets/sfx/legend_drop.ogg",
	"level_up": "res://assets/sfx/level_up.ogg",
	"rebirth": "res://assets/sfx/rebirth.ogg",
	"ui_click": "res://assets/sfx/ui_click.ogg",
}

var _bgm_players: Dictionary = {}
var _sfx_players: Dictionary = {}
var _current_bgm: String = ""


func _ready() -> void:
	for track_name: String in BGM_PATHS:
		var player := AudioStreamPlayer2D.new()
		player.name = "BGM_%s" % track_name.capitalize()
		player.volume_db = -10.0
		var path: String = BGM_PATHS[track_name]
		if ResourceLoader.exists(path):
			player.stream = load(path)
		add_child(player)
		_bgm_players[track_name] = player
	for effect_name: String in SFX_PATHS:
		var player := AudioStreamPlayer2D.new()
		player.name = "SFX_%s" % effect_name.capitalize()
		var path: String = SFX_PATHS[effect_name]
		if ResourceLoader.exists(path):
			player.stream = load(path)
		add_child(player)
		_sfx_players[effect_name] = player


func play_bgm_for_floor(current_floor: int) -> void:
	var track_name: String = "entrance" if current_floor <= 5 else ("depths" if current_floor <= 15 else "abyss")
	if track_name == _current_bgm:
		return
	for player: AudioStreamPlayer2D in _bgm_players.values():
		player.stop()
	_current_bgm = track_name
	var selected: AudioStreamPlayer2D = _bgm_players.get(track_name)
	if selected != null and selected.stream != null:
		selected.play()


func play_sfx(effect_name: String) -> void:
	var player: AudioStreamPlayer2D = _sfx_players.get(effect_name)
	if player != null and player.stream != null:
		player.play()
