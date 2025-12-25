# Changelog

All notable changes to this project will be documented in this file.

## [v1.0.1] - ???

### Changed

- Refactored config handling to use `IOptionsMonitor<T>` with `CurrentValue` pattern (matching K4-WeaponPurchase pattern)
  - Changed `IOptions<T>.Value` to `IOptionsMonitor<T>.CurrentValue` for hot-reload support
  - Changed `AddOptionsWithValidateOnStart<T>()` to `AddOptions<T>()` for consistent pattern
  - Made config monitors static for better accessibility: `GuildConfig`, `UpgradesConfig`, `CommandsConfig`
  - Added convenience properties `Guild`, `Upgrades`, `Commands` that access `.CurrentValue`

### Technical Notes

- All database models use auto-increment integer primary keys - no `[DatabaseGenerated(DatabaseGeneratedOption.None)]` attribute needed
- GameRules handling not applicable (plugin uses JSON config files instead)
