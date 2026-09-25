class_name E0Main
extends Node2D

const WarriorScene = preload("res://scenes/warrior.tscn")
const EnemyScene = preload("res://scenes/melee_enemy.tscn")

var warrior: E0Warrior
var enemy: E0MeleeEnemy
var status_label: Label
var detail_label: Label
var victory_label: Label
var enemy_dead: bool = false
var player_damage_events: int = 0
var enemy_damage_events: int = 0

func _ready() -> void:
	warrior = WarriorScene.instantiate() as E0Warrior
	enemy = EnemyScene.instantiate() as E0MeleeEnemy
	warrior.name = "Warrior"
	enemy.name = "MeleeEnemy"
	add_child(warrior)
	add_child(enemy)
	warrior.position = Vector2(165, 205)
	enemy.position = Vector2(475, 205)
	warrior.set_target(enemy)
	enemy.set_target(warrior)
	warrior.damage_resolved.connect(_on_player_damage_resolved)
	enemy.attack_landed.connect(_on_enemy_attack_landed)
	enemy.died.connect(_on_enemy_died)
	warrior.died.connect(_on_warrior_died)
	_build_hud()
	queue_redraw()

func _process(_delta: float) -> void:
	if status_label == null:
		return
	status_label.text = "전사  HP %d/%d   [%s]      근접 적  HP %d/%d   [%s]" % [
		warrior.hp, warrior.max_hp, warrior.state_name(),
		enemy.hp, enemy.max_hp, enemy.state_name()
	]
	detail_label.text = "자동 접근 → WINDUP → 실제 타격 1회 → RECOVERY    |    R: 재시작"
	if Input.is_key_pressed(KEY_R) and (enemy_dead or warrior.is_dead()):
		get_tree().reload_current_scene()

func _draw() -> void:
	draw_rect(Rect2(0, 0, 640, 360), Color("11131a"))
	for x in range(32, 641, 32):
		draw_line(Vector2(x, 78), Vector2(x, 334), Color("242833"), 1.0)
	for y in range(78, 335, 32):
		draw_line(Vector2(0, y), Vector2(640, y), Color("242833"), 1.0)
	draw_rect(Rect2(24, 94, 592, 220), Color("6c7280"), false, 2.0)
	draw_string(ThemeDB.fallback_font, Vector2(24, 34), "AFFIX: ZERO  E0-C01", HORIZONTAL_ALIGNMENT_LEFT, -1, 22, Color("f2d083"))
	draw_string(ThemeDB.fallback_font, Vector2(24, 60), "Godot 4.7.2 / isolated combat core / no legacy save access", HORIZONTAL_ALIGNMENT_LEFT, -1, 14, Color("aeb8c7"))

func _build_hud() -> void:
	var canvas := CanvasLayer.new()
	canvas.name = "HUD"
	add_child(canvas)
	status_label = Label.new()
	status_label.position = Vector2(24, 318)
	status_label.add_theme_font_size_override("font_size", 13)
	canvas.add_child(status_label)
	detail_label = Label.new()
	detail_label.position = Vector2(24, 340)
	detail_label.add_theme_font_size_override("font_size", 11)
	detail_label.modulate = Color("aeb8c7")
	canvas.add_child(detail_label)
	victory_label = Label.new()
	victory_label.position = Vector2(220, 120)
	victory_label.add_theme_font_size_override("font_size", 22)
	victory_label.visible = false
	canvas.add_child(victory_label)

func _on_player_damage_resolved(_amount: int) -> void:
	player_damage_events += 1

func _on_enemy_attack_landed(_amount: int) -> void:
	enemy_damage_events += 1

func _on_enemy_died() -> void:
	enemy_dead = true
	victory_label.text = "E0-C01 PASS: 적 처치"
	victory_label.modulate = Color("82e69b")
	victory_label.visible = true

func _on_warrior_died() -> void:
	victory_label.text = "전사 사망 - R로 재시작"
	victory_label.modulate = Color("ff7b7b")
	victory_label.visible = true
