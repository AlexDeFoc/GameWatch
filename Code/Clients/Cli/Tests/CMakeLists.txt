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

# Precompile headers
target_precompile_headers(${TARGET_NAME} PRIVATE "${CMAKE_CURRENT_SOURCE_DIR}/Includes/pch.hpp")

# --- COMPILER & LINKER FLAGS ---
if (CMAKE_HOST_SYSTEM_NAME STREQUAL "Windows" AND CMAKE_CXX_COMPILER_FRONTEND_VARIANT STREQUAL "MSVC")
	if (CMAKE_CXX_COMPILER_ID STREQUAL "MSVC")
		target_compile_options(${TARGET_NAME} PRIVATE
			"/permissive-" "/Zc:__cplusplus" "/Zc:preprocessor" "/Wall" "/WX" "/utf-8" "/diagnostics:caret"
		)

		if (GW_CLIENT_CLI_BUILD_MODE)
			if ("${GW_CLIENT_CLI_BUILD_MODE}" STREQUAL "Release")
				target_compile_options(${TARGET_NAME} PRIVATE
					"/GL" "/Oi" "/Ot" "/O2"
				)

				target_link_options(${TARGET_NAME} PRIVATE /LTCG)
			elseif ("${GW_CLIENT_CLI_BUILD_MODE}" STREQUAL "Debug")
				target_compile_options(${TARGET_NAME} PRIVATE
					"/Od" "/RTC1" "/Zc:inline"
				)
			endif ()
		endif ()
	elseif (CMAKE_CXX_COMPILER_ID STREQUAL "Clang")
		target_compile_options(${TARGET_NAME} PRIVATE
			"/permissive-" "/Zc:__cplusplus" "/Wall" "/WX" "/utf-8" "/diagnostics:caret"
		)

		if (GW_CLIENT_CLI_BUILD_MODE)
			if ("${GW_CLIENT_CLI_BUILD_MODE}" STREQUAL "Release")
				target_compile_options(${TARGET_NAME} PRIVATE
					"/O2"
					"-Wno-pre-c++17-compat" "-Wno-c++98-compat" "-Wno-c++98-compat-pedantic" "-Wno-newline-eof"
					"-Wno-global-constructors"
				)

				target_link_options(${TARGET_NAME} PRIVATE "/LTCG")
			elseif ("${GW_CLIENT_CLI_BUILD_MODE}" STREQUAL "Debug")
				target_compile_options(${TARGET_NAME} PRIVATE
					"/Od" "/Zi" "/RTC1"
					"-Wno-pre-c++17-compat" "-Wno-c++98-compat" "-Wno-c++98-compat-pedantic" "-Wno-newline-eof"
					"-Wno-global-constructors"
				)
			endif ()
		endif ()
	endif ()
elseif (CMAKE_HOST_SYSTEM_NAME STREQUAL "Linux" AND CMAKE_CXX_COMPILER_FRONTEND_VARIANT STREQUAL "GNU")
	if (CMAKE_CXX_COMPILER_ID STREQUAL "Clang")
		target_compile_options(${TARGET_NAME} PRIVATE
			"-Wall" "-Wextra" "-Werror" "-fdiagnostics-color=always"
		)

		if (GW_CLIENT_CLI_BUILD_MODE)
			if ("${GW_CLIENT_CLI_BUILD_MODE}" STREQUAL "Release")
				target_compile_options(${TARGET_NAME} PRIVATE
					"-flto" "-O3"
				)

				target_link_options(${TARGET_NAME} PRIVATE -flto)
			elseif ("${GW_CLIENT_CLI_BUILD_MODE}" STREQUAL "Debug")
				target_compile_options(${TARGET_NAME} PRIVATE
					"-Og" "-g3"
				)
			endif ()
		endif ()
	elseif (CMAKE_CXX_COMPILER_ID STREQUAL "GNU")
		target_compile_options(${TARGET_NAME} PRIVATE
			"-Wall" "-Wextra" "-Werror" "-fdiagnostics-color=always"
			"-Wno-padded" "-Wno-inline"
		)

		if (GW_CLIENT_CLI_BUILD_MODE)
			if ("${GW_CLIENT_CLI_BUILD_MODE}" STREQUAL "Release")
				target_compile_options(${TARGET_NAME} PRIVATE
					"-flto" "-O3"
				)

				target_link_options(${TARGET_NAME} PRIVATE "-flto")
			elseif ("${GW_CLIENT_CLI_BUILD_MODE}" STREQUAL "Debug")
				target_compile_options(${TARGET_NAME} PRIVATE
					"-Og" "-g3"
				)
			endif ()
		endif ()
	endif ()
endif ()

if (GW_CLIENT_CLI_COVERING_CODE_THIS_BUILD)
	separate_arguments(COMPILER_ARGS_LIST NATIVE_COMMAND "${GW_CLIENT_CLI_COMPILER_COVERAGE_FLAGS}")
	separate_arguments(LINKER_ARGS_LIST NATIVE_COMMAND "${GW_CLIENT_CLI_LINKER_COVERAGE_FLAGS}")

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