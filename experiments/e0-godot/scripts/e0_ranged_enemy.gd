class_name E0RangedEnemy
extends E0EnemyBase

signal projectile_requested(enemy: E0RangedEnemy, origin: Vector2, direction: Vector2, damage: int, projectile_id: int)

enum State { IDLE, APPROACH, WINDUP, ACTIVE, RECOVERY, HIT, DEAD }

@export var move_speed: float = 58.0
@export var attack_range: float = 205.0
@export var preferred_min_range: float = 130.0
@export var attack_damage: int = 10

@onready var sprite: AnimatedSprite2D = $Sprite

var target: E0Warrior
var state: State = State.IDLE
var state_time_left: float = 0.0
var projectile_serial: int = 0
var projectiles_requested: int = 0
var distance_travelled: float = 0.0

const WINDUP: float = 0.42
const ACTIVE: float = 0.08
const RECOVERY: float = 0.72
const HIT_TIME: float = 0.14

func _ready() -> void:
	enemy_id = &"ranged"
	max_hp = 75
	defense = 1.0
	reward_gold = 12
	reward_xp = 6
	super._ready()
	sprite.sprite_frames = _build_frames()
	sprite.play("idle")
	queue_redraw()

func set_target(next_target: E0Warrior) -> void:
	target = next_target
	if state == State.IDLE:
		_enter_state(State.APPROACH)

func _physics_process(delta: float) -> void:
	if is_dead():
		return
	if target == null or not is_instance_valid(target) or target.is_dead():
		_enter_state(State.IDLE)
		return

	_face_target()
	match state:
		State.IDLE:
			_enter_state(State.APPROACH)
		State.APPROACH:
			var distance := global_position.distance_to(target.global_position)
			var move_direction := Vector2.ZERO
			if distance > attack_range:
				move_direction = global_position.direction_to(target.global_position)
			elif distance < preferred_min_range:
				move_direction = -global_position.direction_to(target.global_position)
			else:
				_begin_cast()
			if not move_direction.is_zero_approx():
				var step := move_direction * move_speed * delta
				global_position += step
				distance_travelled += step.length()
				_play_if_needed("walk")
		State.WINDUP, State.ACTIVE, State.RECOVERY, State.HIT:
			state_time_left = maxf(0.0, state_time_left - delta)
			if is_zero_approx(state_time_left):
				_advance_timed_state()
	queue_redraw()

func _begin_cast() -> void:
	state = State.WINDUP
	state_time_left = WINDUP
	sprite.play("attack")

func _advance_timed_state() -> void:
	match state:
		State.WINDUP:
			state = State.ACTIVE
			state_time_left = ACTIVE
			_fire_projectile()
		State.ACTIVE:
			state = State.RECOVERY
			state_time_left = RECOVERY
		State.RECOVERY:
			_enter_state(State.APPROACH)
		State.HIT:
			_enter_state(State.APPROACH)

func _fire_projectile() -> void:
	if target == null or not is_instance_valid(target) or target.is_dead():
		return
	projectile_serial += 1
	projectiles_requested += 1
	var direction := global_position.direction_to(target.global_position)
	var origin := global_position + direction * 18.0 + Vector2(0, -14)
	projectile_requested.emit(self, origin, direction, attack_damage, projectile_serial)

func state_name() -> String:
	return State.keys()[state]

func has_multiframe_contract() -> bool:
	var frames := sprite.sprite_frames
	return (
		frames.get_frame_count("idle") >= 2
		and frames.get_frame_count("walk") >= 2
		and frames.get_frame_count("attack") >= 3
		and frames.get_frame_count("death") >= 2
	)

func _on_hit() -> void:
	state = State.HIT
	state_time_left = HIT_TIME
	sprite.play("hit")

func _on_death() -> void:
	state = State.DEAD
	sprite.play("death")

func _enter_state(next_state: State) -> void:
	if is_dead() and next_state != State.DEAD:
		return
	state = next_state
	match state:
		State.IDLE:
			_play_if_needed("idle")
		State.APPROACH:
			_play_if_needed("walk")
		State.DEAD:
			sprite.play("death")

func _face_target() -> void:
	sprite.flip_h = target != null and target.global_position.x < global_position.x

func _play_if_needed(animation_name: StringName) -> void:
	if sprite.animation != animation_name:
		sprite.play(animation_name)

func _build_frames() -> SpriteFrames:
	var frames := SpriteFrames.new()
	frames.remove_animation("default")
	_add_animation(frames, "idle", ["ranged_idle_0", "ranged_idle_1"], 3.0, true)
	_add_animation(frames, "walk", ["ranged_walk_0", "ranged_walk_1"], 6.0, true)
	_add_animation(frames, "attack", ["ranged_attack_0", "ranged_attack_1", "ranged_attack_2"], 4.2, false)
	_add_animation(frames, "hit", ["ranged_hit"], 1.0, false)
	_add_animation(frames, "death", ["ranged_death_0", "ranged_death_1"], 4.0, false)
	return frames

func _add_animation(frames: SpriteFrames, animation_name: StringName, frame_names: Array[String], fps: float, loop: bool) -> void:
	frames.add_animation(animation_name)
	frames.set_animation_speed(animation_name, fps)
	frames.set_animation_loop(animation_name, loop)
	for frame_name in frame_names:
		var texture := load("res://assets/%s.svg" % frame_name) as Texture2D
		frames.add_frame(animation_name, texture)

func _draw() -> void:
	_draw_shadow_ellipse(Vector2(0, 2), Vector2(18, 5), Color(0, 0, 0, 0.32))
	var ratio := float(hp) / float(max_hp)
	draw_rect(Rect2(-21, -47, 42, 5), Color("251b21"))
	draw_rect(Rect2(-20, -46, 40.0 * ratio, 3), Color("a46fea"))

func _draw_shadow_ellipse(center: Vector2, radius: Vector2, color: Color) -> void:
	var points := PackedVector2Array()
	for index in 18:
		var angle := TAU * float(index) / 18.0
		points.append(center + Vector2(cos(angle) * radius.x, sin(angle) * radius.y))
	draw_colored_polygon(points, color)
