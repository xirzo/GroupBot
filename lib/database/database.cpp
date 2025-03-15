#include "database.h"

#include <SQLiteCpp/Database.h>
#include <SQLiteCpp/Transaction.h>

#include <algorithm>
#include <random>
#include <vector>

namespace database {

typedef struct db
{
    SQLite::Database* db;
} db;

db* create(const char* filename) {
    db* db = new struct db;

    try {
        db->db =
            new SQLite::Database(filename, SQLite::OPEN_READWRITE | SQLite::OPEN_CREATE);

        db->db->exec("PRAGMA foreign_keys = ON;");

        db->db->exec(
            "CREATE TABLE IF NOT EXISTS user (user_id INTEGER PRIMARY KEY, telegram_id "
            "INTEGER(8), full_name TEXT, "
            "telegram_name TEXT)");

        db->db->exec(
            "CREATE TABLE IF NOT EXISTS admin (admin_id INTEGER PRIMARY KEY, user_id "
            "INTEGER, "
            "FOREIGN KEY (user_id) REFERENCES user (user_id))");

        db->db->exec(
            "CREATE TABLE IF NOT EXISTS list (list_id INTEGER PRIMARY KEY, list_name "
            "TEXT)");

        db->db->exec(
            "CREATE TABLE IF NOT EXISTS list_user (list_user_id INTEGER PRIMARY KEY, "
            "list_id INTEGER, user_id INTEGER, user_position INTEGER, "
            "FOREIGN KEY (list_id) REFERENCES list (list_id), "
            "FOREIGN KEY (user_id) REFERENCES user (user_id))");
    }
    catch (const SQLite::Exception& e) {
        fprintf(stderr, "SQLite error: %s\n", e.what());
        delete db;
        return nullptr;
    }

    return db;
}

std::int32_t addUserIfNotPresent(db* db, const int64_t& telegram_id,
                                 const char* full_name, const char* telegram_name) {
    try {
        SQLite::Statement checkQuery(*db->db,
                                     "SELECT user_id FROM user WHERE telegram_id = ?");

        checkQuery.bind(1, telegram_id);

        if (checkQuery.executeStep()) {
            return checkQuery.getColumn(0).getInt();
        }

        SQLite::Statement insertQuery(
            *db->db,
            "INSERT INTO user (telegram_id, full_name, telegram_name) "
            "VALUES (?, ?, ?)");

        insertQuery.bind(1, telegram_id);
        insertQuery.bind(2, full_name);
        insertQuery.bind(3, telegram_name);

        insertQuery.exec();

        return static_cast<int32_t>(db->db->getLastInsertRowid());
    }
    catch (const SQLite::Exception& e) {
        fprintf(stderr, "error: SQLite error in addUserIfNotPresent: %s\n", e.what());
        return -1;
    }
}

std::int32_t addAdminIfNotPresent(db* db, const int32_t& user_id) {
    try {
        SQLite::Statement checkQuery(*db->db,
                                     "SELECT admin_id FROM admin WHERE user_id = ?");

        checkQuery.bind(1, user_id);

        if (checkQuery.executeStep()) {
            return checkQuery.getColumn(0).getInt();
        }

        SQLite::Statement insertQuery(*db->db,
                                      "INSERT INTO admin (user_id) "
                                      "VALUES (?)");

        insertQuery.bind(1, user_id);

        insertQuery.exec();

        return static_cast<int32_t>(db->db->getLastInsertRowid());
    }
    catch (const SQLite::Exception& e) {
        fprintf(stderr, "error: SQLite error in addAdminIfNotPresent: %s\n", e.what());
        return -1;
    }
}

std::int32_t addList(db* db, const char* list_name) {
    try {
        SQLite::Transaction transaction(*db->db);

        SQLite::Statement checkQuery(*db->db,
                                     "SELECT list_id FROM list WHERE list_name = ?");
        checkQuery.bind(1, list_name);

        std::int32_t list_id;
        if (checkQuery.executeStep()) {
            list_id = checkQuery.getColumn(0).getInt();
        } else {
            SQLite::Statement insertQuery(*db->db,
                                          "INSERT INTO list (list_name) VALUES (?)");
            insertQuery.bind(1, list_name);
            insertQuery.exec();

            list_id = static_cast<int32_t>(db->db->getLastInsertRowid());
        }

        SQLite::Statement getUsersQuery(*db->db, "SELECT user_id FROM user");

        int position = 0;

        while (getUsersQuery.executeStep()) {
            std::int32_t user_id = getUsersQuery.getColumn(0).getInt();

            SQLite::Statement checkUserInListQuery(
                *db->db, "SELECT 1 FROM list_user WHERE list_id = ? AND user_id = ?");
            checkUserInListQuery.bind(1, list_id);
            checkUserInListQuery.bind(2, user_id);

            if (!checkUserInListQuery.executeStep()) {
                SQLite::Statement addUserQuery(*db->db,
                                               "INSERT INTO list_user (list_id, user_id, "
                                               "user_position) VALUES (?, ?, ?)");
                addUserQuery.bind(1, list_id);
                addUserQuery.bind(2, user_id);
                addUserQuery.bind(3, position++);
                addUserQuery.exec();
            }
        }

        transaction.commit();

        return list_id;
    }
    catch (const SQLite::Exception& e) {
        fprintf(stderr, "error: SQLite error in addList: %s\n", e.what());
        return -1;
    }
}

std::int32_t shuffleList(db* db, const std::int32_t& list_id) {
    try {
        SQLite::Transaction transaction(*db->db);

        SQLite::Statement checkListQuery(*db->db, "SELECT 1 FROM list WHERE list_id = ?");

        checkListQuery.bind(1, list_id);

        if (!checkListQuery.executeStep()) {
            fprintf(stderr, "error: List with ID %d not found\n", list_id);
            return -1;
        }

        SQLite::Statement getListUserQuery(
            *db->db, "SELECT list_user_id, user_id FROM list_user WHERE list_id = ?");
        getListUserQuery.bind(1, list_id);

        struct ListUser
        {
            std::int32_t list_user_id;
            std::int32_t user_id;
        };

        std::vector<ListUser> listUsers;

        while (getListUserQuery.executeStep()) {
            ListUser user;
            user.list_user_id = getListUserQuery.getColumn(0).getInt();
            user.user_id = getListUserQuery.getColumn(1).getInt();
            listUsers.push_back(user);
        }

        if (listUsers.empty()) {
            fprintf(stderr, "warning: No users found in list %d to shuffle\n", list_id);
            return list_id;
        }

        std::random_device rd;
        std::mt19937 g(rd());
        std::shuffle(listUsers.begin(), listUsers.end(), g);

        SQLite::Statement updatePositionQuery(
            *db->db, "UPDATE list_user SET user_position = ? WHERE list_user_id = ?");

        for (size_t i = 0; i < listUsers.size(); ++i) {
            updatePositionQuery.reset();
            updatePositionQuery.bind(1, static_cast<int>(i));
            updatePositionQuery.bind(2, listUsers[i].list_user_id);
            updatePositionQuery.exec();
        }

        transaction.commit();

        return list_id;
    }
    catch (const SQLite::Exception& e) {
        fprintf(stderr, "error: SQLite error in shuffleList: %s\n", e.what());
        return -1;
    }
}

void free(db* db) {
    delete db->db;

    delete db;
}

}  // namespace database
