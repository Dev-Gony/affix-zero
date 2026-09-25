extends RefCounted
class_name BuildInfo

const METADATA_PATH: String = "res://resources/build/build_info.json"


static func read_info() -> Dictionary:
	var info: Dictionary = {"version": "uiux-pr-a.1", "stage": "PR-A", "source_commit": "unidentified", "branch": "unknown", "built_at_utc": "unstamped", "dirty": false, "identity_source": "unstamped"}
	if FileAccess.file_exists(METADATA_PATH):
		var parsed: Variant = JSON.parse_string(FileAccess.get_file_as_string(METADATA_PATH))
		if parsed is Dictionary:
			info.merge(parsed as Dictionary, true)
			info["identity_source"] = "build_metadata"
	var project_dir: String = ProjectSettings.globalize_path("res://")
	var git_path: String = project_dir.path_join(".git")
	# Exported builds never execute git. Editor checkouts show the actual local HEAD.
	if OS.has_feature("editor") and not OS.has_feature("web") and (DirAccess.dir_exists_absolute(git_path) or FileAccess.file_exists(git_path)):
		var output: Array = []
		var result: int = OS.execute("git", PackedStringArray(["-C", project_dir, "rev-parse", "HEAD"]), output, false)
		if result == 0 and not output.is_empty():
			var sha: String = String(output[0]).strip_edges()
			if sha.length() == 40:
				info["source_commit"] = sha
				info["identity_source"] = "local_git"
		output.clear()
		result = OS.execute("git", PackedStringArray(["-C", project_dir, "branch", "--show-current"]), output, false)
		if result == 0 and not output.is_empty() and not String(output[0]).strip_edges().is_empty():
			info["branch"] = String(output[0]).strip_edges()
		output.clear()
		result = OS.execute("git", PackedStringArray(["-C", project_dir, "status", "--porcelain", "--untracked-files=no"]), output, false)
		if result == 0:
			info["dirty"] = not output.is_empty() and not String(output[0]).strip_edges().is_empty()
	return info
