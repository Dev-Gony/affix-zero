extends Node2D
class_name EnemyAI

const ENEMY_ATLAS: Texture2D = preload("res://assets/sprites/enemy_atlas_alpha.png")
const ENEMY_REGIONS: Dictionary = {
	"slime": Vector2i(0, 0), "bat": Vector2i(1, 0), "skeleton": Vector2i(2, 0), "goblin": Vector2i(3, 0),
	"dark_knight": Vector2i(0, 1), "lich": Vector2i(1, 1), "dragon": Vector2i(2, 1), "demon_lord": Vector2i(3, 1),
}

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
var _spawn_reveal_left: float = 0.32


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
	if _spawn_reveal_left > 0.0:
		_spawn_reveal_left = maxf(0.0, _spawn_reveal_left - delta)
		queue_redraw()
		return
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
	var enemy_id: String = enemy_data.id if enemy_data != null else "slime"
	var atlas_cell: Vector2i = ENEMY_REGIONS.get(enemy_id, Vector2i.ZERO)
	var source := Rect2(atlas_cell.x * 384, atlas_cell.y * 512, 384, 512)
	var sprite_size: float = clampf(radius * 3.7, 28.0, 54.0)
	var reveal: float = clampf(1.0 - _spawn_reveal_left / 0.32, 0.0, 1.0)
	var sprite_modulate := Color(1.65, 1.65, 1.65, reveal) if _hit_flash_left > 0.0 else Color(1.0, 1.0, 1.0, reveal)
	draw_circle(Vector2(0, sprite_size * 0.28), sprite_size * 0.28, Color(0, 0, 0, 0.35 * reveal))
	if _spawn_reveal_left > 0.0:
		draw_arc(Vector2.ZERO, 10.0 + reveal * 9.0, 0.0, TAU, 24, Color(body_color, 0.75 * (1.0 - reveal)), 2.0)
	draw_texture_rect_region(ENEMY_ATLAS, Rect2(-sprite_size * 0.5, -sprite_size * 0.58, sprite_size, sprite_size), source, sprite_modulate)
	var bar_width: float = maxf(20.0, sprite_size * 0.7)
	var bar_y: float = -sprite_size * 0.52 - 5.0
	draw_rect(Rect2(-bar_width * 0.5, bar_y, bar_width, 3), Color("351822"), true)
	draw_rect(Rect2(-bar_width * 0.5, bar_y, bar_width * clampf(hp / max_hp, 0.0, 1.0), 3), Color("ef4444"), true)
