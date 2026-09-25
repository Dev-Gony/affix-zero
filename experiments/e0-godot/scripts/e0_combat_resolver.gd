class_name E0CombatResolver
extends RefCounted

static func damage(raw_damage: float, defense: float) -> int:
	return maxi(1, roundi(raw_damage - defense))

static func in_range(attacker_position: Vector2, target_position: Vector2, attack_range: float) -> bool:
	return attacker_position.distance_to(target_position) <= attack_range
