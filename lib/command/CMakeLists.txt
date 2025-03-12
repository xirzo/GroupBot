add_library(command STATIC command.cpp)

target_include_directories(command PUBLIC ${CMAKE_CURRENT_SOURCE_DIR})
