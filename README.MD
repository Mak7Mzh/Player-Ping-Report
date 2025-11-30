# Player Ping Report

Adds a simple `/ping` command so players can report suspected server lag.

Config (example)

```json
{
  "Cooldown": 60,
  "Discord WebHook": ""
}
```

- `Cooldown`: seconds between `/ping` uses per player (default 60).
- `Discord WebHook`: Discord webhook URL to forward reports.