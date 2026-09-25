class_name E0RewardLedger
extends RefCounted

var gold: int = 0
var xp: int = 0
var total_collections: int = 0
var duplicate_rejections: int = 0
var _collected_ids: Dictionary = {}

func collect(drop_id: StringName, gold_amount: int, xp_amount: int) -> bool:
	if _collected_ids.has(drop_id):
		duplicate_rejections += 1
		return false
	_collected_ids[drop_id] = true
	gold += gold_amount
	xp += xp_amount
	total_collections += 1
	return true

func has_collected(drop_id: StringName) -> bool:
	return _collected_ids.has(drop_id)
