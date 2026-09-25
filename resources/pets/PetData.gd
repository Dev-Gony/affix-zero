extends Resource
class_name PetData

@export var id: String = ""
@export var display_name: String = ""
@export_range(0, 5, 1) var rarity_index: int = 0
@export var rarity_name: String = "일반"
@export var role: String = "공격형"
@export var element: String = "무속성"
@export var color: Color = Color.WHITE
@export var base_attack: float = 8.0
@export var attack_interval: float = 1.4
@export var attack_range: float = 150.0
@export var follow_distance: float = 22.0
@export var support_heal_percent: float = 0.0
@export var support_interval: float = 8.0
@export var player_damage_bonus_percent: float = 0.0
@export var player_damage_reduction_percent: float = 0.0
@export var player_crit_bonus_percent: float = 0.0
@export var player_xp_bonus_percent: float = 0.0
@export var player_gold_bonus_percent: float = 0.0
@export var unlock_floor: int = 1
@export var description: String = ""
