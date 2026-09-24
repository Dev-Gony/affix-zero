extends Resource
class_name EnemyData

@export var id: String = ""
@export var display_name: String = ""
@export var unlock_floor: int = 1
@export var base_hp: float = 20.0
@export var base_atk: float = 5.0
@export var base_def: float = 1.0
@export var move_speed: float = 25.0
@export var attack_cooldown: float = 1.0
@export_enum("chaser", "hopper", "zigzag", "skirmisher", "brute", "caster", "charger", "boss") var behavior: String = "chaser"
@export var attack_range: float = 0.0
@export var attack_windup: float = 0.22
@export var xp_reward: int = 8
@export var gold_reward: int = 4
@export var radius: float = 8.0
@export var color: Color = Color.WHITE
