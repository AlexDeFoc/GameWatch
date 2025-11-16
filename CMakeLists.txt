# NOTE: Change this to your values
set(CMAKE_TOOLCHAIN_FILE "/opt/vcpkg/scripts/buildsystems/vcpkg.cmake" CACHE STRING "")
set(VCPKG_TARGET_TRIPLET "x64-linux" CACHE STRING "")

cmake_minimum_required(VERSION 4.2.0)
project(GameWatch LANGUAGES CXX)
    
set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

# Main executable target
add_executable(GameWatch_ConsoleApp)

target_sources(GameWatch_ConsoleApp
    PRIVATE
    "${CMAKE_CURRENT_SOURCE_DIR}/source_code/sources/file_management.cpp"
    "${CMAKE_CURRENT_SOURCE_DIR}/source_code/sources/console.cpp"
    "${CMAKE_CURRENT_SOURCE_DIR}/source_code/sources/main.cpp"
    "${CMAKE_CURRENT_SOURCE_DIR}/source_code/sources/package.cpp"
    "${CMAKE_CURRENT_SOURCE_DIR}/source_code/sources/menu_helpers.cpp"
    "${CMAKE_CURRENT_SOURCE_DIR}/source_code/sources/menus.cpp"
    "${CMAKE_CURRENT_SOURCE_DIR}/source_code/sources/options.cpp"
    "${CMAKE_CURRENT_SOURCE_DIR}/source_code/sources/menu_base.cpp"
    "${CMAKE_CURRENT_SOURCE_DIR}/source_code/sources/option_base.cpp"
    "${CMAKE_CURRENT_SOURCE_DIR}/source_code/sources/entry_manager.cpp"
    "${CMAKE_CURRENT_SOURCE_DIR}/source_code/sources/entry.cpp"
    "${CMAKE_CURRENT_SOURCE_DIR}/source_code/sources/config.cpp"
    "${CMAKE_CURRENT_SOURCE_DIR}/source_code/sources/internet_management.cpp"
)

target_include_directories(GameWatch_ConsoleApp PRIVATE "${CMAKE_CURRENT_SOURCE_DIR}/source_code/headers")

set_target_properties(GameWatch_ConsoleApp PROPERTIES
    CXX_STANDARD 23
    CXX_STANDARD_REQUIRED ON
    CXX_EXTENSIONS OFF)

if (CMAKE_CXX_COMPILER_ID STREQUAL "Clang")
    if (CMAKE_CXX_COMPILER_FRONTEND_VARIANT STREQUAL "MSVC")
        target_compile_options(GameWatch_ConsoleApp PRIVATE
            "/W4"
            "/permissive-"
            "/WX")
    elseif (CMAKE_CXX_COMPILER_FRONTEND_VARIANT STREQUAL "GNU")
        target_compile_options(GameWatch_ConsoleApp PRIVATE
            "-Wall"
            "-Wextra"
            "-Wpedantic"
            "-Werror")
    endif()
elseif (CMAKE_CXX_COMPILER_ID STREQUAL "MSVC")
    target_compile_options(GameWatch_ConsoleApp PRIVATE
        "/W4"
        "/permissive-"
        "/WX")
elseif (CMAKE_CXX_COMPILER_ID STREQUAL "GNU")
    target_compile_options(GameWatch_ConsoleApp PRIVATE
        "-Wall"
        "-Wextra"
        "-Wpedantic"
        "-Werror")
endif()

# Dependencies
# Boost settings
#set(Boost_USE_DEBUG_LIBS        OFF)  # ignore debug libs and
#set(Boost_USE_RELEASE_LIBS       ON)  # only find release libs

find_package(boost_asio CONFIG REQUIRED)
target_link_libraries(GameWatch_ConsoleApp PRIVATE Boost::asio)

find_package(boost_asio_core CONFIG REQUIRED)
target_link_libraries(GameWatch_ConsoleApp PRIVATE Boost::asio_core)

find_package(boost_asio_deadline_timer CONFIG REQUIRED)
target_link_libraries(GameWatch_ConsoleApp PRIVATE Boost::asio_deadline_timer)

find_package(boost_asio_spawn CONFIG REQUIRED)
target_link_libraries(GameWatch_ConsoleApp PRIVATE Boost::asio_spawn)

find_package(Boost REQUIRED COMPONENTS beast)
target_link_libraries(GameWatch_ConsoleApp PRIVATE Boost::beast)

find_package( REQUIRED CONFIG)
target_link_libraries(GameWatch_ConsoleApp PRIVATE Boost::beast)

find_package(Boost REQUIRED COMPONENTS json)
target_link_libraries(GameWatch_ConsoleApp PRIVATE Boost::json)

find_package( REQUIRED CONFIG)
target_link_libraries(GameWatch_ConsoleApp PRIVATE Boost::json)

find_package(OpenSSL REQUIRED)
target_link_libraries(GameWatch_ConsoleApp PRIVATE OpenSSL::SSL)
target_link_libraries(GameWatch_ConsoleApp PRIVATE OpenSSL::Crypto)

