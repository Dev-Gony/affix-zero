extends Node

# Optional authored audio always wins. When a file is absent, a lightweight
# procedural PCM fallback keeps local and exported builds from feeling silent.
const SAMPLE_RATE: int = 22050
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

const BGM_MELODIES: Dictionary = {
	"entrance": [220.0, 261.63, 293.66, 261.63, 196.0, 220.0, 174.61, 0.0,
		220.0, 261.63, 329.63, 293.66, 261.63, 220.0, 196.0, 0.0],
	"depths": [164.81, 196.0, 220.0, 174.61, 146.83, 164.81, 130.81, 0.0,
		164.81, 220.0, 246.94, 196.0, 174.61, 146.83, 130.81, 0.0],
	"abyss": [110.0, 116.54, 146.83, 123.47, 103.83, 110.0, 92.5, 0.0,
		110.0, 146.83, 155.56, 123.47, 116.54, 103.83, 92.5, 0.0],
}

const SFX_NOTES: Dictionary = {
	"basic_attack": [520.0, 260.0],
	"critical_hit": [880.0, 1174.66, 1760.0],
	"melee_spin": [220.0, 330.0, 440.0, 330.0],
	"fireball": [196.0, 246.94, 392.0],
	"shield_charge": [130.81, 164.81, 98.0],
	"chain_lightning": [659.25, 987.77, 783.99, 1318.51],
	"multi_slash": [740.0, 370.0, 880.0, 440.0, 1046.5],
	"holy_nova": [523.25, 659.25, 783.99, 1046.5],
	"monster_death": [196.0, 146.83, 98.0],
	"item_drop": [523.25, 659.25],
	"legend_drop": [523.25, 659.25, 783.99, 1046.5, 1318.51],
	"level_up": [392.0, 523.25, 659.25, 783.99],
	"rebirth": [130.81, 196.0, 261.63, 392.0, 523.25],
	"ui_click": [620.0],
}

const SFX_NOTE_LENGTHS: Dictionary = {
	"basic_attack": 0.045,
	"critical_hit": 0.055,
	"melee_spin": 0.055,
	"fireball": 0.075,
	"shield_charge": 0.075,
	"chain_lightning": 0.04,
	"multi_slash": 0.035,
	"holy_nova": 0.09,
	"monster_death": 0.065,
	"item_drop": 0.075,
	"legend_drop": 0.08,
	"level_up": 0.08,
	"rebirth": 0.11,
	"ui_click": 0.025,
}

var _bgm_players: Dictionary = {}
var _sfx_players: Dictionary = {}
var _current_bgm: String = ""
var master_volume: float = 1.0


func _ready() -> void:
	for track_name: String in BGM_PATHS:
		var player := AudioStreamPlayer2D.new()
		player.name = "BGM_%s" % track_name.capitalize()
		player.volume_db = -18.0
		var path: String = BGM_PATHS[track_name]
		player.stream = load(path) if ResourceLoader.exists(path) else _create_bgm_stream(track_name)
		if player.stream is AudioStreamOggVorbis:
			(player.stream as AudioStreamOggVorbis).loop = true
		add_child(player)
		_bgm_players[track_name] = player
	for effect_name: String in SFX_PATHS:
		var player := AudioStreamPlayer2D.new()
		player.name = "SFX_%s" % effect_name.capitalize()
		player.volume_db = -8.0 if effect_name != "ui_click" else -14.0
		var path: String = SFX_PATHS[effect_name]
		player.stream = load(path) if ResourceLoader.exists(path) else _create_sfx_stream(effect_name)
		add_child(player)
		_sfx_players[effect_name] = player


func set_master_volume(value: float) -> void:
	master_volume = clampf(value, 0.0, 1.0)
	var db: float = linear_to_db(master_volume) if master_volume > 0.001 else -80.0
	for player: AudioStreamPlayer2D in _bgm_players.values():
		player.volume_db = db - 10.0
	for player: AudioStreamPlayer2D in _sfx_players.values():
		player.volume_db = db - 2.0


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


func has_complete_audio_bank() -> bool:
	if _bgm_players.size() != BGM_PATHS.size() or _sfx_players.size() != SFX_PATHS.size():
		return false
	for player: AudioStreamPlayer2D in _bgm_players.values():
		if player.stream == null:
			return false
	for player: AudioStreamPlayer2D in _sfx_players.values():
		if player.stream == null:
			return false
	return true


func _create_bgm_stream(track_name: String) -> AudioStreamWAV:
	var melody: Array = BGM_MELODIES.get(track_name, BGM_MELODIES["entrance"])
	var note_length: float = 0.22
	var sample_count: int = int(melody.size() * note_length * SAMPLE_RATE)
	var pcm := PackedByteArray()
	pcm.resize(sample_count * 2)
	for sample_index: int in sample_count:
		var time: float = float(sample_index) / float(SAMPLE_RATE)
		var note_index: int = mini(int(time / note_length), melody.size() - 1)
		var frequency: float = float(melody[note_index])
		var note_time: float = fmod(time, note_length)
		var note_envelope: float = minf(note_time / 0.012, 1.0) * minf((note_length - note_time) / 0.035, 1.0)
		var value: float = 0.0
		if frequency > 0.0:
			var phase: float = TAU * frequency * time
			var lead: float = sin(phase) * 0.55 + _square_wave(phase) * 0.16
			var bass: float = sin(phase * 0.25) * 0.34
			value = (lead + bass) * note_envelope * 0.34
		_write_pcm_sample(pcm, sample_index, value)
	return _make_wav(pcm, true)


func _create_sfx_stream(effect_name: String) -> AudioStreamWAV:
	var notes: Array = SFX_NOTES.get(effect_name, [440.0])
	var note_length: float = float(SFX_NOTE_LENGTHS.get(effect_name, 0.06))
	var sample_count: int = int(notes.size() * note_length * SAMPLE_RATE)
	var pcm := PackedByteArray()
	pcm.resize(sample_count * 2)
	for sample_index: int in sample_count:
		var time: float = float(sample_index) / float(SAMPLE_RATE)
		var note_index: int = mini(int(time / note_length), notes.size() - 1)
		var frequency: float = float(notes[note_index])
		var note_time: float = fmod(time, note_length)
		var progress: float = note_time / note_length
		var phase: float = TAU * frequency * time
		var tone: float = sin(phase) * 0.62 + _square_wave(phase) * 0.18
		if effect_name in ["shield_charge", "monster_death", "rebirth"]:
			tone = sin(phase) * 0.55 + sin(phase * 0.5) * 0.3
		elif effect_name in ["chain_lightning", "multi_slash"]:
			tone = _square_wave(phase) * 0.42 + sin(phase * 2.0) * 0.28
		var envelope: float = minf(note_time / 0.004, 1.0) * pow(1.0 - progress, 1.35)
		_write_pcm_sample(pcm, sample_index, tone * envelope * 0.5)
	return _make_wav(pcm, false)


func _make_wav(pcm: PackedByteArray, looped: bool) -> AudioStreamWAV:
	var stream := AudioStreamWAV.new()
	stream.format = AudioStreamWAV.FORMAT_16_BITS
	stream.mix_rate = SAMPLE_RATE
	stream.stereo = false
	stream.data = pcm
	if looped:
		stream.loop_mode = AudioStreamWAV.LOOP_FORWARD
		stream.loop_begin = 0
		stream.loop_end = pcm.size() / 2
	return stream


func _write_pcm_sample(pcm: PackedByteArray, sample_index: int, value: float) -> void:
	var sample_value: int = int(clampf(value, -1.0, 1.0) * 32767.0)
	var encoded: int = sample_value if sample_value >= 0 else sample_value + 65536
	pcm[sample_index * 2] = encoded & 0xff
	pcm[sample_index * 2 + 1] = (encoded >> 8) & 0xff


func _square_wave(phase: float) -> float:
	return 1.0 if sin(phase) >= 0.0 else -1.0
