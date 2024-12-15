# GroupBot

GroupBot is a Telegram bot designed to create lists of group participants.

## Features

- **/start**: Start interaction with the bot.
- **/addlist**: Add a new list of participants.
- **/toend**: Move a participant to the end of the list.
- **/list**: Display the list of participants.
- **/lists**: Display all lists of participants.
- **/swap**: Swap the position of participants.
- **Принять**: Accept a swap request.
- **Отказаться**: Decline a swap request.

## Installation

1. Clone the repository:
   ```sh
   git clone https://github.com/xirzo/GroupBot.git
   cd GroupBot
   ```

2. Install the required dependencies:
   ```sh
   dotnet restore
   ```

3. Set up the configuration files:

   - Create a `participants.json` file and put all the members of your group

   - Create an `appsettings.json` file in the project root with the following structure:
     ```json
     {
       "Tokens": {
         "BotToken": "YOUR_TELEGRAM_BOT_TOKEN"
       },
       "Database": {
         "Path": "path/to/your/database.db"
       },
       "Participants": {
         "Path": "path/to/participants.json"
       }
     }
     ```

4. Run the bot:
   ```sh
   dotnet run
   ```

## Usage

1. Start the bot by running the installation steps above.
2. Interact with the bot on Telegram using the registered commands.
