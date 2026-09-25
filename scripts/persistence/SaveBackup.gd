extends RefCounted
class_name SaveBackup

# Immutable raw-byte backup. This is intentionally independent of save migration.
const DEFAULT_DIRECTORY: String = "user://backups/uiux-v1"
const MAX_SOURCE_BYTES: int = 16 * 1024 * 1024


static func digest_bytes(bytes: PackedByteArray) -> String:
	var hashing := HashingContext.new()
	hashing.start(HashingContext.HASH_SHA256)
	hashing.update(bytes)
	return hashing.finish().hex_encode()


static func capture(source_path: String, directory: String = DEFAULT_DIRECTORY) -> Dictionary:
	if not FileAccess.file_exists(source_path):
		return {"ok": true, "error": OK, "status": "no_existing_save", "path": "", "sha256": ""}
	var source := FileAccess.open(source_path, FileAccess.READ)
	if source == null:
		return _failure(FileAccess.get_open_error(), "source_open_failed")
	var length: int = source.get_length()
	if length > MAX_SOURCE_BYTES:
		source.close()
		return _failure(ERR_FILE_CANT_READ, "source_exceeds_16_mib_guard")
	var bytes: PackedByteArray = source.get_buffer(length)
	source.close()
	if bytes.size() != length:
		return _failure(ERR_FILE_CANT_READ, "source_read_incomplete")
	var checksum: String = digest_bytes(bytes)
	var stamp: String = Time.get_datetime_string_from_system(true).replace("-", "").replace(":", "")
	var target: String = directory.path_join("%s_%s.json" % [stamp.substr(0, 8), checksum])
	var absolute_dir: String = ProjectSettings.globalize_path(directory)
	var mkdir_error: Error = DirAccess.make_dir_recursive_absolute(absolute_dir)
	if mkdir_error != OK:
		return _failure(mkdir_error, "backup_directory_failed")
	if FileAccess.file_exists(target):
		if not _matches(target, bytes):
			return _failure(ERR_FILE_CORRUPT, "existing_backup_mismatch")
		return {"ok": true, "error": OK, "status": "reused_verified", "path": target, "sha256": checksum}
	var temporary: String = target + ".partial-%d-%d" % [OS.get_process_id(), Time.get_ticks_usec()]
	var output := FileAccess.open(temporary, FileAccess.WRITE)
	if output == null:
		return _failure(FileAccess.get_open_error(), "backup_open_failed")
	output.store_buffer(bytes)
	output.flush()
	var write_error: Error = output.get_error()
	output.close()
	if write_error != OK or not _matches(temporary, bytes):
		_remove_temporary(temporary)
		return _failure(ERR_FILE_CANT_WRITE, "backup_verification_failed")
	# A concurrently-created different backup must not be overwritten.
	if FileAccess.file_exists(target):
		_remove_temporary(temporary)
		if _matches(target, bytes):
			return {"ok": true, "error": OK, "status": "reused_verified", "path": target, "sha256": checksum}
		return _failure(ERR_FILE_CORRUPT, "existing_backup_mismatch")
	var rename_error: Error = DirAccess.rename_absolute(ProjectSettings.globalize_path(temporary), ProjectSettings.globalize_path(target))
	if rename_error != OK:
		_remove_temporary(temporary)
		return _failure(rename_error, "backup_publish_failed")
	if not _matches(target, bytes):
		return _failure(ERR_FILE_CORRUPT, "published_backup_mismatch")
	return {"ok": true, "error": OK, "status": "created_verified", "path": target, "sha256": checksum}


static func _matches(path: String, expected: PackedByteArray) -> bool:
	var file := FileAccess.open(path, FileAccess.READ)
	if file == null:
		return false
	var matches: bool = file.get_length() == expected.size()
	if matches:
		matches = file.get_buffer(expected.size()) == expected
	file.close()
	return matches


static func _remove_temporary(path: String) -> void:
	if FileAccess.file_exists(path):
		DirAccess.remove_absolute(ProjectSettings.globalize_path(path))


static func _failure(error: Error, status: String) -> Dictionary:
	return {"ok": false, "error": int(error), "status": status, "path": "", "sha256": ""}
