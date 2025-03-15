#include "groupbot.h"

#include <tgbot/tgbot.h>

#include <cstdio>
#include <nlohmann/json.hpp>

#include "database.h"

namespace groupbot {

typedef struct bot
{
    TgBot::Bot* bot;
    database::db* db;
} bot;

bot* create(const char* token, const char* users_config_filename,
            const char* admins_config_filename) {
    bot* b = new bot;

    b->bot = new TgBot::Bot(token);
    b->db = database::create("bot_data.db");

    std::ifstream users_file(users_config_filename);

    if (!users_file.is_open()) {
        fprintf(stderr, "error: Failed to open %s\n", users_config_filename);
        return nullptr;
    }

    nlohmann::json users_json;

    try {
        users_json = nlohmann::json::parse(users_file);
    }
    catch (const nlohmann::json::parse_error& e) {
        fprintf(stderr, "error: Failed to parse json %s : %s\n", users_config_filename,
                e.what());
        return nullptr;
    }

    if (!users_json.is_array()) {
        fprintf(stderr, "error: %s is not an array\n", users_config_filename);
        return nullptr;
    }

    for (std::size_t i = 0; i < users_json.size(); ++i) {
        try {
            nlohmann::json& j = users_json[i];

            if (!j.contains("telegram_id") || !j["telegram_id"].is_number_integer()) {
                fprintf(stderr, "error: User entry %zu missing valid telegram_id\n", i);
                continue;
            }

            if (!j.contains("full_name") || !j["full_name"].is_string()) {
                fprintf(stderr, "error: User entry %zu missing valid full_name\n", i);
                continue;
            }

            if (!j.contains("telegram_name") || !j["telegram_name"].is_string()) {
                fprintf(stderr, "error: User entry %zu missing valid telegram_name\n", i);
                continue;
            }

            std::int64_t telegram_id = j["telegram_id"].get<std::int64_t>();
            std::string full_name = j["full_name"].get<std::string>();
            std::string telegram_name = j["telegram_name"].get<std::string>();

            if (!database::addUser(b->db, telegram_id, full_name.c_str(),
                                   telegram_name.c_str())) {
                fprintf(stderr, "error: Failed to add user to database: %s\n",
                        full_name.c_str());
            }
        }
        catch (const std::exception& e) {
            fprintf(stderr, "error: Failed to process user at index %zu: %s\n", i,
                    e.what());
        }
    }

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
