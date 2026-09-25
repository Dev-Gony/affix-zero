class_name V0Enemy
extends Node2D

signal died(enemy: V0Enemy, reward_xp: int, reward_gold: int, world_position: Vector2)
signal attacked_player(damage: int)

var atlas: Texture2D
var atlas_cell := Vector2i.ZERO
var enemy_id: StringName = &"slime"
var max_hp: int = 60
var hp: int = 60
var damage: int = 7
var speed: float = 52.0
var attack_range: float = 24.0
var attack_interval: float = 0.95
var reward_xp: int = 8
var reward_gold: int = 2
var scale_factor: float = 1.0
var is_boss: bool = false

var target: V0Player
var attack_cd: float = 0.0
var windup_left: float = 0.0
var hit_flash_left: float = 0.0
var death_left: float = 0.0
var facing: float = -1.0
var _dead: bool = false
var _stride: float = 0.0

func configure(texture: Texture2D, id: StringName, cell: Vector2i, player: V0Player, boss: bool = false) -> void:
	atlas = texture
	enemy_id = id
	atlas_cell = cell
	target = player
	is_boss = boss
	if boss:
		max_hp = 620
		hp = max_hp
		damage = 16
		speed = 40.0
		attack_range = 36.0
		attack_interval = 0.82
		reward_xp = 80
		reward_gold = 30
		scale_factor = 1.55
	queue_redraw()

func _process(delta: float) -> void:
	if _dead:
		death_left = maxf(0.0, death_left - delta)
		modulate.a = clampf(death_left / 0.28, 0.0, 1.0)
		scale = Vector2.ONE * lerpf(scale_factor * 0.75, scale_factor, modulate.a)
		if is_zero_approx(death_left):
			queue_free()
		return
	if target == null or target.dead:
		return

	attack_cd = maxf(0.0, attack_cd - delta)
	hit_flash_left = maxf(0.0, hit_flash_left - delta)
	_stride += delta

	if windup_left > 0.0:
		windup_left = maxf(0.0, windup_left - delta)
		if is_zero_approx(windup_left):
			if global_position.distance_to(target.global_position) <= attack_range + 8.0:
				attacked_player.emit(damage)
		queue_redraw()
		return

	var to_player := global_position.direction_to(target.global_position)
	if absf(to_player.x) > 0.05:
		facing = 1.0 if to_player.x >= 0.0 else -1.0
	var distance := global_position.distance_to(target.global_position)

	if distance > attack_range:
		var separation := Vector2.ZERO
		var parent := get_parent()
		if parent != null and parent.has_method("separation_for"):
			separation = parent.call("separation_for", self)
		var move_dir := (to_player + separation * 1.35).normalized()
		global_position += move_dir * speed * delta
	elif attack_cd <= 0.0:
		attack_cd = attack_interval
		windup_left = 0.18
	queue_redraw()

func take_damage(amount: int, source_position: Vector2) -> void:
	if _dead:
		return
	hp = maxi(0, hp - amount)
	hit_flash_left = 0.10
	var knock_dir := source_position.direction_to(global_position)
	global_position += knock_dir * (5.0 if not is_boss else 2.0)
	if hp <= 0:
		_dead = true
		death_left = 0.28
		died.emit(self, reward_xp, reward_gold, global_position)
	queue_redraw()

func is_dead() -> bool:
	return _dead

func _draw() -> void:
	if atlas == null:
		return
	var cell_size := Vector2(float(atlas.get_width()) / 4.0, float(atlas.get_height()) / 2.0)
	var source := Rect2(Vector2(atlas_cell) * cell_size, cell_size)
	var size := 38.0 * scale_factor
	var bob := sin(_stride * 8.0 + float(get_instance_id() % 7)) * 1.2
	var flash := Color(1.8, 1.8, 1.8, 1.0) if hit_flash_left > 0.0 else Color.WHITE
	draw_circle(Vector2(0, size * 0.24), size * 0.22, Color(0, 0, 0, 0.36))
	if is_boss:
		draw_arc(Vector2(0, -2), size * 0.56, 0, TAU, 30, Color(0.92, 0.19, 0.24, 0.65), 2.0)
	draw_set_transform(Vector2(0, bob), 0.0, Vector2(facing, 1.0))
	draw_texture_rect_region(atlas, Rect2(-size * 0.5, -size * 0.72, size, size), source, flash)
	draw_set_transform(Vector2.ZERO, 0.0, Vector2.ONE)
	var ratio := float(hp) / float(max_hp)
	var width := 34.0 * scale_factor
	draw_rect(Rect2(-width * 0.5, -size * 0.88, width, 4), Color("2a1619"))
	draw_rect(Rect2(-width * 0.5 + 1, -size * 0.88 + 1, (width - 2.0) * ratio, 2), Color("e65a61" if not is_boss else "ff3447"))
