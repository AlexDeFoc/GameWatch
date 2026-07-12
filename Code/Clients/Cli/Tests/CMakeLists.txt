cmake_minimum_required(VERSION 4.2.3)
project(GameWatch.Client.Cli.Tests LANGUAGES CXX)

set(TARGET_NAME "GameWatch.Client.Cli.Tests")
add_executable(${TARGET_NAME})

set_target_properties(${TARGET_NAME} PROPERTIES
        CXX_STANDARD 23
        CXX_STANDARD_REQUIRED ON
        CXX_EXTENSIONS OFF
)

# Test-specific sources
target_sources(${TARGET_NAME} PRIVATE "${CMAKE_CURRENT_SOURCE_DIR}/Sources/FirstTest.cpp")

# --- COMPILER FLAGS (Tests) ---
# Note: I removed /GL and -flto (LTCG) from the test target.
# Link-Time Optimization slows down test iteration loops dramatically and can interfere with coverage.
if (CMAKE_HOST_SYSTEM_NAME STREQUAL "Windows" AND CMAKE_CXX_COMPILER_FRONTEND_VARIANT STREQUAL "MSVC")
    if (CMAKE_CXX_COMPILER_ID STREQUAL "MSVC")
        target_compile_options(${TARGET_NAME} PRIVATE
                /permissive- /Zc:__cplusplus /Zc:preprocessor /W4 /WX /utf-8 /diagnostics:caret
        )
    endif ()
elseif (CMAKE_HOST_SYSTEM_NAME STREQUAL "Linux" AND CMAKE_CXX_COMPILER_FRONTEND_VARIANT STREQUAL "GNU")
    target_compile_options(${TARGET_NAME} PRIVATE -Wall -Wextra -Werror -fdiagnostics-color=always)
endif ()

if (COMPILER_COVERAGE_FLAGS)
    separate_arguments(COMPILER_ARGS_LIST NATIVE_COMMAND "${COMPILER_COVERAGE_FLAGS}")
    separate_arguments(LINKER_ARGS_LIST NATIVE_COMMAND "${LINKER_COVERAGE_FLAGS}")
    target_compile_options(${TARGET_NAME} PRIVATE ${COMPILER_ARGS_LIST})
    target_link_options(${TARGET_NAME} PRIVATE ${LINKER_ARGS_LIST})
endif ()

# 1. Locate GTest
find_package(GTest CONFIG REQUIRED)

# 2. Link tests against GTest AND main app's core library
target_link_libraries(${TARGET_NAME} PRIVATE
        GTest::gtest
        GTest::gtest_main
        GameWatch.Client.Cli.Core
)

# 3. Discover and register tests
include(GoogleTest)
gtest_discover_tests(${TARGET_NAME})