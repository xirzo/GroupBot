# GroupBot 🤖

GroupBot — это Telegram-бот для эффективного управления списками участников группы. 

## Особенности ✨

- **/start**: Начало взаимодействия с ботом (работает только в личных сообщениях).
- **/addlist**: Добавление нового списка участников (только администраторы).
- **/removelist**: Удаление списка участников (только администраторы).
- **/toend**: Перемещение участника в конец списка.
- **/list**: Отображение списка участников.
- **/lists**: Отображение всех списков участников.
- **/swap**: Обмен позиций между участниками.
- **Принять**: Принятие запроса на обмен.
- **Отказаться**: Отклонение запроса на обмен.

### Важно ⚠️

Для функции **swap** требуется, чтобы участник, принимающий запрос, до этого хотя бы раз писал в личные сообщения боту.

## Установка 🚀

### Запуск через Docker

1. **Клонируйте репозиторий:**

    ```sh
    git clone https://github.com/xirzo/GroupBot.git
    cd GroupBot
    ```

2. **Создайте конфигурационные файлы:**

    - Создайте файл `.env` в корневой директории проекта и добавьте в него ваш ключ:
      
      ```env
      BOT_TOKEN=YOUR_TOKEN
      ```

    - Создайте файл `participants.json` со списком всех участников вашей группы и поместите его в:
      
      ```plaintext
      GroupBot/GroupBot.Program/participants.json
      ```

    - Создайте файл `appsettings.json` с конфигурацией базы данных и поместите его также в директорию `GroupBot/GroupBot.Program/`:
      
      ```json
      {
        "Database": {
          "Path": "bot_data.db"
        }
      }
      ```

    - Если у вас уже есть база данных (`bot_data.db`), разместите её в:
      
      ```plaintext
      GroupBot/GroupBot.Program/bot_data.db
      ```

3. **Соберите и запустите бота:**

    Соберите образ:

    ```sh
    docker compose build
    ```

    Затем запустите контейнер:

    ```sh
    docker compose up
    ```
