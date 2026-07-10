cmake_minimum_required(VERSION 4.3.0)
project(GameWatch.Client.Cli LANGUAGES CXX)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

set(TARGET_NAME "GameWatch.Client.Cli")

add_executable(${TARGET_NAME})

set_target_properties(${TARGET_NAME} PROPERTIES
    CXX_STANDARD 23
    CXX_STANDARD_REQUIRED ON
    CXX_EXTENSIONS OFF
)

target_include_directories(${TARGET_NAME} PRIVATE "${CMAKE_CURRENT_SOURCE_DIR}/Includes")
target_sources(${TARGET_NAME} PRIVATE "${CMAKE_CURRENT_SOURCE_DIR}/Sources/AppEntry/main.cpp")

if (CMAKE_HOST_SYSTEM_NAME STREQUAL "Windows")
    if (CMAKE_CXX_COMPILER_FRONTEND_VARIANT STREQUAL "MSVC" AND CMAKE_CXX_COMPILER_ID STREQUAL "MSVC")
        target_compile_options(${TARGET_NAME} PRIVATE
            /permissive-
            /Zc:__cplusplus
            /Zc:preprocessor
            /Wall
            /WX
            /utf-8
            /diagnostics:caret
            /GL
            /external:W0
            /external:anglebrackets
            /wd4514 # "unreferenced inline function has been removed" (Very common noise with /Wall)
            /wd4710 # "function not inlined"
            /wd4711 # "function selected for automatic inline expansion"
            /wd4820 # "bytes padding added after data member"
            /wd5045 # "Compiler will insert Spectre mitigation for memory load"
        )

        target_link_options(${TARGET_NAME} PRIVATE
            /LTCG
        )
    elseif(CMAKE_CXX_COMPILER_FRONTEND_VARIANT STREQUAL "MSVC" AND CMAKE_CXX_COMPILER_ID STREQUAL "Clang")
        target_compile_options(${TARGET_NAME} PRIVATE
                /permissive-
                /Zc:__cplusplus
                /Wall
                /WX
                /utf-8
                /diagnostics:caret
                /external:W0
                -Wno-c++98-compat
                -Wno-newline-eof
        )

        target_link_options(${TARGET_NAME} PRIVATE
                /LTCG
        )
    endif()
endif()

find_program(CLANG_TIDY_EXE NAMES "clang-tidy")
if(CLANG_TIDY_EXE)
    message(STATUS "Found clang-tidy: ${CLANG_TIDY_EXE}")
    set_target_properties(${TARGET_NAME} PROPERTIES
        CXX_CLANG_TIDY "${CLANG_TIDY_EXE};--config-file=${CMAKE_CURRENT_SOURCE_DIR}/.clang-tidy"
    )
else()
    message(WARNING "clang-tidy executable was NOT found on this system!")
endif()

# 1. Enable CMake's test harness
enable_testing()

# 2. Pull in the Tests subdirectory layout
add_subdirectory(Tests)
