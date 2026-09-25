class_name E0LootDrop
extends Node2D

signal collected(drop: E0LootDrop, gold: int, xp: int)

var drop_id: StringName = &""
var gold: int = 0
var xp: int = 0
var ledger: E0RewardLedger
var collector: E0Warrior
var pickup_radius: float = 24.0
var _collected: bool = false
var _pulse: float = 0.0

func setup(id: StringName, spawn_position: Vector2, gold_amount: int, xp_amount: int, reward_ledger: E0RewardLedger, target_collector: E0Warrior) -> void:
	drop_id = id
	global_position = spawn_position
	gold = gold_amount
	xp = xp_amount
	ledger = reward_ledger
	collector = target_collector
	queue_redraw()

func _process(delta: float) -> void:
	_pulse += delta
	queue_redraw()

func collect_by(actor: E0Warrior) -> bool:
	if _collected or actor != collector or ledger == null:
		return false
	if global_position.distance_to(actor.global_position) > pickup_radius:
		return false
	if not ledger.collect(drop_id, gold, xp):
		return false
	_collected = true
	collected.emit(self, gold, xp)
	queue_free()
	return true

func is_collected() -> bool:
	return _collected

func _draw() -> void:
	var bob := sin(_pulse * 4.0) * 2.0
	draw_circle(Vector2(0, 8), 9.0, Color(0, 0, 0, 0.30))
	var center := Vector2(0, bob)
	var diamond := PackedVector2Array([
		center + Vector2(0, -9),
		center + Vector2(8, 0),
		center + Vector2(0, 9),
		center + Vector2(-8, 0),
	])
	draw_colored_polygon(diamond, Color("f1ca5d"))
	draw_polyline(PackedVector2Array([diamond[0], diamond[1], diamond[2], diamond[3], diamond[0]]), Color("fff0a8"), 2.0)
