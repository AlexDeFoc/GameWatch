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
	CXX_SCAN_FOR_MODULES OFF
)

target_include_directories(${LIB_TARGET} PUBLIC "${CMAKE_CURRENT_SOURCE_DIR}/include")
# Add all actual business logic cpp files here, but NOT main.cpp
target_sources(${LIB_TARGET} PRIVATE
	"${CMAKE_CURRENT_SOURCE_DIR}/src/dir.cpp"
	"${CMAKE_CURRENT_SOURCE_DIR}/src/file.cpp"
	"${CMAKE_CURRENT_SOURCE_DIR}/src/file_sys.cpp"
	"${CMAKE_CURRENT_SOURCE_DIR}/src/lang_mng.cpp"
)

# 2. Build the main executable
add_executable(${EXE_TARGET})
set_target_properties(${EXE_TARGET} PROPERTIES
	CXX_STANDARD 23
	CXX_STANDARD_REQUIRED ON
	CXX_EXTENSIONS OFF
	CXX_SCAN_FOR_MODULES OFF
)
# The executable only needs main.cpp
target_sources(${EXE_TARGET} PRIVATE "${CMAKE_CURRENT_SOURCE_DIR}/src/main.cpp")
target_link_libraries(${EXE_TARGET} PRIVATE ${LIB_TARGET})

# Precompile headers
target_precompile_headers(${LIB_TARGET} PRIVATE "${CMAKE_CURRENT_SOURCE_DIR}/include/pch.hpp")
target_precompile_headers(${EXE_TARGET} PRIVATE "${CMAKE_CURRENT_SOURCE_DIR}/include/pch.hpp")

# --- COMPILER & LINKER FLAGS ---
if (CMAKE_HOST_SYSTEM_NAME STREQUAL "Windows" AND CMAKE_CXX_COMPILER_FRONTEND_VARIANT STREQUAL "MSVC")
	if (CMAKE_CXX_COMPILER_ID STREQUAL "MSVC")
		target_compile_options(${LIB_TARGET} PRIVATE
			"$<$<CONFIG:Release>:/GL;/Oi;/Ot;/O2;/w14710;/wd4711>"
			"$<$<CONFIG:Debug>:/Od;/RTC1;/Zc:inline>"

			"/permissive-" "/Zc:__cplusplus" "/Zc:preprocessor" "/Wall" "/WX" "/utf-8" "/diagnostics:caret"
			"/w15038" "/w14668" "/w14244" "/w14267" "/w14389" "/w14456" "/w14457" "/w14458" "/w14459"
		)

		target_compile_options(${EXE_TARGET} PRIVATE
			"$<$<CONFIG:Release>:/GL;/Oi;/Ot;/O2;/w14710;/wd4711>"
			"$<$<CONFIG:Debug>:/Od;/RTC1;/Zc:inline>"

			"/permissive-" "/Zc:__cplusplus" "/Zc:preprocessor" "/Wall" "/WX" "/utf-8" "/diagnostics:caret"
			"/w15038" "/w14668" "/w14244" "/w14267" "/w14389" "/w14456" "/w14457" "/w14458" "/w14459"
		)

		target_link_options(${LIB_TARGET} PRIVATE "$<$<CONFIG:Release>:/LTCG>")
		target_link_options(${EXE_TARGET} PRIVATE "$<$<CONFIG:Release>:/LTCG>")
	elseif (CMAKE_CXX_COMPILER_ID STREQUAL "Clang")
		target_compile_options(${LIB_TARGET} PRIVATE
			"$<$<CONFIG:Release>:/O2;-Wdouble-promotion;-Winline>"
			"$<$<CONFIG:Debug>:/Od;/Zi;/RTC1>"

			"/permissive-" "/Zc:__cplusplus" "/Wall" "/WX" "/utf-8" "/diagnostics:caret" "-Wundef" "-Wshadow"
			"-Wunused-macros" "-Wfloat-equal" "-Wformat=2" "-Wformat-security" "-Wformat-nonliteral"
			"-Wcast-qual" "-Wcast-align" "-Wpacked" "-Woverloaded-virtual" "-Wzero-as-null-pointer-constant"
			"-Wvirtual-move-assign" "-Wpessimizing-move" "-Wredundant-move" "-Werror=conversion"
			"-Werror=sign-conversion" "-Werror=float-conversion" "-Wno-unused-function" "-Wno-pre-c++17-compat"
			"-Wno-c++98-compat" "-Wno-newline-eof"
		)

		target_compile_options(${EXE_TARGET} PRIVATE
			"$<$<CONFIG:Release>:/O2;-Wdouble-promotion;-Winline>"
			"$<$<CONFIG:Debug>:/Od;/Zi;/RTC1>"

			"/permissive-" "/Zc:__cplusplus" "/Wall" "/WX" "/utf-8" "/diagnostics:caret" "-Wundef" "-Wshadow"
			"-Wunused-macros" "-Wfloat-equal" "-Wformat=2" "-Wformat-security" "-Wformat-nonliteral"
			"-Wcast-qual" "-Wcast-align" "-Wpacked" "-Woverloaded-virtual" "-Wzero-as-null-pointer-constant"
			"-Wvirtual-move-assign" "-Wpessimizing-move" "-Wredundant-move" "-Werror=conversion"
			"-Werror=sign-conversion" "-Werror=float-conversion" "-Wno-unused-function" "-Wno-pre-c++17-compat"
			"-Wno-c++98-compat" "-Wno-newline-eof"
		)

		target_link_options(${LIB_TARGET} PRIVATE "$<$<CONFIG:Release>:-flto>")
		target_link_options(${EXE_TARGET} PRIVATE "$<$<CONFIG:Release>:-flto>")
	endif ()
elseif (CMAKE_HOST_SYSTEM_NAME STREQUAL "Linux" AND CMAKE_CXX_COMPILER_FRONTEND_VARIANT STREQUAL "GNU")
	if (CMAKE_CXX_COMPILER_ID STREQUAL "Clang")
		target_compile_options(${LIB_TARGET} PRIVATE
			"$<$<CONFIG:Release>:-O3;-flto;-Wdouble-promotion;-Winline;-Wdisabled-optimization>"
			"$<$<CONFIG:Debug>:-Og;-g3>"

			"-Wall" "-Wextra" "-Wpedantic" "-Werror" "-Wunused-command-line-argument" "-Wno-unused-function" "-Wundef"
			"-Wshadow" "-Wunused-macros" "-Wfloat-equal" "-Wformat=2" "-Wformat-security" "-Wformat-nonliteral"
			"-Wcast-qual" "-Wcast-align" "-Wpacked" "-Woverloaded-virtual" "-Wzero-as-null-pointer-constant"
			"-Wpessimizing-move" "-Wredundant-move" "-Werror=conversion" "-Werror=sign-conversion" "-Werror=float-conversion"
			"-fvisibility=hidden"
		)

		target_compile_options(${EXE_TARGET} PRIVATE
			"$<$<CONFIG:Release>:-O3;-flto;-Wdouble-promotion;-Winline;-Wdisabled-optimization>"
			"$<$<CONFIG:Debug>:-Og;-g3>"

			"-Wall" "-Wextra" "-Wpedantic" "-Werror" "-Wunused-command-line-argument" "-Wno-unused-function" "-Wundef"
			"-Wshadow" "-Wunused-macros" "-Wfloat-equal" "-Wformat=2" "-Wformat-security" "-Wformat-nonliteral"
			"-Wcast-qual" "-Wcast-align" "-Wpacked" "-Woverloaded-virtual" "-Wzero-as-null-pointer-constant"
			"-Wpessimizing-move" "-Wredundant-move" "-Werror=conversion" "-Werror=sign-conversion" "-Werror=float-conversion"
		)

		target_link_options(${LIB_TARGET} PRIVATE "$<$<CONFIG:Release>:-flto>")
		target_link_options(${EXE_TARGET} PRIVATE "$<$<CONFIG:Release>:-flto>")
	elseif (CMAKE_CXX_COMPILER_ID STREQUAL "GNU")
		target_compile_options(${LIB_TARGET} PRIVATE
			"$<$<CONFIG:Release>:-O3;-flto;-Wdouble-promotion;-Winline;-Wdisabled-optimization>"
			"$<$<CONFIG:Debug>:-Og;-g3>"

			"-Wall" "-Wextra" "-Wpedantic" "-Werror" "-Wno-unused-function" "-Wundef" "-Wshadow" "-Wunused-macros"
			"-Wfloat-equal" "-Wformat=2" "-Wformat-security" "-Wformat-nonliteral" "-Wcast-qual" "-Wcast-align" "-Wpacked"
			"-Woverloaded-virtual" "-Wzero-as-null-pointer-constant" "-Wpessimizing-move" "-Wredundant-move"
			"-Werror=conversion" "-Werror=sign-conversion" "-Werror=float-conversion" "-fvisibility=hidden"
		)

		target_compile_options(${EXE_TARGET} PRIVATE
			"$<$<CONFIG:Release>:-O3;-flto;-Wdouble-promotion;-Winline;-Wdisabled-optimization>"
			"$<$<CONFIG:Debug>:-Og;-g3>"

			"-Wall" "-Wextra" "-Wpedantic" "-Werror" "-Wno-unused-function" "-Wundef" "-Wshadow" "-Wunused-macros"
			"-Wfloat-equal" "-Wformat=2" "-Wformat-security" "-Wformat-nonliteral" "-Wcast-qual" "-Wcast-align" "-Wpacked"
			"-Woverloaded-virtual" "-Wzero-as-null-pointer-constant" "-Wpessimizing-move" "-Wredundant-move"
			"-Werror=conversion" "-Werror=sign-conversion" "-Werror=float-conversion"
		)

		target_link_options(${LIB_TARGET} PRIVATE "$<$<CONFIG:Release>:-flto>")
		target_link_options(${EXE_TARGET} PRIVATE "$<$<CONFIG:Release>:-flto>")
	endif ()
endif ()

# --- CLANG-TIDY INTEGRATION ---
find_program(CLANG_TIDY_EXE NAMES "clang-tidy")
if (CLANG_TIDY_EXE)
	message(STATUS "Found clang-tidy: ${CLANG_TIDY_EXE}")

	# 1. Initialize the command list variable
	set(CLANG_TIDY_CMD "${CLANG_TIDY_EXE}" "--config-file=${CMAKE_CURRENT_SOURCE_DIR}/.clang-tidy")

	# 2. Append the flag to the variable if we are using GCC
	if (CMAKE_CXX_COMPILER_ID STREQUAL "GNU")
		list(APPEND CLANG_TIDY_CMD "--extra-arg=-Wno-ignored-gch")
	endif ()

	# 3. Pass the actual variable to the targets
	set_target_properties(${LIB_TARGET} PROPERTIES
		CXX_CLANG_TIDY "${CLANG_TIDY_CMD}"
	)

	set_target_properties(${EXE_TARGET} PROPERTIES
		CXX_CLANG_TIDY "${CLANG_TIDY_CMD}"
	)
else ()
	message(WARNING "clang-tidy executable was NOT found on this system!")
endif ()

# Coverage Flags
if (GW_CLIENT_CLI_COVERING_CODE_THIS_BUILD)
	separate_arguments(COMPILER_ARGS_LIST NATIVE_COMMAND "${GW_CLIENT_CLI_COMPILER_COVERAGE_FLAGS}")
	separate_arguments(LINKER_ARGS_LIST NATIVE_COMMAND "${GW_CLIENT_CLI_LINKER_COVERAGE_FLAGS}")

	target_compile_options(${LIB_TARGET} PRIVATE ${COMPILER_ARGS_LIST})
	target_compile_options(${EXE_TARGET} PRIVATE ${COMPILER_ARGS_LIST})

	target_link_options(${LIB_TARGET} PRIVATE ${LINKER_ARGS_LIST})
	target_link_options(${EXE_TARGET} PRIVATE ${LINKER_ARGS_LIST})
endif ()

# Enable testing and add the subdirectory
enable_testing()
add_subdirectory(tests)
