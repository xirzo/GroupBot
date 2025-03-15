#ifndef DATABASE_H
#define DATABASE_H

namespace database {
typedef struct db db;

db* create(const char* filename);
void free(db* db);

}  // namespace database
#endif  // DATABASE_H
