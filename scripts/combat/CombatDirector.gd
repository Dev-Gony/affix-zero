extends BattleManager

# BattleManager retains progression and room traversal. This layer owns the
# windup -> release -> hit contract so VFX and damage cannot run independently.
var _pending_target: WeakRef
var _pending_class: String = ""
var _release_left: float = 0.0
var _pending_pet_target: WeakRef
var _pending_pet_id: String = ""
var _pet_release_left: float = 0.0


func _process(delta: float) -> void:
	_tick_attack_release(delta)
	super._process(delta)


func _perform_auto_attack() -> void:
	if _pending_target != null or player._motion_kind in ["skill", "attack"]:
		return
	var target: EnemyAI = _nearest_enemy()
	if target == null or player.global_position.distance_to(target.global_position) > _attack_range():
		return
	_pending_target = weakref(target)
	_pending_class = GameManager.selected_class
	_release_left = PlayerAvatar.ATTACK_WINDUP
	player.play_attack(target.global_position)
	_player_velocity = Vector2.ZERO
	player.set_move_direction(Vector2.ZERO)


func _tick_attack_release(delta: float) -> void:
	if _pending_target == null:
		return
	if _respawning or _traveling or GameManager.game_state != GameManager.GameState.RUNNING or _pending_class != GameManager.selected_class:
		_pending_target = null
		return
	_release_left -= delta
	if _release_left > 0.0:
		return
	var target := _pending_target.get_ref() as EnemyAI
	_pending_target = null
	if target == null or not is_instance_valid(target) or target._dead:
		return
	if player.global_position.distance_to(target.global_position) > _attack_range() + 8.0:
		return
	var attack_power: float = GameManager.atk * GameManager.skill_damage_multiplier() * (1.0 + PetManager.active_player_damage_bonus_percent() * 0.01)
	var crit_chance: float = GameManager.crit + PetManager.active_player_crit_bonus_percent()
	var result: Dictionary = DamageCalculator.calculate_damage(attack_power, target.defense, 0, GameManager.penetration, crit_chance, false)
	if _is_ranged_class():
		var projectile := PlayerBasicProjectile.new()
		projectiles_root.add_child(projectile)
		var muzzle: Vector2 = player.global_position + player._facing * 10.0 + Vector2(0, -7)
		projectile.setup(muzzle, target, result, _basic_projectile_color(), _basic_projectile_style())
		projectile.impacted.connect(_on_player_basic_projectile_impacted)
		effects.spawn_fragments(muzzle, _basic_projectile_color(), 3, 20.0)
	else:
		effects.show_player_basic_attack(GameManager.selected_class, player.global_position, target.global_position, bool(result.get("critical", false)))
		target.take_hit(result)
		_finish_player_attack_feedback(result)


func _perform_auto_skill() -> void:
	if _pending_target != null or player._motion_kind == "attack":
		_skill_time_left = 0.10
		return
	super._perform_auto_skill()


func _update_auto_hunt(delta: float) -> void:
	if _pending_target != null or player._motion_kind == "attack":
		_player_velocity = Vector2.ZERO
		player.set_move_direction(Vector2.ZERO)
		return
	super._update_auto_hunt(delta)


func _update_pet_combat(delta: float) -> void:
	var data: PetData = PetManager.active_pet_data()
	if data == null or not pet.visible:
		_pending_pet_target = null
		return
	if _pending_pet_target != null:
		_pet_release_left -= delta
		if _pet_release_left <= 0.0:
			var target := _pending_pet_target.get_ref() as EnemyAI
			_pending_pet_target = null
			if target != null and is_instance_valid(target) and not target._dead and _pending_pet_id == data.id:
				var damage: int = maxi(1, roundi(PetManager.active_attack_power(GameManager.atk) - target.defense * 0.18))
				var projectile := PlayerBasicProjectile.new()
				projectiles_root.add_child(projectile)
				projectile.setup(pet.global_position, target, {"damage": damage, "critical": false}, data.color, "arcane")
				projectile.speed = 180.0
				projectile.impacted.connect(_on_pet_projectile_impact)
	_pet_attack_time_left = maxf(0.0, _pet_attack_time_left - delta)
	_pet_support_time_left = maxf(0.0, _pet_support_time_left - delta)
	if _pet_attack_time_left <= 0.0 and _pending_pet_target == null:
		var target: EnemyAI = _nearest_enemy()
		if target != null and pet.global_position.distance_to(target.global_position) <= data.attack_range:
			_pet_attack_time_left = PetManager.active_attack_interval()
			_pending_pet_target = weakref(target)
			_pending_pet_id = data.id
			_pet_release_left = 0.12
			pet.play_attack(target.global_position)
	if _pet_support_time_left <= 0.0:
		_pet_support_time_left = PetManager.active_support_interval()
		var heal_percent: float = PetManager.active_support_heal_percent()
		if heal_percent > 0.0 and GameManager.hp < GameManager.max_hp:
			var amount: int = maxi(1, roundi(float(GameManager.max_hp) * heal_percent * 0.01))
			var restored: int = GameManager.heal(amount)
			pet.play_support()
			effects.show_pet_heal(player.global_position, restored, pet.pet_color())


func _on_pet_projectile_impact(target: EnemyAI, result: Dictionary, world_position: Vector2) -> void:
	if target == null or not is_instance_valid(target) or target._dead:
		return
	target.take_hit(result)
	effects.spawn_fragments(world_position, pet.pet_color(), 4, 28.0)


func _begin_room_travel(from_room: int, to_room: int) -> void:
	_pending_target = null
	_pending_pet_target = null
	super._begin_room_travel(from_room, to_room)


func _on_class_selected(class_id: String) -> void:
	_pending_target = null
	_pending_pet_target = null
	super._on_class_selected(class_id)


func _spawn_world_loot(world_position: Vector2) -> void:
	var item: Dictionary = LootManager.roll_drop()
	if not item.is_empty():
		_collect_world_loot(item, world_position)


func _collect_world_loot(item: Dictionary, world_position: Vector2) -> bool:
	# Ownership is committed before the presentation timer. Closing the scene or
	# clearing VFX during room travel must not destroy a rolled/accepted reward.
	var accepted: bool = LootManager.collect_item(item)
	effects.show_drop(world_position, item)
	if accepted:
		_animate_loot_pickup(item.duplicate(true), world_position)
	return accepted


func _animate_loot_pickup(item: Dictionary, world_position: Vector2) -> void:
	var delay: float = LOOT_PICKUP_DELAY + float(int(item.get("rarity_index", 0))) * 0.22
	await get_tree().create_timer(delay, false).timeout
	if is_inside_tree():
		effects.show_pickup(world_position, player.global_position, item)
