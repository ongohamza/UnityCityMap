# Plan Summary

Build a compact human city prototype where schedules, route choices, alarm pressure, loot, and escape form one playable loop.

The city is a network of nodes: citizens, buildings, route triggers, distractions, guard posts, bell/alarm points, storehouse loot, sewer paths, rooftop paths, and an escape tunnel. The player's route is a line through those nodes. Each risky choice can change alarm level, timing, and citizen behavior.

Example route:

```text
Cave exit
-> sewer culvert
-> back alley
-> observe market
-> trigger bakery distraction
-> slip past gatehouse
-> steal storehouse loot
-> escape through return tunnel
```

The first implementation should prove the game loop with simple 2D stand-ins. Finished townspeople model outputs can be imported later in a small test batch.
