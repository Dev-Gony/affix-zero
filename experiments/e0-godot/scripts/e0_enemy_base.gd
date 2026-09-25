class_name E0EnemyBase
extends Node2D

signal died(enemy: E0EnemyBase)

var enemy_id: StringName = &"enemy"
var max_hp: int = 1
var hp: int = 1
var defense: float = 0.0
var reward_gold: int = 0
var reward_xp: int = 0
var damage_events_received: int = 0
var last_attack_instance_received: int = -1
var death_signal_count: int = 0
var _dead: bool = false

func _ready() -> void:
	hp = max_hp

func take_damage(amount: int, attack_instance_id: int) -> bool:
	if _dead:
		return false
	if attack_instance_id == last_attack_instance_received:
		return false
	last_attack_instance_received = attack_instance_id
	damage_events_received += 1
	hp = maxi(0, hp - amount)
	if hp <= 0:
		_dead = true
		death_signal_count += 1
		_on_death()
		died.emit(self)
	else:
		_on_hit()
	queue_redraw()
	return true

func is_dead() -> bool:
	return _dead

func _on_hit() -> void:
	pass

func _on_death() -> void:
	pass
