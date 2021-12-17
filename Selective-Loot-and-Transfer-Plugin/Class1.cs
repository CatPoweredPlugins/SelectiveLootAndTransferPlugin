using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Steam.Data;
using ArchiSteamFarm.Steam.Interaction;
using ArchiSteamFarm.Steam.Storage;
using ArchiSteamFarm.Storage;
using ArchiSteamFarm.Plugins.Interfaces;
using JetBrains.Annotations;


namespace Selective_Loot_and_Transfer_Plugin {
	[Export(typeof(IPlugin))]
	public class Class1 : IBotCommand {
		public string Name => "Selective Loot and Transfer Plugin";
		public Version Version => typeof(Class1).Assembly.GetName().Version ?? new Version("0");
		async Task<string?> IBotCommand.OnBotCommand(Bot bot, ulong steamID, string message, string[] args) {
			if (!bot.HasAccess(steamID, BotConfig.EAccess.Master)) {
				return null;
			}

			return args[0].ToUpperInvariant() switch {
				"TRANSFER#" when args.Length > 3 => await ResponseTransfer(steamID, args[1], args[2], Utilities.GetArgsAsText(args, 3, ",")).ConfigureAwait(false),
				"TRANSFER#" when args.Length > 2 => await ResponseTransfer(bot, steamID, args[1], args[2]).ConfigureAwait(false),
				"LOOT#" when args.Length > 2 => await ResponseLoot(steamID, args[1], Utilities.GetArgsAsText(args, 2, ",")).ConfigureAwait(false),
				"LOOT#" => await ResponseLoot(bot, steamID, args[1]).ConfigureAwait(false),
				"TRANSFERM" when args.Length > 3 => await ResponseTransfer(steamID, args[1], args[2], Utilities.GetArgsAsText(args, 3, ","), false).ConfigureAwait(false),
				"TRANSFERM" when args.Length > 2 => await ResponseTransfer(bot, steamID, args[1], args[2], false).ConfigureAwait(false),
				"LOOTM" when args.Length > 2 => await ResponseLoot(steamID, args[1], Utilities.GetArgsAsText(args, 2, ","), false).ConfigureAwait(false),
				"LOOTM" => await ResponseLoot(bot, steamID, args[1], false).ConfigureAwait(false),
				_ => null,
			};
		}

		public Task OnLoaded() {
			ASF.ArchiLogger.LogGenericInfo("Selective Loot and Transfer Plugin by Ryzhehvost, powered by ginger cats");
			return Task.CompletedTask;
		}

		private static async Task<string?> ResponseTransfer(Bot bot, ulong steamID, string mode, string botNameTo, bool sendNotMarketable = true) {
			if ((steamID == 0) || string.IsNullOrEmpty(botNameTo) || string.IsNullOrEmpty(mode)) {
				ASF.ArchiLogger.LogNullError(nameof(steamID) + " || " + nameof(mode) + " || " + nameof(botNameTo));
				return null;
			}

			if (!bot.HasAccess(steamID, BotConfig.EAccess.Master)) {
				return null;
			}

			if (!bot.IsConnectedAndLoggedOn) {
				return bot.Commands.FormatBotResponse(ArchiSteamFarm.Localization.Strings.BotNotConnected);
			}

			Bot? targetBot = Bot.GetBot(botNameTo);
			if (targetBot == null) {
				return ASF.IsOwner(steamID) ? bot.Commands.FormatBotResponse(string.Format(ArchiSteamFarm.Localization.Strings.BotNotFound, botNameTo)) : null;
			}

			if (!targetBot.IsConnectedAndLoggedOn) {
				return bot.Commands.FormatBotResponse(ArchiSteamFarm.Localization.Strings.BotNotConnected);
			}

			if (targetBot.SteamID == bot.SteamID) {
				return bot.Commands.FormatBotResponse(ArchiSteamFarm.Localization.Strings.BotSendingTradeToYourself);
			}

			string[] modes = mode.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

			if (modes.Length == 0) {
				return bot.Commands.FormatBotResponse(string.Format(ArchiSteamFarm.Localization.Strings.ErrorIsEmpty, nameof(modes)));
			}

			HashSet<Asset.EType> transferTypes = new();

			foreach (string singleMode in modes) {
				switch (singleMode.ToUpper()) {
					case "A":
					case "ALL":
						foreach (Asset.EType type in (Asset.EType[]) Enum.GetValues(typeof(Asset.EType))) {
							transferTypes.Add(type);
						}

						break;
					case "BG":
					case "BACKGROUND":
						transferTypes.Add(Asset.EType.ProfileBackground);
						break;
					case "BO":
					case "BOOSTER":
						transferTypes.Add(Asset.EType.BoosterPack);
						break;
					case "C":
					case "CARD":
						transferTypes.Add(Asset.EType.TradingCard);
						break;
					case "E":
					case "EMOTICON":
						transferTypes.Add(Asset.EType.Emoticon);
						break;
					case "F":
					case "FOIL":
						transferTypes.Add(Asset.EType.FoilTradingCard);
						break;
					case "G":
					case "GEMS":
						transferTypes.Add(Asset.EType.SteamGems);
						break;
					case "U":
					case "UNKNOWN":
						transferTypes.Add(Asset.EType.Unknown);
						break;
					default:
						return bot.Commands.FormatBotResponse(string.Format(ArchiSteamFarm.Localization.Strings.ErrorIsInvalid, mode));
				}
			}

			(bool success, string message) = await bot.Actions.SendInventory(targetSteamID: targetBot.SteamID, filterFunction: item => transferTypes.Contains(item.Type)&&(sendNotMarketable||item.Marketable)).ConfigureAwait(false);

			return bot.Commands.FormatBotResponse(success ? message : string.Format(ArchiSteamFarm.Localization.Strings.WarningFailedWithError, message));
		}

		private static async Task<string?> ResponseTransfer(ulong steamID, string botNames, string mode, string botNameTo, bool sendNotMarketable = true) {
			if ((steamID == 0) || string.IsNullOrEmpty(botNames) || string.IsNullOrEmpty(mode) || string.IsNullOrEmpty(botNameTo)) {
				ASF.ArchiLogger.LogNullError(nameof(steamID) + " || " + nameof(botNames) + " || " + nameof(mode) + " || " + nameof(botNameTo));
				return null;
			}

			HashSet<Bot>? bots = Bot.GetBots(botNames);
			if ((bots == null) || (bots.Count == 0)) {
				return ASF.IsOwner(steamID) ? Commands.FormatStaticResponse(string.Format(ArchiSteamFarm.Localization.Strings.BotNotFound, botNames)) : null;
			}

			IEnumerable<Task<string?>> tasks = bots.Select(curbot => ResponseTransfer(curbot, steamID, mode, botNameTo,sendNotMarketable));
			ICollection<string?> results;

			switch (ASF.GlobalConfig?.OptimizationMode) {
				case GlobalConfig.EOptimizationMode.MinMemoryUsage:
					results = new List<string?>(bots.Count);
					foreach (Task<string?> task in tasks) {
						results.Add(await task.ConfigureAwait(false));
					}

					break;
				default:
					results = await Task.WhenAll(tasks).ConfigureAwait(false);
					break;
			}

			List<string?> responses = new(results.Where(result => !string.IsNullOrEmpty(result)));
			return responses.Count > 0 ? string.Join("", responses) : null;
		}

		private static async Task<string?> ResponseLoot(Bot bot, ulong steamID, string mode, bool sendNotMarketable = true) {
			if (steamID == 0) {
				bot.ArchiLogger.LogNullError(nameof(steamID));

				return null;
			}

			if (!bot.HasAccess(steamID, BotConfig.EAccess.Master)) {
				return null;
			}

			if (!bot.IsConnectedAndLoggedOn) {
				return bot.Commands.FormatBotResponse(ArchiSteamFarm.Localization.Strings.BotNotConnected);
			}

			if (bot.BotConfig.LootableTypes.Count == 0) {
				return bot.Commands.FormatBotResponse(string.Format(ArchiSteamFarm.Localization.Strings.ErrorIsEmpty, nameof(Bot.BotConfig.LootableTypes)));
			}

			string[] modes = mode.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

			if (modes.Length == 0) {
				return bot.Commands.FormatBotResponse(string.Format(ArchiSteamFarm.Localization.Strings.ErrorIsEmpty, nameof(modes)));
			}

			HashSet<Asset.EType> transferTypes = new();

			foreach (string singleMode in modes) {
				switch (singleMode.ToUpper()) {
					case "A":
					case "ALL":
						foreach (Asset.EType type in (Asset.EType[]) Enum.GetValues(typeof(Asset.EType))) {
							transferTypes.Add(type);
						}

						break;
					case "BG":
					case "BACKGROUND":
						transferTypes.Add(Asset.EType.ProfileBackground);
						break;
					case "BO":
					case "BOOSTER":
						transferTypes.Add(Asset.EType.BoosterPack);
						break;
					case "C":
					case "CARD":
						transferTypes.Add(Asset.EType.TradingCard);
						break;
					case "E":
					case "EMOTICON":
						transferTypes.Add(Asset.EType.Emoticon);
						break;
					case "F":
					case "FOIL":
						transferTypes.Add(Asset.EType.FoilTradingCard);
						break;
					case "G":
					case "GEMS":
						transferTypes.Add(Asset.EType.SteamGems);
						break;
					case "U":
					case "UNKNOWN":
						transferTypes.Add(Asset.EType.Unknown);
						break;
					default:
						return bot.Commands.FormatBotResponse(string.Format(ArchiSteamFarm.Localization.Strings.ErrorIsInvalid, mode));
				}
			}

			(bool success, string message) = await bot.Actions.SendInventory(filterFunction: item => transferTypes.Contains(item.Type)&&(sendNotMarketable||item.Marketable)).ConfigureAwait(false);

			return bot.Commands.FormatBotResponse(success ? message : string.Format(ArchiSteamFarm.Localization.Strings.WarningFailedWithError, message));
		}


		private static async Task<string?> ResponseLoot(ulong steamID, string botNames, string mode, bool sendNotMarketable = true) {
			if ((steamID == 0) || string.IsNullOrEmpty(botNames)) {
				ASF.ArchiLogger.LogNullError(nameof(steamID) + " || " + nameof(botNames));

				return null;
			}

			HashSet<Bot>? bots = Bot.GetBots(botNames);

			if ((bots == null) || (bots.Count == 0)) {
				return ASF.IsOwner(steamID) ? Commands.FormatStaticResponse(string.Format(ArchiSteamFarm.Localization.Strings.BotNotFound, botNames)) : null;
			}

			IList<string?> results = await Utilities.InParallel(bots.Select(curbot => ResponseLoot(curbot, steamID, mode,sendNotMarketable))).ConfigureAwait(false);

			List<string?> responses = new(results.Where(result => !string.IsNullOrEmpty(result)));

			return responses.Count > 0 ? string.Join(Environment.NewLine, responses) : null;
		}

	}
}
