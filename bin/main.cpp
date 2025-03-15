#include <csignal>
#include <cstdio>
#include <cstdlib>
#include <cstring>

#include "groupbot.h"

int main() {
    const char* token(std::getenv("token"));

    if (token == nullptr || strlen(token) == 0) {
        fprintf(stderr, "error: token env is not set");
        return EXIT_FAILURE;
    }

    groupbot::bot* b = groupbot::create(token, "users.json", "admins.json");

    groupbot::start(b);

    groupbot::free(b);

    return EXIT_SUCCESS;
}
