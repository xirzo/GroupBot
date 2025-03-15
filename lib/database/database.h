#ifndef DATABASE_H
#define DATABASE_H

#include <cstdint>
namespace database {
typedef struct db db;

db* create(const char* filename);
void free(db* db);

std::int32_t addUser(db* db, const int64_t& telegram_id, const char* full_name,
                     const char* telegram_name);

}  // namespace database
#endif  // DATABASE_H
