#include "groupbot.h"

#include <tgbot/tgbot.h>

#include "database.h"

namespace groupbot {
typedef struct bot
{
    TgBot::Bot* bot;
    database::db* db;
} bot;

bot* create(const char* token) {
    bot* b = new bot;

    b->bot = new TgBot::Bot(token);
    b->db = database::create("bot_data.db");

    return b;
}

void start(bot* bot) {
    bot->bot->getEvents().onAnyMessage([&bot](TgBot::Message::Ptr message) {
        printf("Message: %s\n", message->text.c_str());
    });

    try {
        TgBot::TgLongPoll longPoll(*bot->bot);

        while (true) {
            longPoll.start();
        }
    }
    catch (TgBot::TgException& e) {
        fprintf(stderr, "error: %s\n", e.what());
    }
}

void free(bot* bot) {
    delete bot->bot;

    database::free(bot->db);

    delete bot;
}

}  // namespace groupbot
