# Selective Loot and Transfer Plugin

# Introduction
This plugin allows to send community inventory items of given types from bot to master or to another bot.

## Installation
- this plugin is only *guaranteed* to work with ASF-generic. If you use any other variant and encounter any issues - first you are encouraged to switch to ASF-generic, and only if it does not help - report an issue.
- download `SelectiveLootAndTransferPlugin.zip` file from [latest release](https://github.com/Rudokhvist/SelectiveLootAndTransferPlugin/releases/latest) page.
- unpack downloaded .zip file to `plugins` folder inside your ASF folder, together with included folder.
- (re)start ASF, you should get a message indicating that plugin loaded successfully. 

## Usage
Plugin implements following commands:

Command | Access | Description
--- | --- | ---
`transfer# <Bots> <Modes> <Bot>` | `Master` | Sends from given bot instances to given `Bot` instance, all inventory items that are matching given `modes`, explained **[below](#modes-parameter)**.
`loot# <Bots> <Modes>` | `Master` | Sends all inventory items that are matching given `modes`, explained **[below](#modes-parameter)**, from given bot instances to `Master` user defined in their `SteamUserPermissions` (with lowest steamID if more than one).
`transferm <Bots> <Modes> <Bot>` | `Master` | Same as `transfer#`, but only **Marketable** items are sent.
`lootm <Bots> <Modes>` | `Master` | Same as `loot#`, but only **Marketable** items are sent.

## `Modes` parameter

`<Modes>` argument accepts multiple mode values, separated as usual by a comma. Available mode values are specified below:

Value | Alias | Description
--- | --- | ---
All | A | Same as enabling all item types below
Background | BG | Profile background to use on your Steam profile
Booster | BO | Booster pack
Card | C | Steam trading card, being used for crafting badges (non-foil)
Emoticon | E | Emoticon to use in Steam Chat
Foil | F | Foil variant of `Card`
Gems | G | Steam gems being used for crafting boosters, sacks included
Unknown | U | Every type that doesn't fit in any of the above

For example, in order to send trading cards and foils from `MyBot` to `MyMain`, you'd execute:

`transfer# MyBot C,F MyMain`

---

![downloads](https://img.shields.io/github/downloads/Rudokhvist/SelectiveLootAndTransferPlugin/total.svg?style=social)
