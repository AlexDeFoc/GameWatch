cmake_minimum_required(VERSION 3.16)
project(GameWatch LANGUAGES CXX)

# -------------------------------------------------------------------
# 1. CONFIGURE ALL TARGETS TO LINK WITH THE C++ RUNTIME STATICALLY
# -------------------------------------------------------------------

if(MSVC)
    set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>" CACHE STRING "" FORCE)
    set(CURL_STATIC_CRT ON CACHE BOOL "" FORCE)
    # Not setting static CRT on Linux/macOS – default dynamic is fine
endif()

# -------------------------------------------------------------------
# 1. FETCH THIRD-PARTY LIBRARIES
# -------------------------------------------------------------------
include(FetchContent)

# CPR + its internal curl
set(CPR_ENABLE_SSL         ON  CACHE INTERNAL "")
set(CPR_USE_SYSTEM_CURL    OFF CACHE INTERNAL "")
set(BUILD_SHARED_LIBS      OFF CACHE INTERNAL "")
set(CPR_ENABLE_INSTALL     OFF CACHE INTERNAL "")
set(CPR_CURL_USE_LIBPSL    OFF CACHE INTERNAL "")   # avoid Meson
set(CPR_USE_SYSTEM_LIB_PSL OFF CACHE INTERNAL "")

FetchContent_Declare(
    cpr
    GIT_REPOSITORY https://github.com/libcpr/cpr.git
    GIT_TAG        1.14.2
)

# Glaze (header-only JSON library)
FetchContent_Declare(
    glaze
    GIT_REPOSITORY https://github.com/stephenberry/glaze.git
    GIT_TAG        v7.5.0
)

# ---- Temporarily silence all third‑party warnings ----
set(_orig_cflags   "${CMAKE_C_FLAGS}")
set(_orig_cxxflags "${CMAKE_CXX_FLAGS}")
if(MSVC)
    set(CMAKE_C_FLAGS   "${CMAKE_C_FLAGS} /W0")
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /W0")
else()
    set(CMAKE_C_FLAGS   "${CMAKE_C_FLAGS} -w")
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -w")
endif()

FetchContent_MakeAvailable(cpr glaze)   # both together, both quiet

set(CMAKE_C_FLAGS   "${_orig_cflags}")
set(CMAKE_CXX_FLAGS "${_orig_cxxflags}")

# -------------------------------------------------------------------
# 2. GAMEWATCH EXECUTABLE
# -------------------------------------------------------------------
add_executable(GameWatchCon)

set_target_properties(GameWatchCon PROPERTIES
    CXX_STANDARD          23
    CXX_STANDARD_REQUIRED ON
    CXX_EXTENSIONS        OFF
)

target_precompile_headers(GameWatchCon PRIVATE "Code/Includes/Pch.hpp")
file(GLOB_RECURSE GW_CPP_SOURCES CONFIGURE_DEPENDS
     "${CMAKE_CURRENT_SOURCE_DIR}/Code/Sources/*.cpp")
target_sources(GameWatchCon PRIVATE ${GW_CPP_SOURCES})
target_include_directories(GameWatchCon PRIVATE Code/Includes)

# -------------------------------------------------------------------
# 3. COMPILER OPTIONS (your code only)
# -------------------------------------------------------------------
if(MSVC AND NOT CMAKE_CXX_COMPILER_ID STREQUAL "Clang")
    target_compile_options(GameWatchCon PRIVATE /W4 /WX /permissive-)
    #target_compile_options(GameWatchCon PRIVATE /Wall /WX /permissive- /analyze-)
    #target_compile_options(GameWatchCon PRIVATE
    #    /wd4820 /wd4061 /wd4710 /wd4711 /wd5045
    #)
elseif(CMAKE_CXX_COMPILER_ID STREQUAL "Clang" AND MSVC)
    target_compile_options(GameWatchCon PRIVATE /W4 /WX)# -Wdeprecated -Wunsafe-buffer-usage)
    #target_compile_options(GameWatchCon PRIVATE -Wall -Wextra -Wpedantic -Werror -Wdeprecated -Wunsafe-buffer-usage)
    #target_compile_options(GameWatchCon PRIVATE
    #    -Wno-unused-command-line-argument
    #    -Wno-c++98-compat
    #    -Wno-header-hygiene
    #    -Wno-documentation
    #    -Wno-newline-eof
    #    -Wno-c++98-compat-pedantic
    #    -Wno-padded
    #    -Wno-covered-switch-default
    #    -Wno-switch-enum
    #)
elseif(CMAKE_CXX_COMPILER_ID STREQUAL "Clang")
    target_compile_options(GameWatchCon PRIVATE -Wall -Wextra -Wpedantic -Werror)# -Wdeprecated -fsafe-buffer-usage-suggestions -Wunsafe-buffer-usage -fcolor-diagnostics)
elseif(CMAKE_CXX_COMPILER_ID STREQUAL "GNU")
    target_compile_options(GameWatchCon PRIVATE -Wall -Wextra -Wpedantic -Werror)# -Wdeprecated -fsafe-buffer-usage-suggestions -Wunsafe-buffer-usage)
endif()

# -------------------------------------------------------------------
# 4. LINK
# -------------------------------------------------------------------
target_link_libraries(GameWatchCon PRIVATE cpr::cpr glaze::glaze)