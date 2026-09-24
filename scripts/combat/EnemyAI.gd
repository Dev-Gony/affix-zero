extends Node2D
class_name EnemyAI

signal died(enemy: EnemyAI, world_position: Vector2, fragment_color: Color, xp_reward: int, gold_reward: int)
signal attacked_player(raw_damage: float)
signal damage_received(world_position: Vector2, damage: int, critical: bool)

var enemy_data: EnemyData
var target: Node2D
var max_hp: float = 1.0
var hp: float = 1.0
var attack: float = 1.0
var defense: float = 0.0
var move_speed: float = 20.0
var attack_cooldown: float = 1.0
var radius: float = 8.0
var xp_reward: int = 1
var gold_reward: int = 1
var body_color: Color = Color.WHITE
var _attack_time_left: float = 0.0
var _dead: bool = false
var _hit_flash_left: float = 0.0


func setup(data: EnemyData, current_floor: int, player_target: Node2D) -> void:
	enemy_data = data
	target = player_target
	var scale_factor: float = 1.0 + (current_floor - 1) * 0.3
	max_hp = data.base_hp * scale_factor
	hp = max_hp
	attack = data.base_atk * scale_factor
	defense = data.base_def * scale_factor
	move_speed = data.move_speed
	attack_cooldown = data.attack_cooldown
	radius = data.radius
	xp_reward = maxi(1, roundi(data.xp_reward * (1.0 + (current_floor - 1) * 0.12)))
	gold_reward = maxi(1, roundi(data.gold_reward * (1.0 + (current_floor - 1) * 0.10)))
	body_color = data.color
	queue_redraw()


func _process(delta: float) -> void:
	if _dead or target == null or not is_instance_valid(target):
		return
	if _hit_flash_left > 0.0:
		_hit_flash_left = maxf(0.0, _hit_flash_left - delta)
		queue_redraw()
	_attack_time_left = maxf(0.0, _attack_time_left - delta)
	var distance: float = global_position.distance_to(target.global_position)
	if distance > radius + 11.0:
		global_position += global_position.direction_to(target.global_position) * move_speed * delta
	elif _attack_time_left <= 0.0:
		_attack_time_left = attack_cooldown
		attacked_player.emit(attack)


func take_hit(result: Dictionary) -> void:
	if _dead:
		return
	var damage: int = int(result.get("damage", 1))
	var critical: bool = bool(result.get("critical", false))
	hp -= damage
	_hit_flash_left = 0.09
	damage_received.emit(global_position, damage, critical)
	queue_redraw()
	if hp <= 0.0:
		_die()


func _die() -> void:
	if _dead:
		return
	_dead = true
	died.emit(self, global_position, body_color, xp_reward, gold_reward)
	queue_free()


func _draw() -> void:
	var draw_color: Color = body_color.lightened(0.55) if _hit_flash_left > 0.0 else body_color
	draw_circle(Vector2(0, radius * 0.55), Vector2(radius, radius * 0.35).x, Color(0, 0, 0, 0.28))
	match enemy_data.id if enemy_data != null else "slime":
		"slime":
			draw_circle(Vector2.ZERO, radius, draw_color)
			draw_rect(Rect2(-radius, 0, radius * 2.0, radius), draw_color, true)
		"bat":
			draw_colored_polygon(PackedVector2Array([Vector2(-radius * 2, 0), Vector2(-3, -5), Vector2(0, 5), Vector2(3, -5), Vector2(radius * 2, 0), Vector2(0, radius)]), draw_color)
		"skeleton":
			draw_circle(Vector2(0, -3), radius * 0.72, draw_color)
			draw_rect(Rect2(-radius * 0.55, 2, radius * 1.1, radius), draw_color.darkened(0.12), true)
		"goblin":
			draw_colored_polygon(PackedVector2Array([Vector2(-radius, -radius), Vector2(0, -radius * 0.55), Vector2(radius, -radius), Vector2(radius * 0.7, radius), Vector2(-radius * 0.7, radius)]), draw_color)
		"dark_knight":
			draw_rect(Rect2(-radius, -radius, radius * 2, radius * 2), draw_color, true)
			draw_rect(Rect2(-radius * 0.7, -radius * 1.35, radius * 1.4, radius * 0.5), draw_color.lightened(0.18), true)
		"lich":
			draw_colored_polygon(PackedVector2Array([Vector2(0, -radius * 1.3), Vector2(radius, radius), Vector2(-radius, radius)]), draw_color)
			draw_circle(Vector2(0, -radius * 0.55), radius * 0.5, draw_color.lightened(0.25))
		"dragon":
			draw_colored_polygon(PackedVector2Array([Vector2(-radius * 1.6, 0), Vector2(-radius * 0.4, -radius), Vector2(0, -radius * 0.4), Vector2(radius * 0.4, -radius), Vector2(radius * 1.6, 0), Vector2(0, radius)]), draw_color)
		"demon_lord":
			draw_circle(Vector2.ZERO, radius, draw_color)
			draw_colored_polygon(PackedVector2Array([Vector2(-radius, -radius * 0.4), Vector2(-radius * 1.5, -radius * 1.5), Vector2(-radius * 0.3, -radius), Vector2(radius * 0.3, -radius), Vector2(radius * 1.5, -radius * 1.5), Vector2(radius, -radius * 0.4)]), draw_color.darkened(0.15))
	draw_rect(Rect2(-radius + 2, -2, 3, 3), Color.WHITE, true)
	draw_rect(Rect2(radius - 5, -2, 3, 3), Color.WHITE, true)
	var bar_width: float = radius * 2.0
	draw_rect(Rect2(-radius, -radius - 7, bar_width, 3), Color("351822"), true)
	draw_rect(Rect2(-radius, -radius - 7, bar_width * clampf(hp / max_hp, 0.0, 1.0), 3), Color("ef4444"), true)
