class_name E0Main
extends Node2D

const WarriorScene = preload("res://scenes/warrior.tscn")
const MeleeScene = preload("res://scenes/melee_enemy.tscn")
const RangedScene = preload("res://scenes/ranged_enemy.tscn")
const ProjectileScene = preload("res://scenes/enemy_projectile.tscn")
const LootDropScene = preload("res://scenes/loot_drop.tscn")

var warrior: E0Warrior
var melee_enemy: E0MeleeEnemy
var ranged_enemy: E0RangedEnemy
var enemies: Array[E0EnemyBase] = []
var drops: Array[E0LootDrop] = []
var reward_ledger := E0RewardLedger.new()

var status_label: Label
var detail_label: Label
var reward_label: Label
var victory_label: Label

var death_events: int = 0
var drop_spawn_count: int = 0
var drop_collect_count: int = 0
var projectile_spawn_count: int = 0
var projectile_hit_count: int = 0
var projectile_expire_count: int = 0
var projectile_min_travel_distance: float = INF
var ledger_total_at_first_drop: int = -1
var combat_complete: bool = false

func _ready() -> void:
	warrior = WarriorScene.instantiate() as E0Warrior
	melee_enemy = MeleeScene.instantiate() as E0MeleeEnemy
	ranged_enemy = RangedScene.instantiate() as E0RangedEnemy

	warrior.name = "Warrior"
	melee_enemy.name = "MeleeEnemy"
	ranged_enemy.name = "RangedEnemy"

	add_child(warrior)
	add_child(melee_enemy)
	add_child(ranged_enemy)

	warrior.position = Vector2(140, 215)
	melee_enemy.position = Vector2(365, 215)
	ranged_enemy.position = Vector2(520, 155)

	enemies = [melee_enemy, ranged_enemy]
	melee_enemy.set_target(warrior)
	ranged_enemy.set_target(warrior)
	warrior.set_target(melee_enemy)

	melee_enemy.died.connect(_on_enemy_died)
	ranged_enemy.died.connect(_on_enemy_died)
	ranged_enemy.projectile_requested.connect(_on_projectile_requested)
	warrior.died.connect(_on_warrior_died)

	_build_hud()
	queue_redraw()

func _process(_delta: float) -> void:
	if status_label == null:
		return

	if not warrior.is_dead():
		if not warrior.has_live_target():
			var next_enemy := _nearest_live_enemy()
			if next_enemy != null:
				warrior.set_target(next_enemy)
			elif not warrior.has_pickup_target():
				var next_drop := _nearest_live_drop()
				if next_drop != null:
					warrior.set_pickup_target(next_drop)

		if _all_enemies_dead() and drop_spawn_count > 0 and drop_collect_count == drop_spawn_count and not combat_complete:
			combat_complete = true
			victory_label.text = "E0-C02 PASS: 투사체 + 드랍 회수"
			victory_label.modulate = Color("82e69b")
			victory_label.visible = true

	status_label.text = "전사 %d/%d [%s]   근접 %d/%d [%s]   원거리 %d/%d [%s]" % [
		warrior.hp, warrior.max_hp, warrior.state_name(),
		melee_enemy.hp, melee_enemy.max_hp, melee_enemy.state_name(),
		ranged_enemy.hp, ranged_enemy.max_hp, ranged_enemy.state_name()
	]
	reward_label.text = "RewardLedger  GOLD %d   XP %d   |   드랍 %d/%d   |   투사체 HIT %d/%d" % [
		reward_ledger.gold, reward_ledger.xp, drop_collect_count, drop_spawn_count, projectile_hit_count, projectile_spawn_count
	]
	detail_label.text = "근접 타격 + 원거리 실투사체 + 적 사망 드랍 + 전사 자동 회수    |    R: 재시작"

	if Input.is_key_pressed(KEY_R) and (combat_complete or warrior.is_dead()):
		get_tree().reload_current_scene()

func _nearest_live_enemy() -> E0EnemyBase:
	var best: E0EnemyBase
	var best_distance := INF
	for enemy in enemies:
		if enemy == null or not is_instance_valid(enemy) or enemy.is_dead():
			continue
		var distance := warrior.global_position.distance_to(enemy.global_position)
		if distance < best_distance:
			best_distance = distance
			best = enemy
	return best

func _nearest_live_drop() -> E0LootDrop:
	var best: E0LootDrop
	var best_distance := INF
	for drop in drops:
		if drop == null or not is_instance_valid(drop) or drop.is_collected():
			continue
		var distance := warrior.global_position.distance_to(drop.global_position)
		if distance < best_distance:
			best_distance = distance
			best = drop
	return best

func _all_enemies_dead() -> bool:
	for enemy in enemies:
		if enemy != null and is_instance_valid(enemy) and not enemy.is_dead():
			return false
	return true

func _on_enemy_died(enemy: E0EnemyBase) -> void:
	death_events += 1
	_spawn_drop(enemy)
	if warrior.target == enemy:
		warrior.target = null

func _spawn_drop(enemy: E0EnemyBase) -> void:
	var drop := LootDropScene.instantiate() as E0LootDrop
	var id := StringName("drop_%s" % String(enemy.enemy_id))
	drop.name = "Loot_%s" % String(enemy.enemy_id)
	add_child(drop)
	drop.setup(id, enemy.global_position + Vector2(0, 16), enemy.reward_gold, enemy.reward_xp, reward_ledger, warrior)
	drop.collected.connect(_on_drop_collected)
	drops.append(drop)
	drop_spawn_count += 1
	if ledger_total_at_first_drop < 0:
		ledger_total_at_first_drop = reward_ledger.total_collections

func _on_drop_collected(_drop: E0LootDrop, _gold: int, _xp: int) -> void:
	drop_collect_count += 1

func _on_projectile_requested(_enemy: E0RangedEnemy, origin: Vector2, direction: Vector2, damage: int, projectile_id: int) -> void:
	var projectile := ProjectileScene.instantiate() as E0EnemyProjectile
	projectile.name = "Projectile_%d" % projectile_id
	add_child(projectile)
	projectile.setup(projectile_id, origin, direction, warrior, damage)
	projectile.impacted.connect(_on_projectile_impacted)
	projectile.expired.connect(_on_projectile_expired)
	projectile_spawn_count += 1

func _on_projectile_impacted(_projectile: E0EnemyProjectile, travel_distance: float) -> void:
	projectile_hit_count += 1
	projectile_min_travel_distance = minf(projectile_min_travel_distance, travel_distance)

func _on_projectile_expired(_projectile: E0EnemyProjectile) -> void:
	projectile_expire_count += 1

func _on_warrior_died() -> void:
	victory_label.text = "전사 사망 - R로 재시작"
	victory_label.modulate = Color("ff7b7b")
	victory_label.visible = true

func _draw() -> void:
	draw_rect(Rect2(0, 0, 640, 360), Color("11131a"))
	for x in range(32, 641, 32):
		draw_line(Vector2(x, 78), Vector2(x, 310), Color("242833"), 1.0)
	for y in range(78, 311, 32):
		draw_line(Vector2(0, y), Vector2(640, y), Color("242833"), 1.0)
	draw_rect(Rect2(24, 94, 592, 198), Color("6c7280"), false, 2.0)
	draw_string(ThemeDB.fallback_font, Vector2(24, 34), "AFFIX: ZERO  E0-C02", HORIZONTAL_ALIGNMENT_LEFT, -1, 22, Color("f2d083"))
	draw_string(ThemeDB.fallback_font, Vector2(24, 60), "Godot 4.7.2 / projectile travel / reward ledger / isolated save", HORIZONTAL_ALIGNMENT_LEFT, -1, 14, Color("aeb8c7"))

func _build_hud() -> void:
	var canvas := CanvasLayer.new()
	canvas.name = "HUD"
	add_child(canvas)

	status_label = Label.new()
	status_label.position = Vector2(24, 298)
	status_label.add_theme_font_size_override("font_size", 12)
	canvas.add_child(status_label)

	reward_label = Label.new()
	reward_label.position = Vector2(24, 319)
	reward_label.add_theme_font_size_override("font_size", 12)
	reward_label.modulate = Color("f1ca5d")
	canvas.add_child(reward_label)

	detail_label = Label.new()
	detail_label.position = Vector2(24, 341)
	detail_label.add_theme_font_size_override("font_size", 10)
	detail_label.modulate = Color("aeb8c7")
	canvas.add_child(detail_label)

	victory_label = Label.new()
	victory_label.position = Vector2(170, 112)
	victory_label.add_theme_font_size_override("font_size", 20)
	victory_label.visible = false
	canvas.add_child(victory_label)
