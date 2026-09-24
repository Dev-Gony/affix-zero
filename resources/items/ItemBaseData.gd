extends Resource
class_name ItemBaseData

@export var id: String = ""
@export var display_name: String = ""
@export_enum("weapon", "helmet", "armor", "gloves", "boots", "ring", "amulet") var slot: String = "weapon"
@export var tier: int = 1
@export var base_stats: Dictionary = {}
@export_range(0, 29, 1) var icon_index: int = 0
