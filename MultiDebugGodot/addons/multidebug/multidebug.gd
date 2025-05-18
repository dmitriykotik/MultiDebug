@tool
extends EditorPlugin

const AUTOLOAD_NAME := "MDS"
const AUTOLOAD_PATH := "res://addons/multidebug/Scripts/Server.cs"
const AUTOLOAD_NAME2 := "MDC"
const AUTOLOAD_PATH2 := "res://addons/multidebug/Scripts/Client.cs"
var dock: Control

func _enter_tree() -> void:
	_add_autoloads()
	_add_dock()
	_add_settings()
	call_deferred("_init_mdc")  # <- Ключевой момент!

func _add_autoloads():
	var autoloads := ProjectSettings.get_setting("autoload", {})
	var changed := false
	if not autoloads.has(AUTOLOAD_NAME):
		add_autoload_singleton(AUTOLOAD_NAME, AUTOLOAD_PATH)
		changed = true
	if not autoloads.has(AUTOLOAD_NAME2):
		add_autoload_singleton(AUTOLOAD_NAME2, AUTOLOAD_PATH2)
		changed = true
	if changed:
		ProjectSettings.save()

func _add_dock():
	var scene = preload("Scenes/MDClientDock.tscn")
	dock = scene.instantiate()
	add_control_to_dock(DOCK_SLOT_RIGHT_UL, dock)
	dock.name = "MultiDebug Client"
	
	var ConnectButton = dock.get_node("ConnectButton")
	ConnectButton.pressed.connect(self.Connect)
	var DisconnectButton = dock.get_node("DisconnectButton")
	DisconnectButton.pressed.connect(self.Disconnect)
	var SendButton = dock.get_node("SendButton")
	SendButton.pressed.connect(self.Send)
	var InputLine = dock.get_node("CommandInput")
	InputLine.text_submitted.connect(self.Send)

func _init_mdc():
	await get_tree().process_frame  # Ждём 1 кадр
	var mdc = get_node_or_null("/root/MDC")
	if mdc:
		mdc.Init(dock)

func _exit_tree() -> void:
	if ProjectSettings.get_setting("autoload", {}).has(AUTOLOAD_NAME):
		remove_autoload_singleton(AUTOLOAD_NAME)
	if ProjectSettings.get_setting("autoload", {}).has(AUTOLOAD_NAME2):
		remove_autoload_singleton(AUTOLOAD_NAME2)
	ProjectSettings.save()
	remove_control_from_docks(dock)
	dock.free()

func _add_settings():
	var settings = {
		"multidebug/server/enable": {
			"type": TYPE_BOOL,
			"default": true
		},
		"multidebug/server/port": {
			"type": TYPE_INT,
			"default": 5000
		},
		"multidebug/server/debug_mode": {
			"type": TYPE_BOOL,
			"default": false
		},
		"multidebug/server/password": {
			"type": TYPE_STRING,
			"default": "admin"
		}
	}

	for key in settings:
		if not ProjectSettings.has_setting(key):
			var s = settings[key]
			ProjectSettings.set_setting(key, s.default)
			ProjectSettings.set_initial_value(key, s.default)
			ProjectSettings.add_property_info({
				"name": key,
				"type": s.type,
				"usage": PROPERTY_USAGE_DEFAULT
			})
	ProjectSettings.save()

func Connect():
	var mdc = get_node_or_null("/root/MDC")
	if mdc:
		mdc.Connect()

func Disconnect():
	var mdc = get_node_or_null("/root/MDC")
	if mdc:
		mdc.Disconnect()

func Send(_text := ""):
	var mdc = get_node_or_null("/root/MDC")
	if mdc:
		mdc.Send()
