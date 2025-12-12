using Economy.Contract;
using K4_Guilds.Commands;
using K4_Guilds.Config;
using K4_Guilds.Database;
using K4_Guilds.Database.Models;
using K4_Guilds.Models;
using K4_Guilds.Services;
using K4Guilds.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.GameEventDefinitions;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Plugins;

namespace K4_Guilds;

[PluginMetadata(
	Id = "k4.guilds",
	Version = "1.0.1",
	Name = "K4 - Guilds",
	Author = "K4ryuu",
	Description = "Guild system with ranks, upgrades, and developer API"
)]
public sealed partial class Plugin(ISwiftlyCore core) : BasePlugin(core)
{
	public static new ISwiftlyCore Core { get; private set; } = null!;

	public static IOptionsMonitor<GuildConfig> GuildConfig { get; private set; } = null!;
	public static IOptionsMonitor<UpgradesConfig> UpgradesConfig { get; private set; } = null!;
	public static IOptionsMonitor<CommandsConfig> CommandsConfig { get; private set; } = null!;

	internal GuildConfig Guild => GuildConfig.CurrentValue;
	internal UpgradesConfig Upgrades => UpgradesConfig.CurrentValue;
	internal CommandsConfig Commands => CommandsConfig.CurrentValue;

	private DatabaseService _database = null!;
	private GuildService _guildService = null!;
	private UpgradeService _upgradeService = null!;
	private PerkService _perkService = null!;
	private InviteService _inviteService = null!;
	private GuildCommands _commands = null!;
	private CancellationTokenSource? _interestTimerCts;
	private CancellationTokenSource? _scoreboardTimerCts;

	public IEconomyAPIv1? EconomyAPI { get; private set; }

	public event EventHandler<GuildEventArgs>? GuildCreated;
	public event EventHandler<GuildEventArgs>? GuildDisbanded;
	public event EventHandler<GuildMemberEventArgs>? MemberJoined;
	public event EventHandler<GuildMemberEventArgs>? MemberLeft;
	public event EventHandler<GuildMemberEventArgs>? MemberKicked;
	public event EventHandler<GuildMemberEventArgs>? MemberPromoted;
	public event EventHandler<GuildMemberEventArgs>? MemberDemoted;
	public event EventHandler<GuildInviteEventArgs>? InviteSent;

	public override void Load(bool hotReload)
	{
		Core = base.Core;

		InitializeConfigs();
		InitializeDatabase();
		InitializeServices();
		RegisterEvents();
		_commands.RegisterCommands();
		StartInterestTimer();
		StartScoreboardRefreshTimer();
	}

	public override void Unload()
	{
		_interestTimerCts?.Cancel();
		_interestTimerCts = null;
		_scoreboardTimerCts?.Cancel();
		_scoreboardTimerCts = null;
	}

	public override void UseSharedInterface(IInterfaceManager interfaceManager)
	{
		if (!interfaceManager.HasSharedInterface("Economy.API.v1"))
		{
			Core.Logger.LogWarning("Economy API is not available.");
			return;
		}

		EconomyAPI = interfaceManager.GetSharedInterface<IEconomyAPIv1>("Economy.API.v1");
		EconomyAPI.EnsureWalletKind(Guild.WalletKind);
	}

	public override void ConfigureSharedInterface(IInterfaceManager interfaceManager)
	{
		const string apiVersion = "K4Guilds.Api.v1";
		var apiService = new GuildsApiService(this);
		interfaceManager.AddSharedInterface<IK4GuildsApi, GuildsApiService>(apiVersion, apiService);
	}

	private void InitializeConfigs()
	{
		GuildConfig = BuildConfigService<GuildConfig>("guild.json", "K4GuildsGuild");
		UpgradesConfig = BuildConfigService<UpgradesConfig>("upgrades.json", "K4GuildsUpgrades");
		CommandsConfig = BuildConfigService<CommandsConfig>("commands.json", "K4GuildsCommands");
	}

	private static IOptionsMonitor<T> BuildConfigService<T>(string fileName, string sectionName) where T : class, new()
	{
		Core.Configuration
			.InitializeJsonWithModel<T>(fileName, sectionName)
			.Configure(builder =>
			{
				builder.AddJsonFile(fileName, optional: false, reloadOnChange: true);
			});

		ServiceCollection services = new();
		services.AddSwiftly(Core)
			.AddOptions<T>()
			.BindConfiguration(fileName);

		var provider = services.BuildServiceProvider();
		return provider.GetRequiredService<IOptionsMonitor<T>>();
	}

	private void InitializeDatabase()
	{
		_database = new DatabaseService(Guild.DatabaseConnection);
		Task.Run(async () => await _database.InitializeAsync());
	}

	private void InitializeServices()
	{
		_guildService = new GuildService(this, _database);
		_inviteService = new InviteService();
		_guildService.SetInviteService(_inviteService);
		_upgradeService = new UpgradeService(this, _database, _guildService);
		_perkService = new PerkService(this, _database);
		_commands = new GuildCommands(this, _guildService, _upgradeService);
	}

	private void RegisterEvents()
	{
		Core.GameEvent.HookPost<EventPlayerConnectFull>(OnPlayerConnectFull);
		Core.GameEvent.HookPost<EventPlayerDisconnect>(OnPlayerDisconnect);
	}

	private HookResult OnPlayerConnectFull(EventPlayerConnectFull ev)
	{
		var player = ev.UserIdPlayer;
		if (!player.IsValid) return HookResult.Continue;

		Task.Run(async () =>
		{
			await UpdatePlayerScoreboardTag(player);
		});

		return HookResult.Continue;
	}

	private async Task UpdatePlayerScoreboardTag(IPlayer player)
	{
		if (!Guild.ShowTagOnScoreboard) return;

		var guild = await _guildService.GetPlayerGuildAsync(player.SteamID);
		var tag = guild?.Tag ?? string.Empty;

		Core.Scheduler.NextWorldUpdate(() =>
		{
			if (!player.IsValid) return;
			var controller = player.Controller;
			if (controller == null) return;

			controller.Clan = tag;
			controller.ClanUpdated();
		});
	}

	internal void RefreshPlayerScoreboardTag(ulong steamId)
	{
		foreach (var player in Core.PlayerManager.GetAllPlayers())
		{
			if (player.SteamID == steamId && player.IsValid)
			{
				Task.Run(async () => await UpdatePlayerScoreboardTag(player));
				return;
			}
		}
	}

	internal void RefreshGuildScoreboardTags(int guildId)
	{
		Task.Run(async () =>
		{
			var guild = await _guildService.GetGuildAsync(guildId);
			if (guild == null) return;

			var memberSteamIds = new HashSet<ulong>(guild.Members.Select(m => m.SteamId));
			var onlinePlayers = Core.PlayerManager.GetAllPlayers()
				.Where(p => p.IsValid && memberSteamIds.Contains(p.SteamID))
				.ToList();

			foreach (var player in onlinePlayers)
				await UpdatePlayerScoreboardTag(player);
		});
	}

	private void StartInterestTimer()
	{
		if (!Upgrades.BankInterest.Enabled) return;

		var intervalSeconds = Upgrades.BankInterest.IntervalMinutes * 60f;
		_interestTimerCts = Core.Scheduler.RepeatBySeconds(intervalSeconds, ProcessBankInterest);
	}

	private void StartScoreboardRefreshTimer()
	{
		if (!Guild.ShowTagOnScoreboard || Guild.ScoreboardRefreshIntervalSeconds <= 0) return;

		_scoreboardTimerCts = Core.Scheduler.RepeatBySeconds(Guild.ScoreboardRefreshIntervalSeconds, RefreshAllScoreboardTags);
	}

	private void RefreshAllScoreboardTags()
	{
		Task.Run(async () =>
		{
			foreach (var player in Core.PlayerManager.GetAllPlayers())
			{
				if (player.IsValid)
					await UpdatePlayerScoreboardTag(player);
			}
		});
	}

	private void ProcessBankInterest()
	{
		Task.Run(async () =>
		{
			var guilds = await _database.GetAllGuildsAsync(0, 1000);
			foreach (var guild in guilds)
			{
				var interestPercent = _upgradeService.CalculateBankInterestPercent(guild);
				if (interestPercent <= 0 || guild.BankBalance <= 0) continue;

				var interest = (long)(guild.BankBalance * interestPercent / 100);
				if (interest <= 0) continue;

				var maxCapacity = _guildService.CalculateMaxBankCapacity(guild);
				var newBalance = Math.Min(guild.BankBalance + interest, maxCapacity);

				await _database.UpdateBankBalanceAsync(guild.Id, newBalance);
				_guildService.InvalidateGuildCache(guild.Id);
			}
		});
	}

	private HookResult OnPlayerDisconnect(EventPlayerDisconnect ev)
	{
		var player = ev.UserIdPlayer;
		if (player.IsValid)
			_guildService.InvalidatePlayerCache(player.SteamID);

		return HookResult.Continue;
	}

	internal void OnGuildCreated(Guild guild)
	{
		GuildCreated?.Invoke(this, new GuildEventArgs { Guild = new GuildWrapper(guild, _guildService) });
	}

	internal void OnGuildDisbanded(Guild guild)
	{
		_inviteService.RemoveAllInvitesForGuild(guild.Id);
		GuildDisbanded?.Invoke(this, new GuildEventArgs { Guild = new GuildWrapper(guild, _guildService) });
	}

	internal void OnMemberJoined(Guild guild, GuildMember member, IPlayer? player)
	{
		RefreshPlayerScoreboardTag(member.SteamId);

		var rank = _guildService.GetRankByPriority(member.RankPriority);
		MemberJoined?.Invoke(this, new GuildMemberEventArgs
		{
			Guild = new GuildWrapper(guild, _guildService),
			Member = new MemberWrapper(member, rank),
			Player = player
		});
	}

	internal void OnMemberLeft(Guild guild, GuildMember member, IPlayer? player)
	{
		RefreshPlayerScoreboardTag(member.SteamId);

		var rank = _guildService.GetRankByPriority(member.RankPriority);
		MemberLeft?.Invoke(this, new GuildMemberEventArgs
		{
			Guild = new GuildWrapper(guild, _guildService),
			Member = new MemberWrapper(member, rank),
			Player = player
		});
	}

	internal void OnMemberKicked(Guild guild, GuildMember member, IPlayer? player)
	{
		RefreshPlayerScoreboardTag(member.SteamId);

		var rank = _guildService.GetRankByPriority(member.RankPriority);
		MemberKicked?.Invoke(this, new GuildMemberEventArgs
		{
			Guild = new GuildWrapper(guild, _guildService),
			Member = new MemberWrapper(member, rank),
			Player = player
		});
	}

	internal void OnMemberPromoted(Guild guild, GuildMember member, IPlayer? player)
	{
		var rank = _guildService.GetRankByPriority(member.RankPriority);
		MemberPromoted?.Invoke(this, new GuildMemberEventArgs
		{
			Guild = new GuildWrapper(guild, _guildService),
			Member = new MemberWrapper(member, rank),
			Player = player
		});
	}

	internal void OnMemberDemoted(Guild guild, GuildMember member, IPlayer? player)
	{
		var rank = _guildService.GetRankByPriority(member.RankPriority);
		MemberDemoted?.Invoke(this, new GuildMemberEventArgs
		{
			Guild = new GuildWrapper(guild, _guildService),
			Member = new MemberWrapper(member, rank),
			Player = player
		});
	}

	internal void OnInviteSent(Guild guild, ulong inviterSteamId, ulong inviteeSteamId)
	{
		InviteSent?.Invoke(this, new GuildInviteEventArgs
		{
			Guild = new GuildWrapper(guild, _guildService),
			InviterSteamId = inviterSteamId,
			InviteeSteamId = inviteeSteamId
		});
	}

	internal GuildService GetGuildService() => _guildService;
	internal UpgradeService GetUpgradeService() => _upgradeService;
	internal PerkService GetPerkService() => _perkService;
	internal DatabaseService GetDatabase() => _database;
}
