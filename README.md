<a name="readme-top"></a>

![GitHub tag (with filter)](https://img.shields.io/github/v/tag/K4ryuu/K4-Guilds-SwiftlyS2?style=for-the-badge&label=Version)
![GitHub Repo stars](https://img.shields.io/github/stars/K4ryuu/K4-Guilds-SwiftlyS2?style=for-the-badge)
![GitHub issues](https://img.shields.io/github/issues/K4ryuu/K4-Guilds-SwiftlyS2?style=for-the-badge)
![GitHub](https://img.shields.io/github/license/K4ryuu/K4-Guilds-SwiftlyS2?style=for-the-badge)
![GitHub all releases](https://img.shields.io/github/downloads/K4ryuu/K4-Guilds-SwiftlyS2/total?style=for-the-badge)
[![Discord](https://img.shields.io/badge/Discord-Join%20Server-5865F2?style=for-the-badge&logo=discord&logoColor=white)](https://dsc.gg/k4-fanbase)

<!-- PROJECT LOGO -->
<br />
<div align="center">
  <h1 align="center">KitsuneLab©</h1>
  <h3 align="center">K4-Guilds</h3>
  <a align="center">A comprehensive guild/clan system for Counter-Strike 2 servers. Create and manage guilds with ranks, permissions, shared bank, upgrades, and a fully extensible system via developer API - add custom perks, menus, and more through external plugins.</a>

  <p align="center">
    <br />
    <a href="https://github.com/K4ryuu/K4-Guilds-SwiftlyS2/releases/latest">Download</a>
    ·
    <a href="https://github.com/K4ryuu/K4-Guilds-SwiftlyS2/issues/new?assignees=K4ryuu&labels=bug&projects=&template=bug_report.md&title=%5BBUG%5D">Report Bug</a>
    ·
    <a href="https://github.com/K4ryuu/K4-Guilds-SwiftlyS2/issues/new?assignees=K4ryuu&labels=enhancement&projects=&template=feature_request.md&title=%5BREQ%5D">Request Feature</a>
  </p>
</div>

### Support My Work

I create free, open-source Counter-Strike 2 plugins for the community. If you'd like to support my work, consider becoming a sponsor!

#### 💖 GitHub Sponsors

Support this project through [GitHub Sponsors](https://github.com/sponsors/K4ryuu) with flexible options:

- **One-time** or **monthly** contributions
- **Custom amount** - choose what works for you
- **Multiple tiers available** - from basic benefits to priority support or private project access

Every contribution helps me dedicate more time to development, support, and creating new features. Thank you! 🙏

<p align="center">
  <a href="https://github.com/sponsors/K4ryuu">
    <img src="https://img.shields.io/badge/sponsor-30363D?style=for-the-badge&logo=GitHub-Sponsors&logoColor=#EA4AAA" alt="GitHub Sponsors" />
  </a>
</p>

⭐ **Or support me for free by starring this repository!**
---

## Features

### Core Guild System

- **Guild Creation** - Create guilds with custom name and tag
- **Member Management** - Invite, kick, promote, and demote members
- **Rank System** - Fully configurable ranks with granular permissions
- **Guild Chat** - Private communication channel for guild members

### Economy & Progression

- **Guild Bank** - Shared currency pool with deposit/withdraw functionality
- **Upgrade System** - Four upgradeable stats:
  - **Member Slots** - Increase max guild capacity
  - **Bank Capacity** - Expand maximum bank balance
  - **XP Boost** - Percentage bonus for guild members
  - **Bank Interest** - Passive income on bank balance
- **Economy Integration** - Works with Economy or any compatible economy plugin

### Extensible Perk System

- **Two Perk Types**:
  - **Purchasable** - One-time purchase, can be toggled on/off
  - **Upgradeable** - Multi-level perks with scaling costs and effects
- **Example Perks** (in `src-perks/`):
  - **K4-Guilds-Speed** - Movement speed boost per level
  - **K4-Guilds-Health** - Extra HP on spawn per level
- **Event Hooks** - Perks can react to: spawn, death, kill, round start/end
- **Developer API** - Create custom perks in external plugins (see `src-perks/` for working examples)

### Localization

- Full translation support with customizable messages
- All text, colors, and formatting configurable via `translations/`

<p align="right">(<a href="#readme-top">back to top</a>)</p>

---

## Dependencies

- [**SwiftlyS2**](https://github.com/swiftly-solution/swiftlys2): Server plugin framework for Counter-Strike 2
- **Database**: One of the following supported databases:
  - **MySQL / MariaDB** - Recommended for production
  - **PostgreSQL** - Full support
  - **SQLite** - Great for single-server setups
- [**Economy**](https://github.com/SwiftlyS2-Plugins/Economy): Required for guild economy functionality

<p align="right">(<a href="#readme-top">back to top</a>)</p>

---

## Installation

1. Install [SwiftlyS2](https://github.com/swiftly-solution/swiftlys2) on your server
2. Configure your database connection in SwiftlyS2's `database.jsonc`
3. [Download the latest release](https://github.com/K4ryuu/K4-Guilds-SwiftlyS2/releases/latest)
4. Extract to your server's `swiftlys2/plugins/` directory
5. Configure the plugin files (see Configuration section)
6. Restart your server - database tables will be created automatically

<p align="right">(<a href="#readme-top">back to top</a>)</p>

---

## Configuration

The plugin uses multiple configuration files for better organization:

### `guild.json` - Core Settings

| Option                             | Description                                      | Default       |
| ---------------------------------- | ------------------------------------------------ | ------------- |
| `DatabaseConnection`               | Database connection name (from database.jsonc)   | `"host"`      |
| `WalletKind`                       | Economy wallet type to use                       | `"credits"`   |
| `DefaultSlots`                     | Starting member slots for new guilds             | `5`           |
| `MaxSlots`                         | Maximum slots a guild can have (with upgrades)   | `20`          |
| `CreationCost`                     | Cost to create a guild                           | `5000`        |
| `MinNameLength`                    | Minimum guild name length                        | `3`           |
| `MaxNameLength`                    | Maximum guild name length                        | `32`          |
| `MaxTagLength`                     | Maximum tag length                               | `4`           |
| `ShowTagOnScoreboard`              | Show guild tag as clan tag on scoreboard         | `true`        |
| `ScoreboardRefreshIntervalSeconds` | Interval to refresh scoreboard tags (0=disabled) | `60`          |
| `GuildRanks`                       | Array of rank definitions                        | _(see below)_ |

### Rank Configuration

Each rank has:

- `Name` - Translation key for rank name (e.g., `"k4.rank.leader"`)
- `Priority` - Higher = more authority (Leader = 100, Member = 0)
- `Permissions` - Bitfield of allowed actions
- `IsDefault` - Whether new members get this rank

**Available Permissions:**
| Permission | Value | Description |
|------------|-------|-------------|
| `Chat` | 1 | Use guild chat |
| `Invite` | 2 | Invite new members |
| `Kick` | 4 | Kick members |
| `Promote` | 8 | Promote members |
| `Demote` | 16 | Demote members |
| `Withdraw` | 32 | Withdraw from bank |
| `Upgrade` | 64 | Purchase upgrades |
| `ManagePerks` | 128 | Buy/toggle perks |
| `All` | -1 | All permissions |

### `upgrades.json` - Upgrade Settings

Each upgrade type (`SlotUpgrade`, `BankCapacity`, `XPBoost`, `BankInterest`) has:

| Option           | Description                 |
| ---------------- | --------------------------- |
| `Enabled`        | Enable/disable this upgrade |
| `MaxLevel`       | Maximum upgrade level       |
| `BaseCost`       | Cost for first level        |
| `CostMultiplier` | Cost scaling per level      |

**Type-specific settings:**

| Upgrade Type   | Extra Setting      | Default | Description                   |
| -------------- | ------------------ | ------- | ----------------------------- |
| `SlotUpgrade`  | `SlotsPerLevel`    | `2`     | Member slots added per level  |
| `BankCapacity` | `BaseCapacity`     | `10000` | Starting bank capacity        |
| `BankCapacity` | `CapacityPerLevel` | `5000`  | Additional capacity per level |
| `XPBoost`      | `BoostPerLevel`    | `5`     | XP boost percentage per level |
| `BankInterest` | `InterestPerLevel` | `1.0`   | Interest percentage per level |
| `BankInterest` | `IntervalMinutes`  | `60`    | How often interest is applied |

### `commands.json` - Command Customization

Customize command names, aliases, and permissions for all guild commands.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

---

## Commands

### Main Commands

| Command                      | Description                 |
| ---------------------------- | --------------------------- |
| `!guild` / `!g`              | Show help / guild commands  |
| `!guild create <name> <tag>` | Create a new guild          |
| `!guild info`                | View guild information      |
| `!guild members`             | List guild members          |
| `!guild leave`               | Leave your guild            |
| `!guild disband`             | Disband guild (leader only) |

### Member Management

| Command                   | Description            |
| ------------------------- | ---------------------- |
| `!guild invite <player>`  | Invite a player        |
| `!guild accept`           | Accept pending invite  |
| `!guild decline`          | Decline pending invite |
| `!guild kick <player>`    | Kick a member          |
| `!guild promote <player>` | Promote a member       |
| `!guild demote <player>`  | Demote a member        |

### Economy

| Command                    | Description              |
| -------------------------- | ------------------------ |
| `!guild deposit <amount>`  | Deposit to guild bank    |
| `!guild withdraw <amount>` | Withdraw from guild bank |
| `!guild upgrade`           | View available upgrades  |
| `!guild upgrade <type>`    | Purchase an upgrade      |

### Other

| Command                    | Description                |
| -------------------------- | -------------------------- |
| `!gc <message>`            | Send guild chat message    |
| `!guild perks`             | View available perks       |
| `!guild perks buy <id>`    | Purchase/upgrade a perk    |
| `!guild perks toggle <id>` | Toggle a perk on/off       |
| `!guild rename <name>`     | Rename guild (leader only) |

<p align="right">(<a href="#readme-top">back to top</a>)</p>

---

## Developer API

K4-Guilds provides a comprehensive API for other plugins to interact with the guild system.

### Getting the API

```csharp
var guildsApi = plugin.GetApi<IK4GuildsApi>("k4.guilds");
```

### Guild Queries

```csharp
// Get player's guild
var guild = await guildsApi.GetPlayerGuildAsync(steamId);

// Check if player is in a guild
bool inGuild = await guildsApi.IsInGuildAsync(steamId);

// Check permissions
bool canKick = await guildsApi.HasPermissionAsync(steamId, GuildPermission.Kick);
```

### Bank Operations

```csharp
// Add currency to guild bank (from external source)
await guildsApi.AddToBankAsync(guildId, 1000, "Tournament prize");

// Remove currency from guild bank
await guildsApi.RemoveFromBankAsync(guildId, 500, "Event fee");
```

### Custom Perks

Create custom perks by implementing `IGuildPerk` in a separate plugin. See `src-perks/` for complete working examples:

- `src-perks/K4-Guilds-Speed/` - Speed boost perk plugin
- `src-perks/K4-Guilds-Health/` - Health boost perk plugin

Each perk is a standalone plugin that:

1. References `K4-GuildsApi.dll` from `src-plugin/resources/exports/`
2. Gets the API via `UseSharedInterface`
3. Registers its perk with `_guildsApi.RegisterPerk()`
4. Hooks game events and uses the API to check perk status

**Available perk event hooks:**

- `OnMemberSpawn(IPerkContext ctx)` - Called when guild member spawns
- `OnMemberDeath(IPerkContext ctx)` - Called when guild member dies
- `OnMemberKill(IPerkContext ctx, IPlayer victim)` - Called when guild member gets a kill
- `OnRoundStart(IPerkContext ctx)` - Called at round start
- `OnRoundEnd(IPerkContext ctx, int winnerTeam)` - Called at round end

### Events

```csharp
guildsApi.GuildCreated += (sender, args) => {
    Console.WriteLine($"Guild created: {args.Guild.Name}");
};

guildsApi.MemberJoined += (sender, args) => {
    Console.WriteLine($"{args.Member.PlayerName} joined {args.Guild.Name}");
};
```

### Available Events

- `GuildCreated` - New guild created
- `GuildDisbanded` - Guild dissolved
- `MemberJoined` - Player joined a guild
- `MemberLeft` - Player left a guild
- `MemberKicked` - Player was kicked
- `MemberPromoted` - Player was promoted
- `MemberDemoted` - Player was demoted
- `InviteSent` - Guild invite sent

<p align="right">(<a href="#readme-top">back to top</a>)</p>

---

## Database

The plugin uses automatic schema management with FluentMigrator. Tables are created automatically on first run.

### Supported Databases

| Database        | Status  | Notes                                      |
| --------------- | ------- | ------------------------------------------ |
| MySQL / MariaDB | ✅ Full | Recommended for multi-server setups        |
| PostgreSQL      | ✅ Full | Alternative for existing Postgres setups   |
| SQLite          | ✅ Full | Perfect for single-server, no setup needed |

### Database Tables

- `k4_guilds` - Guild records (name, tag, leader, bank balance)
- `k4_guild_members` - Guild membership and ranks
- `k4_guild_upgrades` - Guild upgrade levels
- `k4_guild_perks` - Guild perk states and levels

<p align="right">(<a href="#readme-top">back to top</a>)</p>

---

## Translations

All messages are fully customizable via the `translations/` folder. The plugin ships with English (`en.jsonc`) by default.

To add a new language:

1. Copy `en.jsonc` to your language code (e.g., `hu.jsonc`, `de.jsonc`)
2. Translate all values
3. The plugin will automatically use the player's preferred language

<p align="right">(<a href="#readme-top">back to top</a>)</p>

---

## License

Distributed under the GPL-3.0 License. See [`LICENSE.md`](LICENSE.md) for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>
