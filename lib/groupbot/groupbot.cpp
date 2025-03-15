#include "groupbot.h"

#include <tgbot/tgbot.h>

namespace groupbot {
typedef struct bot
{
    TgBot::Bot* bot;
} bot;

bot* create(const char* token) {
    bot* b = (bot*)malloc(sizeof(*b));

    b->bot = new TgBot::Bot(token);

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
    delete[] bot->bot;

    free(bot);
}

}  // namespace groupbot
