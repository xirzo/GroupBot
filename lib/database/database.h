#ifndef DATABASE_H
#define DATABASE_H

#include <cstdint>
namespace database {
typedef struct db db;

db* create(const char* filename);
void free(db* db);

std::int32_t addUserIfNotPresent(db* db, const int64_t& telegram_id,
                                 const char* full_name, const char* telegram_name);
std::int32_t addAdminIfNotPresent(db* db, const int32_t& user_id);
std::int32_t addList(db* db, const char* list_name);
std::int32_t shuffleList(db* db, const std::int32_t& list_id);

}  // namespace database
#endif  // DATABASE_H
