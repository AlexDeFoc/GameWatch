cmake_minimum_required(VERSION 4.2.2)
project(GameWatch LANGUAGES CXX)

add_executable(GameWatchCon)

set_target_properties(GameWatchCon PROPERTIES
    CXX_STANDARD          23
    CXX_STANDARD_REQUIRED ON
    CXX_EXTENSIONS        OFF
)

target_precompile_headers(GameWatchCon PRIVATE "Code/GameWatchCon/Includes/Pch.hpp")

target_sources(GameWatchCon
    PRIVATE
    "${CMAKE_CURRENT_SOURCE_DIR}/Code/GameWatchCon/Sources/Core/AppController.cpp"
    "${CMAKE_CURRENT_SOURCE_DIR}/Code/GameWatchCon/Sources/Core/MessageQueue.cpp"
    "${CMAKE_CURRENT_SOURCE_DIR}/Code/GameWatchCon/Sources/Core/ThreadPool.cpp"

    "${CMAKE_CURRENT_SOURCE_DIR}/Code/GameWatchCon/Sources/ProgramEntry.cpp"

    PUBLIC
    "${CMAKE_CURRENT_SOURCE_DIR}/Code/GameWatchCon/Includes/Core/AppController.hpp"
    "${CMAKE_CURRENT_SOURCE_DIR}/Code/GameWatchCon/Includes/Core/Handler.hpp"
    "${CMAKE_CURRENT_SOURCE_DIR}/Code/GameWatchCon/Includes/Core/Message.hpp"
    "${CMAKE_CURRENT_SOURCE_DIR}/Code/GameWatchCon/Includes/Core/MessageQueue.hpp"
    "${CMAKE_CURRENT_SOURCE_DIR}/Code/GameWatchCon/Includes/Core/ThreadPool.hpp"
    
    "${CMAKE_CURRENT_SOURCE_DIR}/Code/GameWatchCon/Includes/Messages/Quit.hpp"
    "${CMAKE_CURRENT_SOURCE_DIR}/Code/GameWatchCon/Includes/Messages/BlockingMessageTest.hpp"

    "${CMAKE_CURRENT_SOURCE_DIR}/Code/GameWatchCon/Includes/Pch.hpp"
    "${CMAKE_CURRENT_SOURCE_DIR}/Code/GameWatchCon/Includes/PlatformSpecificPch.hpp"
)

target_include_directories(GameWatchCon PRIVATE "${CMAKE_CURRENT_SOURCE_DIR}/Code/GameWatchCon/Includes")

# -------------------------------------------------------------------
# 3. COMPILER OPTIONS
# -------------------------------------------------------------------
if(MSVC AND NOT CMAKE_CXX_COMPILER_ID STREQUAL "Clang")
    target_compile_options(GameWatchCon PRIVATE /W4 /WX /permissive-)
elseif(CMAKE_CXX_COMPILER_ID STREQUAL "Clang" AND MSVC)
    target_compile_options(GameWatchCon PRIVATE /W4 /WX)
elseif(CMAKE_CXX_COMPILER_ID STREQUAL "Clang")
    target_compile_options(GameWatchCon PRIVATE -Wall -Wextra -Wpedantic -Werror)
elseif(CMAKE_CXX_COMPILER_ID STREQUAL "GNU")
    target_compile_options(GameWatchCon PRIVATE -Wall -Wextra -Wpedantic -Werror)
endif()