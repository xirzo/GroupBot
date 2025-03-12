cmake_minimum_required(VERSION 3.30.5)

project(
  bot
  DESCRIPTION "IS GroupBot"
  LANGUAGES CXX)

set(CMAKE_CXX_STANDARD_REQUIRED True)
set(Boost_USE_MULTITHREADED ON)

find_package(Threads REQUIRED)
find_package(OpenSSL REQUIRED)
find_package(
  Boost
  COMPONENTS system
  REQUIRED)
find_package(CURL)
include_directories(/usr/local/include ${OPENSSL_INCLUDE_DIR}
                    ${Boost_INCLUDE_DIR})

if(CURL_FOUND)
  include_directories(${CURL_INCLUDE_DIRS})
  add_definitions(-DHAVE_CURL)
endif()

add_subdirectory(bin)
add_subdirectory(lib)
