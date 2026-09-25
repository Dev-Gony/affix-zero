# Game HUD — staged migration

The former fixed-center-player and permanent 640x400 design rules are archived. Authority: the approved v1.0 handoff pinned in docs/uiux-v1/spec_lock.json.

PR-A does NOT resize the viewport or rearrange combat UI. It adds a small build badge and F8 read-only diagnostics, without affecting combat or save data.

PR-C target: UI1280x720/world640x360. HUD left(16,16,304,72), run(384,16,336,64), tools(880,16,384,48), XP(16,96,1248,8), active(16,576,300,56), dock(352,656,576,48). Internal contents use containers, not independent absolute positions for every child.
Tools: AUTO, x1/x2/x5, settings. Save moves to Ctrl+S and ESC; exit stays in ESC. Boss floors show 0/1 and boss health, not the normal kill denominator. UI remains camera-independent.
Management uses the unified workspace defined in the handoff. Never shrink text to conceal an overflow. Inspect 1280x720/800,1366x768,1920x1080,2560x1440 and Windows DPI100/125/150 at PR-C.
