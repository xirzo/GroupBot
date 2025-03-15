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
            "INTEGER, full_name TEXT, "
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
            "list_id INTEGER, user_id INTEGER, "
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

void free(db* db) {
    delete db->db;

    delete db;
}

}  // namespace database
