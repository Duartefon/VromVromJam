extends VehicleBody3D

@onready var cam_arm: SpringArm3D = $CamArm
@onready var wheel_back_left: MeshInstance3D = $"truck/truck/wheel-back-left"
@onready var wheel_back_right: MeshInstance3D = $"truck/truck/wheel-back-right"

var max_RPM = 450
var max_toruqe = 300
var turn_speed = 3 
var turn_amunt = 0.3


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	cam_arm.position = position
	
	var dir = Input.get_action_strength("gas") - Input.get_action_strength("brake")
	var steering_dir = Input.get_action_Strength("left") - Input.get_action_Strength("right")
	
	
	var left_RPM = abs()
