# E0-C03 performance harness

Purpose: establish the first real Windows performance baseline before scaling toward survivor-style crowds.

Fixed fixture:
- Godot 4.7.2 Standard
- Compatibility renderer
- x1 time scale
- 640x360 game viewport, 1280x720 window override
- 1 warrior
- 40 enemies: 32 melee + 8 ranged
- real E0 projectile nodes
- enemy HP raised so entity count remains stable at 40
- drops intentionally remain 0 during the measurement fixture; death/drop correctness is covered by E0-C02

Windows benchmark defaults:
- warm-up 10 seconds
- measurement 60 seconds
- repetitions 3
- vsync disabled by the harness
- timing source Time.get_ticks_usec monotonic wall clock

Outputs:
- JSON summary
- CSV raw frame samples
- both written under the E0 custom user data directory in the perf folder

Proposed first target:
- p95 <= 16.7 ms
- p99 <= 33.3 ms

These are decision targets, not pre-claimed results.

CI/headless only validates the harness contract. CI performance numbers are not Windows GTX 1050 evidence.
