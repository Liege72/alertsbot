# AlertsBot

## Quick start

### Step One
After cloning this respiratory, make sure you have the following packages installed:
- Discord.Net Version 3.2.0 + `Install-Package Discord.Net -Version 3.2.0`
- MongoDB.Driver Version 2.14.1 + `Install-Package MongoDB.Driver -Version 2.14.1`
- MongoDB.Bson Version 2.14.1 + `Install-Package MongoDB.Bson -Version 2.14.1`

### Step Two
Open the `Config.jsonc` file and enter your custom settings.
**BotToken:** This is your discord applications API key.
**MongoCS:** This is your mongodb connection string; these usually start with mongodb://
**Database/Collection Name:** These will be the names of your database and collection that your bot will store info in
**Prefix:** The prefix you want your bot to use
**CustomStatus:** The custom status you want your bot to have, leave `null` if you want to keep it blank
**AdminUser:** The ID of the user that can use the send, enable and disable commands
