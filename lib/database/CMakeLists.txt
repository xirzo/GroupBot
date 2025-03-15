add_library(database STATIC database.cpp)

target_link_libraries(database PRIVATE SQLiteCpp)

target_include_directories(database PUBLIC ${CMAKE_CURRENT_SOURCE_DIR})
