#include "groupbot.h"

#include <tgbot/tgbot.h>

#include <cstdio>
#include <nlohmann/json.hpp>
#include <nlohmann/json_fwd.hpp>

#include "database.h"

namespace groupbot {

typedef struct bot
{
    TgBot::Bot* bot;
    database::db* db;
} bot;

void __readUsers(bot* b, const char* users_config_filename) {
    std::ifstream users_file(users_config_filename);

    if (!users_file.is_open()) {
        fprintf(stderr, "error: Failed to open %s\n", users_config_filename);
        return;
    }

    nlohmann::json users_json;

    try {
        users_json = nlohmann::json::parse(users_file);
    }
    catch (const nlohmann::json::parse_error& e) {
        fprintf(stderr, "error: Failed to parse json %s : %s\n", users_config_filename,
                e.what());
        return;
    }

    if (!users_json.is_array()) {
        fprintf(stderr, "error: %s is not an array\n", users_config_filename);
        return;
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

            if (!database::addUserIfNotPresent(b->db, telegram_id, full_name.c_str(),
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
}

void __readAdmins(bot* b, const char* admins_config_filename) {
    std::ifstream admins_file(admins_config_filename);

    if (!admins_file.is_open()) {
        fprintf(stderr, "error: Failed to open %s\n", admins_config_filename);
        return;
    }

    nlohmann::json admins_json;

    try {
        admins_json = nlohmann::json::parse(admins_file);
    }
    catch (const nlohmann::json::parse_error& e) {
        fprintf(stderr, "error: Failed to parse json %s : %s\n", admins_config_filename,
                e.what());
        return;
    }

    if (!admins_json.is_array()) {
        fprintf(stderr, "error: %s is not an array\n", admins_config_filename);
        return;
    }

    for (const nlohmann::json& j : admins_json) {
        try {
            if (!j.is_number_integer()) {
                fprintf(stderr, "error: Admins json array contains non-integer value\n");
                continue;
            }

            std::int32_t admin_user_id = j.get<std::int32_t>();

            if (!database::addAdminIfNotPresent(b->db, admin_user_id)) {
                fprintf(stderr, "error: Failed to add admin to database, admin id: %d\n",
                        admin_user_id);
            }
        }
        catch (const std::exception& e) {
            fprintf(stderr, "error: Failed to process admin %s\n", e.what());
        }
    }
}

bot* create(const char* token, const char* users_config_filename,
            const char* admins_config_filename) {
    bot* b = new bot;

    b->bot = new TgBot::Bot(token);
    b->db = database::create("bot_data.db");

    __readUsers(b, users_config_filename);
    __readAdmins(b, admins_config_filename);

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
