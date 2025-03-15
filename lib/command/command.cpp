#include "command.h"

#include <sstream>

namespace command {

void registerCommand(repository* repo, const std::string& command_name,
                     CommandFunction function) {
    if (!repo) {
        fprintf(stderr, "error: Cannot register command - null repository\n");
        return;
    }

    repo->commands[command_name] = function;
}

bool executeCommand(repository* repo, void* context,
                    const std::string& command_with_args) {
    if (!repo) {
        fprintf(stderr, "error: Null repository pointer\n");
        return false;
    }

    std::istringstream iss(command_with_args);
    std::string command_name;
    iss >> command_name;

    auto it = repo->commands.find(command_name);
    if (it == repo->commands.end()) {
        fprintf(stderr, "error: Unknown command '%s'\n", command_name.c_str());
        return false;
    }

    std::vector<std::string> args;
    std::string arg;
    while (iss >> arg) {
        args.push_back(arg);
    }

    try {
        it->second(context, args);
        return true;
    }
    catch (const std::exception& e) {
        fprintf(stderr, "error: Command execution failed: %s\n", e.what());
        return false;
    }
}

}  // namespace command
