add_executable(${PROJECT_NAME} main.cpp)

target_link_libraries(
  ${PROJECT_NAME}
  PRIVATE /usr/local/lib/libTgBot.a ${CMAKE_THREAD_LIBS_INIT}
          ${OPENSSL_LIBRARIES} ${Boost_LIBRARIES} ${CURL_LIBRARIES})
