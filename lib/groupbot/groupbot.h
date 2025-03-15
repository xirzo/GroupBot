#ifndef GROUPBOT_H
#define GROUPBOT_H

namespace groupbot {
typedef struct bot bot;

bot* create(const char* token);
void start(bot* bot);
void free(bot* bot);

}  // namespace groupbot

#endif  // GROUPBOT_H
