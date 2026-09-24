extends Node

signal rebirth_completed(new_count: int)
signal permanent_upgrade_purchased(stat_name: String)

const CLASS_RESOURCE_DIRECTORY: String = "res://resources/classes/"


func required_level() -> int:
	return 20 + GameManager.rebirth_count * 10


func reward_points() -> int:
	return 1 + floori(float(GameManager.level) / 10.0)


func can_rebirth() -> bool:
	return GameManager.level >= required_level()


func rebirth() -> bool:
	if not can_rebirth():
		return false
	var reward: int = reward_points()
	GameManager.rebirth_count += 1
	GameManager.rebirth_points += reward
	sync_unlocked_classes()
	GameManager.reset_run_progress()
	GameManager.recalculate_stats(false)
	rebirth_completed.emit(GameManager.rebirth_count)
	GameManager.notification_requested.emit("환생 완료! 영구 포인트 +%d" % reward, Color("ffd166"))
	AudioManager.play_sfx("rebirth")
	SaveManager.save_game()
	return true


func purchase_permanent_upgrade(stat_name: String) -> bool:
	if GameManager.rebirth_points <= 0 or not GameManager.permanent_upgrades.has(stat_name):
		return false
	GameManager.rebirth_points -= 1
	GameManager.permanent_upgrades[stat_name] = int(GameManager.permanent_upgrades[stat_name]) + 1
	GameManager.recalculate_stats()
	permanent_upgrade_purchased.emit(stat_name)
	return true


func gold_multiplier() -> float:
	return pow(1.1, GameManager.rebirth_count)


func sync_unlocked_classes() -> void:
	for file_name: String in DirAccess.get_files_at(CLASS_RESOURCE_DIRECTORY):
		if not file_name.ends_with(".tres"):
			continue
		var resource: Resource = load(CLASS_RESOURCE_DIRECTORY + file_name)
		if not resource is ClassData:
			continue
		var class_data := resource as ClassData
		if GameManager.rebirth_count >= class_data.unlock_rebirths and not GameManager.unlocked_classes.has(class_data.id):
			GameManager.unlocked_classes.append(class_data.id)
