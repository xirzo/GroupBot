#include "database.h"

#include <SQLiteCpp/Database.h>

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

std::int32_t addUser(db* db, const int64_t& telegram_id, const char* full_name,
                     const char* telegram_name) {
    try {
        SQLite::Statement query(
            *db->db,
            "INSERT INTO user (telegram_id, full_name, telegram_name) "
            "VALUES (?, ?, ?)");

        query.bind(1, telegram_id);
        query.bind(2, full_name);
        query.bind(3, telegram_name);

        query.exec();

        return static_cast<int32_t>(db->db->getLastInsertRowid());
    }
    catch (const SQLite::Exception& e) {
        fprintf(stderr, "error: SQLite error in addUser: %s\n", e.what());
        return -1;
    }
}

void free(db* db) {
    delete db->db;

    delete db;
}

}  // namespace database
