class_name V0AssetLoader
extends RefCounted

static func repo_asset_path(relative_path: String) -> String:
	return ProjectSettings.globalize_path("res://../../%s" % relative_path)

static func load_repo_texture(relative_path: String) -> Texture2D:
	var full_path := repo_asset_path(relative_path)
	var image := Image.load_from_file(full_path)
	if image == null or image.is_empty():
		push_error("V0 asset load failed: %s" % full_path)
		return null
	return ImageTexture.create_from_image(image)
