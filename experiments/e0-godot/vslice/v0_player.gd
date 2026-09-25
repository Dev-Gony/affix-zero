class_name V0Player
extends Node2D

signal attack_landed(target: Node, damage: int, world_position: Vector2)
signal died

var atlas: Texture2D
var atlas_cell := Vector2i(0, 0)
var max_hp: int = 180
var hp: int = 180
var attack_damage: int = 34
var move_speed: float = 108.0
var attack_range: float = 46.0
var attack_interval: float = 0.52

var target: Node2D
var attack_cd: float = 0.0
var windup_left: float = 0.0
var attack_fx_left: float = 0.0
var hit_flash_left: float = 0.0
var facing: float = 1.0
var dead: bool = false
var kills: int = 0

func configure(texture: Texture2D) -> void:
	atlas = texture
	queue_redraw()

func set_target(next_target: Node2D) -> void:
	target = next_target

func _process(delta: float) -> void:
	if dead:
		return
	attack_cd = maxf(0.0, attack_cd - delta)
	hit_flash_left = maxf(0.0, hit_flash_left - delta)
	attack_fx_left = maxf(0.0, attack_fx_left - delta)

	if windup_left > 0.0:
		windup_left = maxf(0.0, windup_left - delta)
		if is_zero_approx(windup_left):
			_resolve_attack()
		queue_redraw()
		return

	if target == null or not is_instance_valid(target):
		queue_redraw()
		return
	if target.has_method("is_dead") and bool(target.call("is_dead")):
		target = null
		return

	var distance := global_position.distance_to(target.global_position)
	var direction := global_position.direction_to(target.global_position)
	if absf(direction.x) > 0.05:
		facing = 1.0 if direction.x >= 0.0 else -1.0

	if distance > attack_range:
		global_position += direction * move_speed * delta
	elif attack_cd <= 0.0:
		attack_cd = attack_interval
		windup_left = 0.11
		attack_fx_left = 0.22
	queue_redraw()

func _resolve_attack() -> void:
	if target == null or not is_instance_valid(target):
		return
	if target.has_method("is_dead") and bool(target.call("is_dead")):
		return
	if global_position.distance_to(target.global_position) > attack_range + 8.0:
		return
	if target.has_method("take_damage"):
		target.call("take_damage", attack_damage, global_position)
		attack_landed.emit(target, attack_damage, target.global_position)

func take_damage(raw_damage: int) -> void:
	if dead:
		return
	hp = maxi(0, hp - raw_damage)
	hit_flash_left = 0.10
	if hp <= 0:
		dead = true
		died.emit()
	queue_redraw()

func heal(amount: int) -> void:
	hp = mini(max_hp, hp + amount)

func _draw() -> void:
	if atlas == null:
		return
	var cell_size := Vector2(float(atlas.get_width()) / 3.0, float(atlas.get_height()) / 2.0)
	var source := Rect2(Vector2(atlas_cell) * cell_size, cell_size)
	var body_size := Vector2(48, 48)
	var lunge := 0.0
	if windup_left > 0.0:
		lunge = -3.0
	elif attack_fx_left > 0.0:
		lunge = 4.0
	var modulate := Color(1.75, 1.75, 1.75, 1.0) if hit_flash_left > 0.0 else Color.WHITE
	draw_circle(Vector2(0, 13), 11.0, Color(0, 0, 0, 0.42))
	draw_set_transform(Vector2(facing * lunge, 0), 0.0, Vector2(facing, 1.0))
	draw_texture_rect_region(atlas, Rect2(-24, -34, body_size.x, body_size.y), source, modulate)
	draw_set_transform(Vector2.ZERO, 0.0, Vector2.ONE)
	if attack_fx_left > 0.0:
		var alpha := clampf(attack_fx_left / 0.22, 0.0, 1.0)
		var base_angle := 0.0 if facing > 0.0 else PI
		draw_arc(Vector2(facing * 9.0, -5), 28.0, base_angle - 0.78, base_angle + 0.78, 18, Color(1.0, 0.70, 0.32, alpha), 4.0)
