extends RefCounted
class_name DamageCalculator


static func calculate_damage(
	attack: float,
	enemy_defense: float,
	skill_level: int = 0,
	penetration_percent: float = 0.0,
	critical_percent: float = 0.0,
	is_skill: bool = false
) -> Dictionary:
	var base_damage: float = attack - enemy_defense + randi_range(0, 3)
	var skill_bonus: float = base_damage * (1.0 + skill_level * 0.1) if is_skill else 0.0
	var penetration: float = enemy_defense * penetration_percent * 0.01
	var damage: float = maxf(1.0, base_damage + skill_bonus + penetration)
	var critical: bool = randf() * 100.0 < critical_percent
	if critical:
		damage *= 1.8
	return {
		"damage": maxi(1, roundi(damage)),
		"critical": critical,
	}
