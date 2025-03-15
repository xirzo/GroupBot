add_library(groupbot STATIC groupbot.cpp)

target_link_libraries(
  groupbot
  PRIVATE nlohmann_json::nlohmann_json
          database
          /usr/local/lib/libTgBot.a
          ${CMAKE_THREAD_LIBS_INIT}
          ${OPENSSL_LIBRARIES}
          ${Boost_LIBRARIES}
          ${CURL_LIBRARIES})

target_include_directories(groupbot PUBLIC ${CMAKE_CURRENT_SOURCE_DIR})
