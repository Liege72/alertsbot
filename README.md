# AlertsBot

## Preparation
In preparation of creating this bot, you should have the following:
- A Discord application made with the [discord developer portal](https://discord.com/developers/applications)
- A [MongoDB](https://mongodb.com) database of your own
- A basic understanding how how to use the discord developer portal
---
## Quick start

### Step One
After cloning this respiratory, make sure you have the following packages installed:<br /><br />
**Discord.Net Version 3.2.0 +** 
```
Install-Package Discord.Net -Version 3.2.0
```
**MongoDB.Driver Version 2.14.1 +** 
```
Install-Package MongoDB.Driver -Version 2.14.1
```
**MongoDB.Bson Version 2.14.1 +** 
```
Install-Package MongoDB.Bson -Version 2.14.1
```

### Step Two
Open the `Config.jsonc` file and enter your custom settings.
- **BotToken:** This is your discord applications API key.
- **MongoCS:** This is your mongodb connection string; these usually start with `mongodb://`
- **DatabaseName:** This will be the name of the database your collection will be stored in
- **CollectionName:** This will be the name of the collection that the bots data will be stored in
- **Prefix:** This is the prefix you want your bot to use
- **CustomStatus:** This is the custom status you want your bot to have, leave `null` if you want to keep it blank
- **AdminUser:** This is the ID of the user that can use the send, enable and disable commands

### Step Three
Run the `AlertsBot.exe` file to start the bot. The bot should be up and running and you can use it as you wish! See the next section to see the available commands.

---

## Bot Use (Commands)
*Assume the prefix is `!`*<br />

**Bot Setup**<br />
`!setup <channel>` This command will store the GuildID and target ChannelID to the database to allow alerts to be sent there. This command must be used before receiving alerts.

**Mention Role**<br />
`!mentionrole <role>` If a server wants to mention a role for every alert, they can use this command to allow this. *You cannot allow  @ everyone pings YET*

**Enable/Disable Alerts**<br />
`!enable <guildId>` `!disable <guildid>` These commands will enable/disable alerts on the targeted server. If a bot has been setup in a server, this command must be used by the bot owner to enable the alerts.

I recommended giving the bot administrator permissions simply to prevent errors from occurring.

---

## FAQ
**Q: Do I have to use MongoDB?**<br />
A: No, but if you do not, you will have to implement your own database handler/service.

**Q: I used the `send` command but I am not seeing any alerts being sent.**<br />
A: Make sure you have used the `enable` command to enable alerts on the server. 
