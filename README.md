# GroupBot

GroupBot — это Telegram-бот, предназначенный для управления списками участников группы.

## Особенности

- **/start**: Начать взаимодействие с ботом. Работает только в лс.
- **/addlist**: Добавить новый список участников. Только администраторы могут использовать команду.
- **/removelist**: Удалить список участников. Только администраторы могут использовать команду.
- **/toend**: Переместить участника в конец списка.
- **/list**: Показать список участников.
- **/lists**: Показать все списки участников.
- **/swap**: Поменять местами участников.
- **Принять**: Принять запрос на обмен.
- **Отказаться**: Отклонить запрос на обмен.

## Установка

### Через Docker

1. Клонируйте репозиторий:
    ```sh
    git clone https://github.com/xirzo/GroupBot.git
    cd GroupBot
    ```

2. Соберите:
    ```sh
    docker build -t groupbot .
    ```

3. Создайте конфигурационные файлы:
    - Добавьте свой ключ в переменную окружения
   ```bash
	export BOT_TOKEN="your-telegram-bot-token-here"
   ```
    - Создайте файл `participants.json` со всеми участниками вашей группы и положите его в GroupBot/GroupBot.Program/participants.json.
    - Создайте файл `appsettings.json` со следующей структурой и положите его в GroupBot/GroupBot.Program/participants.json:
      ```json
      {
        "Database": {
          "Path": "/app/data/database.db"
        }
      }
      ```

5. Запустите контейнер:
    ```sh
    docker run -d -p 8080:80 --name groupbot -v bot_data:/app/data groupbot
    ```

### Через .NET CLI

1. Клонируйте репозиторий:
    ```sh
    git clone https://github.com/xirzo/GroupBot.git
    cd GroupBot
    ```

2. Установите зависимости:
    ```sh
    dotnet restore
    ```

3. Настройте конфигурационные файлы:
    - Добавьте свой ключ в переменную окружения
   ```bash
	export BOT_TOKEN="your-telegram-bot-token-here"
   ```
    - Создайте файл `participants.json` со всеми участниками вашей группы и положите его в GroupBot/GroupBot.Program/participants.json.
    - Создайте файл `appsettings.json` со следующей структурой и положите его в GroupBot/GroupBot.Program/participants.json:
      ```json
      {
        "Database": {
          "Path": "/app/data/database.db"
        }
      }
      ```

4. Запустите бота:
    ```sh
    dotnet run
    ```
