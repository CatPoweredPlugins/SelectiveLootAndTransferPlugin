# Selective Loot and Transfer Plugin

# Introduction
This plugin allows to send community inventory items of given types from bot to master or to another bot.

## Installation
- download .zip file from [latest release](https://github.com/Ryzhehvost/Selective-Loot-and-Transfer-Plugin/releases/latest), in most cases you need `Selective-Loot-and-Transfer-Plugin.zip`, but if you use ASF-generic-netf.zip (you really need a strong reason to do that) download `Selective-Loot-and-Transfer-Plugin-netf.zip`.
- unpack downloaded .zip file to `plugins` folder inside your ASF folder.
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

You can get support for this plugin in https://steamcommunity.com/groups/Ryzheplugins (or just use github issues).

---


# Плагин для выборочных loot и transfer

# Введение
Этот палгин позволяет отправлять предметы заданных типов из инвенаря сообщества от заданных ботов другому боту или аккаунту, указанному как `Master`.

## Установка
- скачайте файл .zip из [последнего релиза](https://github.com/Ryzhehvost/Selective-Loot-and-Transfer-Plugin/releases/latest), в большинстве случаев вам нужен файл `Selective-Loot-and-Transfer-Plugin.zip`, не если вы по какой-то причине пользуетесь ASF-generic-netf.zip (а для этого нужны веские причины) - скачайте `Selective-Loot-and-Transfer-Plugin-netf.zip`.
- распакуйте скачанный файл .zip в папку `plugins` внутри вашей папки с ASF.
- (пере)запустите ASF, вы должны получить сообщение что плагин успешно загружен. 

## Использование
Плагин реализует следующие команды:

Команда | Доступ | Описание
--- | --- | ---
`transfer# <Bots> <Modes> <Bot>` | `Master` | Отправляет боту `Bot` от заданных ботов все предметы инвентаря, соответствующие `modes`, описанным **[ниже](#user-content-параметр-modes)**.
`loot# <Bots> <Modes>` | `Master` | Отправляет все предметы инвентаря, соответствующие `modes`, описанным **[ниже](#user-content-параметр-modes)**, от заданных ботов к их `Master` заданному в `SteamUserPermissions` (с самым меньшим steamID, если их больше одного).
`transferm <Bots> <Modes> <Bot>` | `Master` | Аналогично `transfer#`, но отправляются только предметы, которые **можно продать** на торговой площадке Steam.
`lootm <Bots> <Modes>` | `Master` | Аналогично `loot#`, но отправляются только предметы, которые **можно продать** на торговой площадке Steam.



## Параметр `Modes`
Параметр `<Modes>` может включать в себя несколько настроек режима передачи предметов, разделённых запятыми. Допустимые обозначения режима перечислены ниже:

| Значение   | Сокращение | Описание                                                                  |
| ---------- | ---------- | ------------------------------------------------------------------------- |
| All        | A          | Все типы предметов, указанные ниже                                        |
| Background | BG         | Фоны, используемые в вашем профиле Steam                                  |
| Booster    | BO         | Наборы карточек                                                           |
| Card       | C          | Коллекционные карточки Steam, используемые для создания значков (обычных) |
| Emoticon   | E          | Смайлики, используемые в чате Steam                                       |
| Foil       | F          | Аналог `Card`, но для металлических карточек                              |
| Gems       | G          | Самоцветы и мешки самоцветов, используемые для создания наборов карточек  |
| Unknown    | U          | Типы предметов, которые не попадают ни в одну из категорий выше           |

Например, чтобы передать обычные и металлические коллекционные карточки от бота `MyBot` боту `MyMain`, вы используете команду:

`transfer# MyBot C,F MyMain`

Помощь по этому плагину вы можете получить в https://steamcommunity.com/groups/Ryzheplugins (или просто используйте раздел issues)

![downloads](https://img.shields.io/github/downloads/Ryzhehvost/Selective-Loot-and-Transfer-Plugin/total.svg?style=social)
