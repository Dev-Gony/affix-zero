extends Node

signal level_up(new_level: int)


func xp_needed(level: int) -> int:
	return level * 25 + level * level * 5


func add_xp(base_amount: int) -> void:
	var adjusted_amount: int = maxi(0, int(round(base_amount * (1.0 + GameManager.xp_bonus * 0.01))))
	GameManager.xp += adjusted_amount
	while GameManager.xp >= xp_needed(GameManager.level):
		GameManager.xp -= xp_needed(GameManager.level)
		GameManager.level += 1
		GameManager.recalculate_stats(false)
		GameManager.hp = GameManager.max_hp
		GameManager.mp = GameManager.max_mp
		level_up.emit(GameManager.level)
		AudioManager.play_sfx("level_up")
	GameManager.stats_changed.emit()
