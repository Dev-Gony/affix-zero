extends Node

enum GameState {
	MENU,
	RUNNING,
	PAUSED,
}

const AVAILABLE_SPEED_MULTIPLIERS: Array[float] = [1.0, 2.0, 5.0]

var hp: int = 100
var mp: int = 50
var atk: int = 10
var def: int = 5
var spd: float = 1.0
var crit: float = 0.05
var level: int = 1
var xp: int = 0
var gold: int = 0
var floor: int = 1

var game_state: GameState = GameState.MENU
var speed_multiplier: float = 1.0
