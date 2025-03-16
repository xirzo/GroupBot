#include "groupbot.h"

#include <tgbot/tgbot.h>

#include <cstdio>
#include <nlohmann/json.hpp>
#include <nlohmann/json_fwd.hpp>

#include "command.h"
#include "database.h"

namespace groupbot {

typedef struct bot
{
    TgBot::Bot* bot;
    database::db* db;
    command::repository* repo;
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

            if (!database::addAdmin(b->db, admin_user_id)) {
                fprintf(stderr, "error: Failed to add admin to database, admin id: %d\n",
                        admin_user_id);
            }
        }
        catch (const std::exception& e) {
            fprintf(stderr, "error: Failed to process admin %s\n", e.what());
        }
    }
}

void __addListCommand(void* context, const std::vector<std::string>& args) {
    groupbot::bot* b = reinterpret_cast<groupbot::bot*>(context);

    if (args.empty()) {
        fprintf(stderr, "Usage: /addlist <list_name>\n");
        return;
    }

    printf("Creating list: %s\n", args[0].c_str());

    if (database::addList(b->db, args[0].c_str()) < 0) {
        fprintf(stderr, "Failed to create list: %s\n", args[0].c_str());
    } else {
        printf("List created successfully\n");
    }
}

void __removeListCommand(void* context, const std::vector<std::string>& args) {
    groupbot::bot* b = reinterpret_cast<groupbot::bot*>(context);

    if (args.empty()) {
        fprintf(stderr, "Usage: /removelist <list_name>\n");
        return;
    }

    printf("Removing list: %s\n", args[0].c_str());

    if (database::removeList(b->db, args[0].c_str()) < 0) {
        fprintf(stderr, "Failed to remove list: %s\n", args[0].c_str());
    } else {
        printf("List removed successfully\n");
    }
}

bot* create(const char* token, const char* users_config_filename,
            const char* admins_config_filename) {
    bot* b = new bot;

    b->bot = new TgBot::Bot(token);
    b->db = database::create("bot_data.db");

    __readUsers(b, users_config_filename);
    __readAdmins(b, admins_config_filename);

    b->repo = new command::repository;

    command::registerCommand(b->repo, "/addlist", __addListCommand);
    command::registerCommand(b->repo, "/removelist", __removeListCommand);

    database::swapUsers(b->db, "G", 4, 6);

    return b;
}

void start(bot* b) {
    b->bot->getEvents().onAnyMessage([&b](TgBot::Message::Ptr message) {
        command::executeCommand(b->repo, reinterpret_cast<void*>(b), message->text);
    });

    try {
        TgBot::TgLongPoll longPoll(*b->bot);

        while (true) {
            longPoll.start();
        }
    }
    catch (TgBot::TgException& e) {
        fprintf(stderr, "error: %s\n", e.what());
    }
}

void free(bot* bot) {
    if (!bot) {
        fprintf(stderr, "error: Bot is already freed\n");
        return;
    }

    delete bot->bot;
    delete bot->repo;

    database::free(bot->db);

    delete bot;
}

}  // namespace groupbot
