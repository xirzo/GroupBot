# GroupBot

GroupBot — это Telegram-бот, предназначенный для управления списками участников группы.

## Особенности

- **/start**: Начать взаимодействие с ботом.
- **/addlist**: Добавить новый список участников.
- **/toend**: Переместить участника в конец списка.
- **/list**: Показать список участников.
- **/lists**: Показать все списки участников.
- **/swap**: Поменять местами участников.
- **Принять**: Принять запрос на обмен.
- **Отказаться**: Отклонить запрос на обмен.
- **/deletelist**: Удалить список участников. (Новое)
- **/start**: Открыть списки в личных сообщениях. (Новое)
- **/addlist**: Только администраторы могут использовать команду. (Новое)

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
    - Создайте файл `participants.json` и добавьте всех участников вашей группы.
    - Создайте файл `appsettings.json` в корне проекта со следующей структурой:
      ```json
      {
        "Tokens": {
          "BotToken": "YOUR_TELEGRAM_BOT_TOKEN"
        },
        "Database": {
          "Path": "/app/data/database.db"
        },
        "Participants": {
          "Path": "/app/data/participants.json"
        }
      }
      ```

4. Запустите контейнер:
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
    - Создайте файл `participants.json` и добавьте всех участников вашей группы.
    - Создайте файл `appsettings.json` в корне проекта со следующей структурой:
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

4. Запустите бота:
    ```sh
    dotnet run
    ```
