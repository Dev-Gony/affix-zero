class_name E0PerfMetrics
extends RefCounted

static func summarize(samples_ms: Array[float]) -> Dictionary:
	if samples_ms.is_empty():
		return {
			"sample_count": 0,
			"avg_ms": 0.0,
			"avg_fps": 0.0,
			"p50_ms": 0.0,
			"p95_ms": 0.0,
			"p99_ms": 0.0,
			"max_ms": 0.0,
		}
	var sorted := samples_ms.duplicate()
	sorted.sort()
	var total := 0.0
	for value in sorted:
		total += value
	var average := total / float(sorted.size())
	return {
		"sample_count": sorted.size(),
		"avg_ms": average,
		"avg_fps": 1000.0 / average if average > 0.0 else 0.0,
		"p50_ms": _percentile(sorted, 0.50),
		"p95_ms": _percentile(sorted, 0.95),
		"p99_ms": _percentile(sorted, 0.99),
		"max_ms": sorted[sorted.size() - 1],
	}

static func _percentile(sorted_samples: Array[float], quantile: float) -> float:
	if sorted_samples.is_empty():
		return 0.0
	var index := ceili(float(sorted_samples.size() - 1) * clampf(quantile, 0.0, 1.0))
	return sorted_samples[index]
