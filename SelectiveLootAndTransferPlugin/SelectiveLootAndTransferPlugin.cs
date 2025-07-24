using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Steam.Data;
using ArchiSteamFarm.Steam.Interaction;
using ArchiSteamFarm.Storage;
using ArchiSteamFarm.Plugins.Interfaces;
using ArchiSteamFarm.Web.GitHub.Data;
using ArchiSteamFarm.Localization;
using System.Globalization;
using JetBrains.Annotations;
using ArchiSteamFarm.Web.GitHub;

namespace SelectiveLootAndTransferPlugin {
#pragma warning disable CA1863 // VS, please, fuck off with this 'CompositeFormat' bullshit;
#pragma warning disable CA1812 // ASF uses this class during runtime
	[UsedImplicitly]
	internal sealed class SelectiveLootAndTransferPlugin : IBotCommand2, IGitHubPluginUpdates {
		public string Name => nameof(SelectiveLootAndTransferPlugin);
		public Version Version => typeof(SelectiveLootAndTransferPlugin).Assembly.GetName().Version ?? throw new InvalidOperationException(nameof(Version));
		public string RepositoryName => "CatPoweredPlugins/SelectiveLootAndTransferPlugin";

		private static readonly char[] Separator = [','];

		public async Task<Uri?> GetTargetReleaseURL(Version asfVersion, string asfVariant, bool asfUpdate, bool stable, bool forced) {
			ArgumentNullException.ThrowIfNull(asfVersion);
			ArgumentException.ThrowIfNullOrEmpty(asfVariant);

			if (string.IsNullOrEmpty(RepositoryName)) {
				ASF.ArchiLogger.LogGenericError(string.Format(CultureInfo.CurrentCulture, Strings.WarningFailedWithError, nameof(RepositoryName)));

				return null;
			}

			ReleaseResponse? releaseResponse = await GitHubService.GetLatestRelease(RepositoryName, stable).ConfigureAwait(false);

			if (releaseResponse == null) {
				return null;
			}

			Version newVersion = new(releaseResponse.Tag);

			if (!((Version.Major == newVersion.Major) && (Version.Minor == newVersion.Minor) && (Version.Build == newVersion.Build)) && !(asfUpdate || forced)) {
				ASF.ArchiLogger.LogGenericInfo(string.Format(CultureInfo.CurrentCulture, "New {0} plugin version {1} is only compatible with latest ASF version", Name, newVersion));

				return null;
			}

			if ((Version >= newVersion) & !forced) {
				ASF.ArchiLogger.LogGenericInfo(string.Format(CultureInfo.CurrentCulture, Strings.PluginUpdateNotFound, Name, Version, newVersion));

				return null;
			}

			if (releaseResponse.Assets.Count == 0) {
				ASF.ArchiLogger.LogGenericWarning(string.Format(CultureInfo.CurrentCulture, Strings.PluginUpdateNoAssetFound, Name, Version, newVersion));

				return null;
			}

			ReleaseAsset? asset = await ((IGitHubPluginUpdates) this).GetTargetReleaseAsset(asfVersion, asfVariant, newVersion, releaseResponse.Assets).ConfigureAwait(false);

			if ((asset == null) || !releaseResponse.Assets.Contains(asset)) {
				ASF.ArchiLogger.LogGenericWarning(string.Format(CultureInfo.CurrentCulture, Strings.PluginUpdateNoAssetFound, Name, Version, newVersion));

				return null;
			}

			ASF.ArchiLogger.LogGenericInfo(string.Format(CultureInfo.CurrentCulture, Strings.PluginUpdateFound, Name, Version, newVersion));

			return asset.DownloadURL;
		}

		public async Task<string?> OnBotCommand(Bot bot, EAccess access, string message, string[] args, ulong steamID = 0) {
			if (access < EAccess.Master) {
				return null;
			}

			return args[0].ToUpperInvariant() switch {
				"COMMANDS" => "\n" + Name + "\n\nTRANSFER# [<Bots>] <Modes> <Bot>\nLOOT# [<Bots>] <Modes>\nTRANSFER+ [<Bots>] <Modes> <Appids> <Bot>\nLOOT+ [<Bots>] <Modes> <Appids>\nTRANSFERM [<Bots>] <Modes> <Bot>\nLOOTM [<Bots>] <Modes>\nTRANSFERM+ [<Bots>] <Modes> <Appids> <Bot>\nLOOTM+ [<Bots>] <Modes> <Appids>\n\n---------------------",
				"TRANSFER#" when args.Length > 3 => await ResponseTransfer(access, steamID, args[1], args[2], Utilities.GetArgsAsText(args, 3, ",")).ConfigureAwait(false),
				"TRANSFER#" when args.Length > 2 => await ResponseTransfer(bot, access, args[1], args[2]).ConfigureAwait(false),
				"LOOT#" when args.Length > 2 => await ResponseLoot(access, steamID, args[1], Utilities.GetArgsAsText(args, 2, ",")).ConfigureAwait(false),
				"LOOT#" => await ResponseLoot(bot, access, args[1]).ConfigureAwait(false),
				"TRANSFER+" when args.Length > 4 => await ResponseTransferApp(access, steamID, args[1], args[2], args[3], Utilities.GetArgsAsText(args, 4, ",")).ConfigureAwait(false),
				"TRANSFER+" when args.Length > 3 => await ResponseTransferApp(bot, access, args[1], args[2], args[3]).ConfigureAwait(false),
				"LOOT+" when args.Length > 3 => await ResponseLootApp(access, steamID, args[1], args[2], Utilities.GetArgsAsText(args, 3, ",")).ConfigureAwait(false),
				"LOOT+" when args.Length > 2 => await ResponseLootApp(bot, access, args[1], args[2]).ConfigureAwait(false),
				"TRANSFERM" when args.Length > 3 => await ResponseTransfer(access, steamID, args[1], args[2], Utilities.GetArgsAsText(args, 3, ","), false).ConfigureAwait(false),
				"TRANSFERM" when args.Length > 2 => await ResponseTransfer(bot, access, args[1], args[2], false).ConfigureAwait(false),
				"LOOTM" when args.Length > 2 => await ResponseLoot(access, steamID, args[1], Utilities.GetArgsAsText(args, 2, ","), false).ConfigureAwait(false),
				"LOOTM" => await ResponseLoot(bot, access, args[1], false).ConfigureAwait(false),
				"TRANSFERM+" when args.Length > 4 => await ResponseTransferApp(access, steamID, args[1], args[2], args[3], Utilities.GetArgsAsText(args, 4, ","), false).ConfigureAwait(false),
				"TRANSFERM+" when args.Length > 3 => await ResponseTransferApp(bot, access, args[1], args[2], args[3], false).ConfigureAwait(false),
				"LOOTM+" when args.Length > 3 => await ResponseLootApp(access, steamID, args[1], args[2], Utilities.GetArgsAsText(args, 3, ","), false).ConfigureAwait(false),
				"LOOTM+" when args.Length > 2 => await ResponseLootApp(bot, access, args[1], args[2], false).ConfigureAwait(false),
				_ => null,
			};
		}

		public Task OnLoaded() {
			ASF.ArchiLogger.LogGenericInfo("Selective Loot and Transfer Plugin by Rudokhvist, powered by ginger cats");

			return Task.CompletedTask;
		}

		private static async Task<string?> ResponseTransfer(Bot bot, EAccess access, string mode, string botNameTo, bool sendNotMarketable = true) {
			if (string.IsNullOrEmpty(botNameTo) || string.IsNullOrEmpty(mode)) {
				ASF.ArchiLogger.LogNullError(null, nameof(mode) + " || " + nameof(botNameTo));

				return null;
			}

			if (access < EAccess.Master) {
				return null;
			}

			if (!bot.IsConnectedAndLoggedOn) {
				return bot.Commands.FormatBotResponse(Strings.BotNotConnected);
			}

			Bot? targetBot = Bot.GetBot(botNameTo);

			if (targetBot == null) {
				return access >= EAccess.Owner ? bot.Commands.FormatBotResponse(string.Format(CultureInfo.CurrentCulture, Strings.BotNotFound, botNameTo)) : null;
			}

			if (!targetBot.IsConnectedAndLoggedOn) {
				return bot.Commands.FormatBotResponse(Strings.BotNotConnected);
			}

			if (targetBot.SteamID == bot.SteamID) {
				return bot.Commands.FormatBotResponse(Strings.BotSendingTradeToYourself);
			}

			string[] modes = mode.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

			if (modes.Length == 0) {
				return bot.Commands.FormatBotResponse(string.Format(CultureInfo.CurrentCulture, Strings.ErrorIsEmpty, nameof(modes)));
			}

			HashSet<EAssetType> transferTypes = [];

			foreach (string singleMode in modes) {
				switch (singleMode.ToUpper(CultureInfo.CurrentCulture)) {
					case "A":
					case "ALL":
						foreach (EAssetType type in (EAssetType[]) Enum.GetValues<EAssetType>()) {
							transferTypes.Add(type);
						}

						break;
					case "BG":
					case "BACKGROUND":
						transferTypes.Add(EAssetType.ProfileBackground);

						break;
					case "BO":
					case "BOOSTER":
						transferTypes.Add(EAssetType.BoosterPack);

						break;
					case "C":
					case "CARD":
						transferTypes.Add(EAssetType.TradingCard);

						break;
					case "E":
					case "EMOTICON":
						transferTypes.Add(EAssetType.Emoticon);

						break;
					case "F":
					case "FOIL":
						transferTypes.Add(EAssetType.FoilTradingCard);

						break;
					case "G":
					case "GEMS":
						transferTypes.Add(EAssetType.SteamGems);

						break;
					case "U":
					case "UNKNOWN":
						transferTypes.Add(EAssetType.Unknown);

						break;
					default:
						return bot.Commands.FormatBotResponse(string.Format(CultureInfo.CurrentCulture, Strings.ErrorIsInvalid, mode));
				}
			}

			(bool success, string message) = await bot.Actions.SendInventory(targetSteamID: targetBot.SteamID, filterFunction: item => transferTypes.Contains(item.Type) && (sendNotMarketable || item.Marketable)).ConfigureAwait(false);

			return bot.Commands.FormatBotResponse(success ? message : string.Format(CultureInfo.CurrentCulture, Strings.WarningFailedWithError, message));
		}

		private static async Task<string?> ResponseTransfer(EAccess access, ulong steamID, string botNames, string mode, string botNameTo, bool sendNotMarketable = true) {
			if (string.IsNullOrEmpty(botNames) || string.IsNullOrEmpty(mode) || string.IsNullOrEmpty(botNameTo)) {
				ASF.ArchiLogger.LogNullError(null, nameof(botNames) + " || " + nameof(mode) + " || " + nameof(botNameTo));

				return null;
			}

			HashSet<Bot>? bots = Bot.GetBots(botNames);

			if ((bots == null) || (bots.Count == 0)) {
				return access >= EAccess.Owner ? Commands.FormatStaticResponse(string.Format(CultureInfo.CurrentCulture, Strings.BotNotFound, botNames)) : null;
			}

			IEnumerable<Task<string?>> tasks = bots.Select(bot => ResponseTransfer(bot, Commands.GetProxyAccess(bot, access, steamID), mode, botNameTo, sendNotMarketable));
			List<string?> results;

			switch (ASF.GlobalConfig?.OptimizationMode) {
				case GlobalConfig.EOptimizationMode.MinMemoryUsage:
					results = new List<string?>(bots.Count);

					foreach (Task<string?> task in tasks) {
						results.Add(await task.ConfigureAwait(false));
					}

					break;
				default:
					results = [.. await Task.WhenAll(tasks).ConfigureAwait(false)];

					break;
			}

			List<string?> responses = [..results.Where(static result => !string.IsNullOrEmpty(result))];

			return responses.Count > 0 ? string.Join("", responses) : null;
		}

		private static async Task<string?> ResponseTransferApp(Bot bot, EAccess access, string mode, string appIDs, string botNameTo, bool sendNotMarketable = true) {
			if (string.IsNullOrEmpty(botNameTo) || string.IsNullOrEmpty(mode) || string.IsNullOrEmpty(appIDs)) {
				ASF.ArchiLogger.LogNullError(null, nameof(mode) + " || " + nameof(botNameTo) + " || " + nameof(appIDs));

				return null;
			}

			if (access < EAccess.Master) {
				return null;
			}

			if (!bot.IsConnectedAndLoggedOn) {
				return bot.Commands.FormatBotResponse(Strings.BotNotConnected);
			}

			Bot? targetBot = Bot.GetBot(botNameTo);

			if (targetBot == null) {
				return access >= EAccess.Owner ? bot.Commands.FormatBotResponse(string.Format(CultureInfo.CurrentCulture, Strings.BotNotFound, botNameTo)) : null;
			}

			if (!targetBot.IsConnectedAndLoggedOn) {
				return bot.Commands.FormatBotResponse(Strings.BotNotConnected);
			}

			if (targetBot.SteamID == bot.SteamID) {
				return bot.Commands.FormatBotResponse(Strings.BotSendingTradeToYourself);
			}

			string[] appIDTexts = appIDs.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

			if (appIDTexts.Length == 0) {
				return bot.Commands.FormatBotResponse(string.Format(CultureInfo.CurrentCulture, Strings.ErrorIsEmpty, nameof(appIDTexts)));
			}

			HashSet<uint> realAppIDs = [];

			foreach (string appIDText in appIDTexts) {
				if (!uint.TryParse(appIDText, out uint appID) || (appID == 0)) {
					return bot.Commands.FormatBotResponse(string.Format(CultureInfo.CurrentCulture, Strings.ErrorIsInvalid, nameof(appID)));
				}

				realAppIDs.Add(appID);
			}

			string[] modes = mode.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

			if (modes.Length == 0) {
				return bot.Commands.FormatBotResponse(string.Format(CultureInfo.CurrentCulture, Strings.ErrorIsEmpty, nameof(modes)));
			}

			HashSet<EAssetType> transferTypes = [];

			foreach (string singleMode in modes) {
				switch (singleMode.ToUpper(CultureInfo.CurrentCulture)) {
					case "A":
					case "ALL":
						foreach (EAssetType type in (EAssetType[]) Enum.GetValues<EAssetType>()) {
							transferTypes.Add(type);
						}

						break;
					case "BG":
					case "BACKGROUND":
						transferTypes.Add(EAssetType.ProfileBackground);

						break;
					case "BO":
					case "BOOSTER":
						transferTypes.Add(EAssetType.BoosterPack);

						break;
					case "C":
					case "CARD":
						transferTypes.Add(EAssetType.TradingCard);

						break;
					case "E":
					case "EMOTICON":
						transferTypes.Add(EAssetType.Emoticon);

						break;
					case "F":
					case "FOIL":
						transferTypes.Add(EAssetType.FoilTradingCard);

						break;
					case "G":
					case "GEMS":
						transferTypes.Add(EAssetType.SteamGems);

						break;
					case "U":
					case "UNKNOWN":
						transferTypes.Add(EAssetType.Unknown);

						break;
					default:
						return bot.Commands.FormatBotResponse(string.Format(CultureInfo.CurrentCulture, Strings.ErrorIsInvalid, mode));
				}
			}

			(bool success, string message) = await bot.Actions.SendInventory(targetSteamID: targetBot.SteamID, filterFunction: item => transferTypes.Contains(item.Type) && (sendNotMarketable || item.Marketable) && realAppIDs.Contains(item.RealAppID)).ConfigureAwait(false);

			return bot.Commands.FormatBotResponse(success ? message : string.Format(CultureInfo.CurrentCulture, Strings.WarningFailedWithError, message));
		}

		private static async Task<string?> ResponseTransferApp(EAccess access, ulong steamID, string botNames, string mode, string appIDs, string botNameTo, bool sendNotMarketable = true) {
			if (string.IsNullOrEmpty(botNames) || string.IsNullOrEmpty(mode) || string.IsNullOrEmpty(botNameTo)) {
				ASF.ArchiLogger.LogNullError(null, nameof(botNames) + " || " + nameof(mode) + " || " + nameof(botNameTo));

				return null;
			}

			HashSet<Bot>? bots = Bot.GetBots(botNames);

			if ((bots == null) || (bots.Count == 0)) {
				return access >= EAccess.Owner ? Commands.FormatStaticResponse(string.Format(CultureInfo.CurrentCulture, Strings.BotNotFound, botNames)) : null;
			}

			IEnumerable<Task<string?>> tasks = bots.Select(bot => ResponseTransferApp(bot, Commands.GetProxyAccess(bot, access, steamID), mode, appIDs, botNameTo, sendNotMarketable));
			List<string?> results;

			switch (ASF.GlobalConfig?.OptimizationMode) {
				case GlobalConfig.EOptimizationMode.MinMemoryUsage:
					results = new List<string?>(bots.Count);

					foreach (Task<string?> task in tasks) {
						results.Add(await task.ConfigureAwait(false));
					}

					break;
				default:
					results = [.. await Task.WhenAll(tasks).ConfigureAwait(false)];

					break;
			}

			List<string?> responses = [..results.Where(static result => !string.IsNullOrEmpty(result))];

			return responses.Count > 0 ? string.Join("", responses) : null;
		}

		private static async Task<string?> ResponseLoot(Bot bot, EAccess access, string mode, bool sendNotMarketable = true) {
			if (string.IsNullOrEmpty(mode)) {
				ASF.ArchiLogger.LogNullError(null, nameof(mode));

				return null;
			}

			if (access < EAccess.Master) {
				return null;
			}

			if (!bot.IsConnectedAndLoggedOn) {
				return bot.Commands.FormatBotResponse(Strings.BotNotConnected);
			}

			if (bot.BotConfig.LootableTypes.Count == 0) {
				return bot.Commands.FormatBotResponse(string.Format(CultureInfo.CurrentCulture, Strings.ErrorIsEmpty, nameof(Bot.BotConfig.LootableTypes)));
			}

			string[] modes = mode.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

			if (modes.Length == 0) {
				return bot.Commands.FormatBotResponse(string.Format(CultureInfo.CurrentCulture, Strings.ErrorIsEmpty, nameof(modes)));
			}

			HashSet<EAssetType> transferTypes = [];

			foreach (string singleMode in modes) {
				switch (singleMode.ToUpper(CultureInfo.CurrentCulture)) {
					case "A":
					case "ALL":
						foreach (EAssetType type in (EAssetType[]) Enum.GetValues<EAssetType>()) {
							transferTypes.Add(type);
						}

						break;
					case "BG":
					case "BACKGROUND":
						transferTypes.Add(EAssetType.ProfileBackground);

						break;
					case "BO":
					case "BOOSTER":
						transferTypes.Add(EAssetType.BoosterPack);

						break;
					case "C":
					case "CARD":
						transferTypes.Add(EAssetType.TradingCard);

						break;
					case "E":
					case "EMOTICON":
						transferTypes.Add(EAssetType.Emoticon);

						break;
					case "F":
					case "FOIL":
						transferTypes.Add(EAssetType.FoilTradingCard);

						break;
					case "G":
					case "GEMS":
						transferTypes.Add(EAssetType.SteamGems);

						break;
					case "U":
					case "UNKNOWN":
						transferTypes.Add(EAssetType.Unknown);

						break;
					default:
						return bot.Commands.FormatBotResponse(string.Format(CultureInfo.CurrentCulture, Strings.ErrorIsInvalid, mode));
				}
			}

			(bool success, string message) = await bot.Actions.SendInventory(filterFunction: item => transferTypes.Contains(item.Type) && (sendNotMarketable || item.Marketable)).ConfigureAwait(false);

			return bot.Commands.FormatBotResponse(success ? message : string.Format(CultureInfo.CurrentCulture, Strings.WarningFailedWithError, message));
		}

		private static async Task<string?> ResponseLoot(EAccess access, ulong steamID, string botNames, string mode, bool sendNotMarketable = true) {
			if (string.IsNullOrEmpty(botNames)) {
				ASF.ArchiLogger.LogNullError(null, nameof(botNames));

				return null;
			}

			HashSet<Bot>? bots = Bot.GetBots(botNames);

			if ((bots == null) || (bots.Count == 0)) {
				return access >= EAccess.Owner ? Commands.FormatStaticResponse(string.Format(CultureInfo.CurrentCulture, Strings.BotNotFound, botNames)) : null;
			}

			IList<string?> results = await Utilities.InParallel(bots.Select(bot => ResponseLoot(bot, Commands.GetProxyAccess(bot, access, steamID), mode, sendNotMarketable))).ConfigureAwait(false);

			List<string?> responses = [..results.Where(static result => !string.IsNullOrEmpty(result))];

			return responses.Count > 0 ? string.Join(Environment.NewLine, responses) : null;
		}

		private static async Task<string?> ResponseLootApp(Bot bot, EAccess access, string mode, string appIDs, bool sendNotMarketable = true) {
			if (string.IsNullOrEmpty(mode) || string.IsNullOrEmpty(appIDs)) {
				ASF.ArchiLogger.LogNullError(null, nameof(mode) + " || " + nameof(appIDs));

				return null;
			}

			if (access < EAccess.Master) {
				return null;
			}

			if (!bot.IsConnectedAndLoggedOn) {
				return bot.Commands.FormatBotResponse(Strings.BotNotConnected);
			}

			if (bot.BotConfig.LootableTypes.Count == 0) {
				return bot.Commands.FormatBotResponse(string.Format(CultureInfo.CurrentCulture, Strings.ErrorIsEmpty, nameof(Bot.BotConfig.LootableTypes)));
			}

			string[] appIDTexts = appIDs.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

			if (appIDTexts.Length == 0) {
				return bot.Commands.FormatBotResponse(string.Format(CultureInfo.CurrentCulture, Strings.ErrorIsEmpty, nameof(appIDTexts)));
			}

			HashSet<uint> realAppIDs = [];

			foreach (string appIDText in appIDTexts) {
				if (!uint.TryParse(appIDText, out uint appID) || (appID == 0)) {
					return bot.Commands.FormatBotResponse(string.Format(CultureInfo.CurrentCulture, Strings.ErrorIsInvalid, nameof(appID)));
				}

				realAppIDs.Add(appID);
			}

			string[] modes = mode.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

			if (modes.Length == 0) {
				return bot.Commands.FormatBotResponse(string.Format(CultureInfo.CurrentCulture, Strings.ErrorIsEmpty, nameof(modes)));
			}

			HashSet<EAssetType> transferTypes = [];

			foreach (string singleMode in modes) {
				switch (singleMode.ToUpper(CultureInfo.CurrentCulture)) {
					case "A":
					case "ALL":
						foreach (EAssetType type in (EAssetType[]) Enum.GetValues<EAssetType>()) {
							transferTypes.Add(type);
						}

						break;
					case "BG":
					case "BACKGROUND":
						transferTypes.Add(EAssetType.ProfileBackground);

						break;
					case "BO":
					case "BOOSTER":
						transferTypes.Add(EAssetType.BoosterPack);

						break;
					case "C":
					case "CARD":
						transferTypes.Add(EAssetType.TradingCard);

						break;
					case "E":
					case "EMOTICON":
						transferTypes.Add(EAssetType.Emoticon);

						break;
					case "F":
					case "FOIL":
						transferTypes.Add(EAssetType.FoilTradingCard);

						break;
					case "G":
					case "GEMS":
						transferTypes.Add(EAssetType.SteamGems);

						break;
					case "U":
					case "UNKNOWN":
						transferTypes.Add(EAssetType.Unknown);

						break;
					default:
						return bot.Commands.FormatBotResponse(string.Format(CultureInfo.CurrentCulture, Strings.ErrorIsInvalid, mode));
				}
			}

			(bool success, string message) = await bot.Actions.SendInventory(filterFunction: item => transferTypes.Contains(item.Type) && (sendNotMarketable || item.Marketable) && realAppIDs.Contains(item.RealAppID)).ConfigureAwait(false);

			return bot.Commands.FormatBotResponse(success ? message : string.Format(CultureInfo.CurrentCulture, Strings.WarningFailedWithError, message));
		}

		private static async Task<string?> ResponseLootApp(EAccess access, ulong steamID, string botNames, string mode, string appIDs, bool sendNotMarketable = true) {
			if (string.IsNullOrEmpty(botNames)) {
				ASF.ArchiLogger.LogNullError(null, nameof(botNames));

				return null;
			}

			HashSet<Bot>? bots = Bot.GetBots(botNames);

			if ((bots == null) || (bots.Count == 0)) {
				return access >= EAccess.Owner ? Commands.FormatStaticResponse(string.Format(CultureInfo.CurrentCulture, Strings.BotNotFound, botNames)) : null;
			}

			IList<string?> results = await Utilities.InParallel(bots.Select(bot => ResponseLootApp(bot, Commands.GetProxyAccess(bot, access, steamID), mode, appIDs, sendNotMarketable))).ConfigureAwait(false);

			List<string?> responses = [..results.Where(static result => !string.IsNullOrEmpty(result))];

			return responses.Count > 0 ? string.Join(Environment.NewLine, responses) : null;
		}
	}
}
#pragma warning restore CA1812 // ASF uses this class during runtime
#pragma warning restore CA1863 // Use 'CompositeFormat'
