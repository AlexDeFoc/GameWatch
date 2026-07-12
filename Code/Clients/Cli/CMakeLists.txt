cmake_minimum_required(VERSION 4.2.3)
project(GameWatch.Client.Cli LANGUAGES CXX)
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

set(LIB_TARGET "GameWatch.Client.Cli.Core")
set(EXE_TARGET "GameWatch.Client.Cli")

# 1. Build the core logic as a static library
add_library(${LIB_TARGET} STATIC)
set_target_properties(${LIB_TARGET} PROPERTIES
        CXX_STANDARD 23
        CXX_STANDARD_REQUIRED ON
        CXX_EXTENSIONS OFF
)

target_include_directories(${LIB_TARGET} PUBLIC "${CMAKE_CURRENT_SOURCE_DIR}/Includes")
# Add all actual business logic cpp files here, but NOT main.cpp
target_sources(${LIB_TARGET} PRIVATE "${CMAKE_CURRENT_SOURCE_DIR}/Sources/ExampleLogic/a.cpp")

# 2. Build the main executable
add_executable(${EXE_TARGET})
set_target_properties(${EXE_TARGET} PROPERTIES
        CXX_STANDARD 23
        CXX_STANDARD_REQUIRED ON
        CXX_EXTENSIONS OFF
)
# The executable only needs main.cpp
target_sources(${EXE_TARGET} PRIVATE "${CMAKE_CURRENT_SOURCE_DIR}/Sources/AppEntry/main.cpp")
target_link_libraries(${EXE_TARGET} PRIVATE ${LIB_TARGET})

# --- COMPILER FLAGS ---
# Apply your flags to the LIB_TARGET (which the EXE will inherit where appropriate)
if (CMAKE_HOST_SYSTEM_NAME STREQUAL "Windows" AND CMAKE_CXX_COMPILER_FRONTEND_VARIANT STREQUAL "MSVC")
    if (CMAKE_CXX_COMPILER_ID STREQUAL "MSVC")
        target_compile_options(${LIB_TARGET} PRIVATE
                /permissive- /Zc:__cplusplus /Zc:preprocessor /Wall /WX /utf-8 /diagnostics:caret
                /GL /external:W0 /external:anglebrackets /wd4514 /wd4710 /wd4711 /wd4820 /wd5045
        )
        target_link_options(${LIB_TARGET} PRIVATE /LTCG)
    elseif (CMAKE_CXX_COMPILER_ID STREQUAL "Clang")
        target_compile_options(${LIB_TARGET} PRIVATE
                /permissive- /Zc:__cplusplus /Wall /WX /utf-8 /diagnostics:caret /external:W0
                -Wno-c++98-compat -Wno-newline-eof
        )
        target_link_options(${LIB_TARGET} PRIVATE /LTCG)
    endif ()
elseif (CMAKE_HOST_SYSTEM_NAME STREQUAL "Linux" AND CMAKE_CXX_COMPILER_FRONTEND_VARIANT STREQUAL "GNU")
    if (CMAKE_CXX_COMPILER_ID STREQUAL "Clang")
        target_compile_options(${LIB_TARGET} PRIVATE
                -Wall -Wextra -Werror -fdiagnostics-color=always -flto
                -Wno-newline-eof -Wno-padded
        )
        target_link_options(${LIB_TARGET} PUBLIC -flto)
    elseif (CMAKE_CXX_COMPILER_ID STREQUAL "GNU")
        target_compile_options(${LIB_TARGET} PRIVATE
                -Wall -Wextra -Werror -fdiagnostics-color=always -flto
                -Wno-padded -Wno-inline
        )
        target_link_options(${LIB_TARGET} PUBLIC -flto)
    endif ()
endif ()

# --- CLANG-TIDY INTEGRATION ---
find_program(CLANG_TIDY_EXE NAMES "clang-tidy")
if (CLANG_TIDY_EXE)
    message(STATUS "Found clang-tidy: ${CLANG_TIDY_EXE}")

    # Apply to the core library
    set_target_properties(${LIB_TARGET} PROPERTIES
            CXX_CLANG_TIDY "${CLANG_TIDY_EXE};--config-file=${CMAKE_CURRENT_SOURCE_DIR}/.clang-tidy"
    )

    # Apply to the main executable
    set_target_properties(${EXE_TARGET} PROPERTIES
            CXX_CLANG_TIDY "${CLANG_TIDY_EXE};--config-file=${CMAKE_CURRENT_SOURCE_DIR}/.clang-tidy"
    )
else ()
    message(WARNING "clang-tidy executable was NOT found on this system!")
endif ()

# Coverage Flags
# Coverage Flags
if (COMPILER_COVERAGE_FLAGS)
    separate_arguments(COMPILER_ARGS_LIST NATIVE_COMMAND "${COMPILER_COVERAGE_FLAGS}")
    target_compile_options(${LIB_TARGET} PRIVATE ${COMPILER_ARGS_LIST})
    target_compile_options(${EXE_TARGET} PRIVATE ${COMPILER_ARGS_LIST})
endif ()
if (LINKER_COVERAGE_FLAGS)
    separate_arguments(LINKER_ARGS_LIST NATIVE_COMMAND "${LINKER_COVERAGE_FLAGS}")
    target_link_options(${LIB_TARGET} PRIVATE ${LINKER_ARGS_LIST})
    target_link_options(${EXE_TARGET} PRIVATE ${LINKER_ARGS_LIST})
endif ()

# Enable testing and add the subdirectory
enable_testing()
add_subdirectory(Tests)