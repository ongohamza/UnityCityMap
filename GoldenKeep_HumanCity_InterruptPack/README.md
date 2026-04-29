# Golden Keep - Human City Interrupt Pack v2

Purpose: pause the final Phase 3 cards and build the smallest first human city that already behaves like a living system.

This pack is gameplay integration only. It assumes the image-to-3D model pipeline exists elsewhere. Use 2D stand-ins or a tiny batch of finished model outputs while proving schedules, routes, alarms, loot, and escape.

## Contents

- `DATA/` - 50 citizens, 25 buildings, schedules, route graph, and layout reference.
- `UNITY_SCRIPTS/` - city clock, waypoint, schedule spawning, alarm, route choice, distraction, loot, escape, and raid objective scripts.
- `CARDS/` - solo-friendly implementation cards for Hamza.
- `ART_PIPELINE/` - finished model import notes and 2D cleanup support.
- `QA/` - test checklist for the first playable city.
- `AUDIT_NOTES.md` - data and design audit summary.

## Design Target

The prototype should feel like:

```text
living city
-> player chooses route nodes
-> risky choices raise alarm
-> citizens and guards react
-> direct rush becomes dangerous
-> planned sewer, rooftop, market, or gate timing routes become possible
-> steal loot
-> escape
```

Keep it small. Do not build a big city yet.
