cmake_minimum_required(VERSION 4.2.3)

project(GameWatch.Client.Cli.Tests LANGUAGES CXX)

set(TARGET_NAME "GameWatch.Client.Cli.Tests")
add_executable(${TARGET_NAME})

set_target_properties(${TARGET_NAME} PROPERTIES
	CXX_STANDARD 23
	CXX_STANDARD_REQUIRED ON
	CXX_EXTENSIONS OFF
	CXX_SCAN_FOR_MODULES OFF
)

target_include_directories(${TARGET_NAME} PRIVATE "${CMAKE_CURRENT_SOURCE_DIR}/include")

# Test-specific sources
target_sources(${TARGET_NAME} PRIVATE "${CMAKE_CURRENT_SOURCE_DIR}/src/lang_mng.cpp")

# Precompile headers
target_precompile_headers(${TARGET_NAME} PRIVATE "${CMAKE_CURRENT_SOURCE_DIR}/include/pch.hpp")

# --- COMPILER & LINKER FLAGS ---
if (CMAKE_HOST_SYSTEM_NAME STREQUAL "Windows" AND CMAKE_CXX_COMPILER_FRONTEND_VARIANT STREQUAL "MSVC")
	if (CMAKE_CXX_COMPILER_ID STREQUAL "MSVC")
		target_compile_options(${TARGET_NAME} PRIVATE
			"$<$<CONFIG:Release>:/GL;/Oi;/Ot;/O2;/w14710;/wd4711>"
			"$<$<CONFIG:Debug>:/Od;/RTC1;/Zc:inline>"

			"/permissive-" "/Zc:__cplusplus" "/Zc:preprocessor" "/Wall" "/WX" "/utf-8" "/diagnostics:caret"
			"/w15038" "/w14668" "/w14244" "/w14267" "/w14389" "/w14456" "/w14457" "/w14458" "/w14459"
		)

		target_link_options(${TARGET_NAME} PRIVATE "$<$<CONFIG:Release>:/LTCG>")
	elseif (CMAKE_CXX_COMPILER_ID STREQUAL "Clang")
		target_compile_options(${TARGET_NAME} PRIVATE
			"$<$<CONFIG:Release>:/O2;-Wdouble-promotion;-Winline>"
			"$<$<CONFIG:Debug>:/Od;/Zi;/RTC1>"

			"/permissive-" "/Zc:__cplusplus" "/Wall" "/WX" "/utf-8" "/diagnostics:caret" "-Wundef" "-Wshadow"
			"-Wunused-macros" "-Wfloat-equal" "-Wformat=2" "-Wformat-security" "-Wformat-nonliteral"
			"-Wcast-qual" "-Wcast-align" "-Wpacked" "-Woverloaded-virtual" "-Wzero-as-null-pointer-constant"
			"-Wvirtual-move-assign" "-Wpessimizing-move" "-Wredundant-move" "-Werror=conversion"
			"-Werror=sign-conversion" "-Werror=float-conversion" "-Wno-unused-function" "-Wno-pre-c++17-compat"
			"-Wno-c++98-compat" "-Wno-newline-eof" "-Wno-c++98-compat-pedantic" "-Wno-global-constructors"
		)

		target_link_options(${TARGET_NAME} PRIVATE "$<$<CONFIG:Release>:-flto>")
	endif ()
elseif (CMAKE_HOST_SYSTEM_NAME STREQUAL "Linux" AND CMAKE_CXX_COMPILER_FRONTEND_VARIANT STREQUAL "GNU")
	if (CMAKE_CXX_COMPILER_ID STREQUAL "Clang")
		target_compile_options(${TARGET_NAME} PRIVATE
			"$<$<CONFIG:Release>:-O3;-flto;-Wdouble-promotion;-Winline;-Wdisabled-optimization>"
			"$<$<CONFIG:Debug>:-Og;-g3>"

			"-Wall" "-Wextra" "-Wpedantic" "-Werror" "-Wunused-command-line-argument" "-Wno-unused-function" "-Wundef"
			"-Wshadow" "-Wunused-macros" "-Wfloat-equal" "-Wformat=2" "-Wformat-security" "-Wformat-nonliteral"
			"-Wcast-qual" "-Wcast-align" "-Wpacked" "-Woverloaded-virtual" "-Wzero-as-null-pointer-constant"
			"-Wpessimizing-move" "-Wredundant-move" "-Werror=conversion"
			"-Werror=sign-conversion" "-Werror=float-conversion"
		)

		target_link_options(${TARGET_NAME} PRIVATE "$<$<CONFIG:Release>:-flto>")
	elseif (CMAKE_CXX_COMPILER_ID STREQUAL "GNU")
		target_compile_options(${TARGET_NAME} PRIVATE
			"$<$<CONFIG:Release>:-O3;-flto;-Wdouble-promotion;-Winline;-Wdisabled-optimization>"
			"$<$<CONFIG:Debug>:-Og;-g3>"

			"-Wall" "-Wextra" "-Wpedantic" "-Werror" "-Wno-unused-function" "-Wundef" "-Wshadow" "-Wunused-macros"
			"-Wfloat-equal" "-Wformat=2" "-Wformat-security" "-Wformat-nonliteral" "-Wcast-qual" "-Wcast-align" "-Wpacked"
			"-Woverloaded-virtual" "-Wzero-as-null-pointer-constant" "-Wpessimizing-move" "-Wredundant-move"
			"-Werror=conversion" "-Werror=sign-conversion" "-Werror=float-conversion"
		)

		target_link_options(${TARGET_NAME} PRIVATE "$<$<CONFIG:Release>:-flto>")
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
