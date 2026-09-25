class_name E0MeleeEnemy
extends Node2D

signal died
signal attack_landed(amount: int)

enum State { IDLE, APPROACH, WINDUP, ACTIVE, RECOVERY, HIT, DEAD }

@export var move_speed: float = 72.0
@export var attack_range: float = 52.0
@export var attack_damage: int = 12
@export var defense: float = 2.0
@export var max_hp: int = 100

@onready var sprite: AnimatedSprite2D = $Sprite

var hp: int = max_hp
var target: E0Warrior
var state: State = State.IDLE
var state_time_left: float = 0.0
var attack_damage_applied: bool = false
var total_attacks_resolved: int = 0
var damage_events_received: int = 0
var last_attack_instance_received: int = -1
var distance_travelled: float = 0.0

const WINDUP: float = 0.28
const ACTIVE: float = 0.08
const RECOVERY: float = 0.42
const HIT_TIME: float = 0.14

func _ready() -> void:
	hp = max_hp
	sprite.sprite_frames = _build_frames()
	sprite.play("idle")
	queue_redraw()

func set_target(next_target: E0Warrior) -> void:
	target = next_target
	if state == State.IDLE:
		_enter_state(State.APPROACH)

func _physics_process(delta: float) -> void:
	if state == State.DEAD:
		return
	if target == null or not is_instance_valid(target) or target.is_dead():
		if state != State.HIT:
			_enter_state(State.IDLE)
		return

	_face_target()
	match state:
		State.IDLE:
			_enter_state(State.APPROACH)
		State.APPROACH:
			var distance := global_position.distance_to(target.global_position)
			if distance > attack_range:
				var step := global_position.direction_to(target.global_position) * move_speed * delta
				global_position += step
				distance_travelled += step.length()
				_play_if_needed("walk")
			else:
				_begin_attack()
		State.WINDUP, State.ACTIVE, State.RECOVERY, State.HIT:
			state_time_left = maxf(0.0, state_time_left - delta)
			if state == State.ACTIVE and not attack_damage_applied:
				attack_damage_applied = true
				if global_position.distance_to(target.global_position) <= attack_range + 5.0:
					target.take_damage(attack_damage)
					total_attacks_resolved += 1
					attack_landed.emit(attack_damage)
			if is_zero_approx(state_time_left):
				_advance_timed_state()
	queue_redraw()

func _begin_attack() -> void:
	attack_damage_applied = false
	state = State.WINDUP
	state_time_left = WINDUP
	sprite.play("attack")

func _advance_timed_state() -> void:
	match state:
		State.WINDUP:
			state = State.ACTIVE
			state_time_left = ACTIVE
		State.ACTIVE:
			state = State.RECOVERY
			state_time_left = RECOVERY
		State.RECOVERY:
			_enter_state(State.APPROACH)
		State.HIT:
			_enter_state(State.APPROACH)

func take_damage(amount: int, attack_instance_id: int) -> void:
	if state == State.DEAD:
		return
	if attack_instance_id == last_attack_instance_received:
		return
	last_attack_instance_received = attack_instance_id
	damage_events_received += 1
	hp = maxi(0, hp - amount)
	if hp <= 0:
		state = State.DEAD
		sprite.play("death")
		died.emit()
	else:
		state = State.HIT
		state_time_left = HIT_TIME
		sprite.play("hit")
	queue_redraw()

func is_dead() -> bool:
	return state == State.DEAD

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

func _enter_state(next_state: State) -> void:
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
	_add_animation(frames, "idle", ["enemy_idle_0", "enemy_idle_1"], 3.5, true)
	_add_animation(frames, "walk", ["enemy_walk_0", "enemy_walk_1"], 6.5, true)
	_add_animation(frames, "attack", ["enemy_attack_0", "enemy_attack_1", "enemy_attack_2"], 5.0, false)
	_add_animation(frames, "hit", ["enemy_hit"], 1.0, false)
	_add_animation(frames, "death", ["enemy_death_0", "enemy_death_1"], 4.0, false)
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
	draw_rect(Rect2(-20, -46, 40.0 * ratio, 3), Color("e85d5d"))

func _draw_shadow_ellipse(center: Vector2, radius: Vector2, color: Color) -> void:
	var points := PackedVector2Array()
	for index in 18:
		var angle := TAU * float(index) / 18.0
		points.append(center + Vector2(cos(angle) * radius.x, sin(angle) * radius.y))
	draw_colored_polygon(points, color)
