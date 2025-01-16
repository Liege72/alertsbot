*After several years of selling this simple bot, I have decided to make the code public to contribute to the community and showcase more of my work on GitHub. Please note that this code was written several years ago and may not represent my current proficiency with C#.

# AlertsBot
This bot was originally developed for day traders who requested a custom solution and has primarily been sold to day traders and cryptocurrency enthusiasts. The bot is designed to send alerts to servers where it has been enabled. Using a simple slash command, an admin_user (see details below) can broadcast alerts to all servers where they have enabled the bot. While the bot can be added to multiple servers, it will only forward alerts to servers explicitly enabled by an admin_user.

## Preparation
In preparation of creating this bot, you should have the following:
- A Discord application made with the [discord developer portal](https://discord.com/developers/applications)
- A [MongoDB](https://mongodb.com) database of your own
- A basic understanding how how to use the discord developer portal
- A method of hosting this bot using a server, docker, whatever the case may be for you
---
## Quick start

### Step One
After cloning this repository, make sure you have the following packages installed:<br /><br />
**Discord.Net Version 3.17.0 +** 
```
Install-Package Discord.Net -Version 3.17.0
```
**MongoDB.Driver Version 3.1.0 +** 
```
Install-Package MongoDB.Driver -Version 3.1.0
```
**MongoDB.Bson Version 3.1.0 +** 
```
Install-Package MongoDB.Bson -Version 3.1.0
```

### Step Two
Open the `Config.jsonc` file and enter your custom settings.
- `token`: This is your Discord apps API key
- `mongo_connection_string`: This is your MongoDB connection string; it should start with `mongodb://`
- `database_name`: This will be the name of the database your collection will be stored in
- `collection_name`: This will be the name of the collection that the bots data will be stored in
- `custom_status`: This is the custom status you want your bot to have, leave `null` if you want to keep it blank
- `admin_users`: This is an array of Discord user ids for the users that you want to be able to control your bot and send alerts; ids should be strings not longs

### Step Three
Run the `AlertsBot.exe` file to start the bot. The bot should be up and running and you can use it as you wish! See the next section to see the available commands.

---

## Bot Use (Commands)

**Bot Setup**<br />
`/setup <channel>` This command will store the GuildID and target ChannelID to the database to allow alerts to be sent there. This command must be used before receiving alerts.

**Mention Role**<br />
`/mentionrole <role>` If a server wants to mention a role for every alert, they can use this command to allow this.

**Enable/Disable Alerts**<br />
`/enable <guildId>` `/disable <guildid>` These commands will enable/disable alerts on the targeted server. If a bot has been setup in a server, this command must be used by the bot owner to enable the alerts.

I recommended giving the bot administrator permissions simply to prevent errors from occurring.

---

## FAQ
**Q: Do I have to use MongoDB?**<br />
A: No, but if you do not, you will have to implement your own database handler/service.

**Q: I used the `send` command but I am not seeing any alerts being sent.**<br />
A: Make sure you have used the `enable` command to enable alerts on the server. 

**Q: Why can't I use the `setup` or `mentionrole` commands for a server?**<br />
A: Only users with administrator permissions are allowed to use these commands. 
