# AFFIX: ZERO — Godot UI/UX v1.0 authority

The approved handoff is pinned in `docs/uiux-v1/spec_lock.json`. Read its MASTER and contracts, not the archived web landing-page template. CSS, GSAP, web CTA and mobile breakpoint rules are not Godot implementation requirements.

## Target, not current runtime

PR-A keeps the existing runtime unchanged. PR-C introduces a 1280x720 logical UI and a separate 640x360 pixel-world SubViewport. The world remains 1920x1200 in world units. Integer world scaling and correctly transformed pointer coordinates are required; no double stretching.

## Visual contract

Surfaces: #10141C, #19212D, #232E3E. Stroke #526176. Text #F2F4F8/#B8C2D1. Action #E9BC6B. Positive #73D5AD. Negative #FF9C9C. Focus #8FCBFF. Modal scrim alpha .65.
Rarity IDs: normal/magic/rare/unique/legend. Always show a grade name as well as color; selected/equipped/locked are independent states.
Target UI text: body18, secondary16, metadata14, headings24. 12 is allowed only for short card badges. Buttons44 high, compact hit area at least40x40. Gaps4/8/12/16/24/32. Korean-capable typography with recorded license; no arbitrary tiny fallback text.

## Interaction contract

One equipment workspace contains 7 equipped slots, 60-slot bag, and full replacement comparison. Existing 1/2 shortcuts focus gear/bag in that same workspace. Enter equips, L locks, Delete opens a sale confirmation. No irreversible action on selection.
View filter affects display only. Pickup policy affects future/unclaimed field drops only. Existing possessions are sold only through a separate confirmed action.
Management/settings default to full simulation pause. ESC closes only the topmost route. UI timing uses real time, not x5 game time.

## Completion

No new design is complete until its milestone's automated, rendered and user-play checks pass. Preserve currently approved sprites until new art is approved. See approved MASTER chapters7–14 and19–24 for exact geometry, state contracts, save rules and acceptance criteria.
