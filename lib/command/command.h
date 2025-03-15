#ifndef COMMAND_H
#define COMMAND_H

#include <functional>
#include <string>
#include <unordered_map>
#include <vector>

namespace command {

typedef std::function<void(void* context, const std::vector<std::string>&)>
    CommandFunction;

typedef struct repository
{
    std::unordered_map<std::string, CommandFunction> commands;
} repository;

void registerCommand(repository* repo, const std::string& command_name,
                     CommandFunction function);

bool executeCommand(repository* repo, void*, const std::string& command_with_args);

}  // namespace command

#endif  // COMMAND_H
