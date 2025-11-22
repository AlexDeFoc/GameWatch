#include "console.hpp"

#include <cmath>
#include <iostream>
#include <print>

#if defined(__linux__) || defined(__APPLE__)
#include <unistd.h>
#include <termios.h>
#endif

using namespace gw;

Console::Console() noexcept
    : m_configured_for_custom_actions{false}
    , m_unicode_ready{false}
#if defined(_WIN32) || defined(_WIN64)
    , m_stdin_handle{nullptr}
    , m_stdout_handle{nullptr}
    , m_stdin_bak_mode{0}
    , m_stdout_bak_mode{0}
    , m_stdin_current_mode{0}
    , m_stdout_current_mode{0}
    , m_stdin_bak_code_page{0}
    , m_stdout_bak_code_page{0}
#endif
{
#if defined(_WIN32) || defined(_WIN64)
    bool continue_configuring{true};

    m_stdin_handle = ::GetStdHandle(STD_INPUT_HANDLE);

    if (m_stdin_handle == INVALID_HANDLE_VALUE)
        continue_configuring = false;

    if (continue_configuring) {
        m_stdout_handle = ::GetStdHandle(STD_OUTPUT_HANDLE);

        if (m_stdout_handle == INVALID_HANDLE_VALUE)
            continue_configuring = false;
    }

    if (continue_configuring) {
        BOOL success = ::GetConsoleMode(m_stdin_handle, &m_stdin_bak_mode);
        if (!success)
            continue_configuring = false;
    }

    if (continue_configuring) {
        BOOL success = ::GetConsoleMode(m_stdout_handle, &m_stdout_bak_mode);
        if (!success)
            continue_configuring = false;
    }

    if (continue_configuring) {
        m_stdin_current_mode = m_stdin_bak_mode | ENABLE_PROCESSED_INPUT;

        BOOL success = ::SetConsoleMode(m_stdin_handle, m_stdin_current_mode);
        if (!success) {
            continue_configuring = false;
            m_stdin_current_mode = m_stdin_bak_mode;
        }
    }

    if (continue_configuring) {
        m_stdout_current_mode =
            m_stdout_bak_mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING;

        BOOL success = ::SetConsoleMode(m_stdout_handle, m_stdout_current_mode);
        if (!success) {
            continue_configuring = false;
            m_stdout_current_mode = m_stdout_bak_mode;
            m_stdin_current_mode = m_stdin_bak_mode;

            ::SetConsoleMode(m_stdin_handle, m_stdin_bak_mode);
        }
    }

    if (continue_configuring)
        m_configured_for_custom_actions = true;

    if (continue_configuring) {
        m_stdin_bak_code_page = ::GetConsoleCP();
        m_stdout_bak_code_page = ::GetConsoleOutputCP();

        if (m_stdin_bak_code_page == 0 || m_stdout_bak_code_page == 0)
            continue_configuring = false;
    }

    if (continue_configuring) {
        BOOL success = ::SetConsoleCP(CP_UTF8);

        if (!success) {
            continue_configuring = false;
            ::SetConsoleCP(m_stdin_bak_code_page);
        }
    }

    if (continue_configuring) {
        BOOL success = ::SetConsoleOutputCP(CP_UTF8);

        if (!success) {
            continue_configuring = false;
            ::SetConsoleOutputCP(m_stdout_bak_code_page);
            ::SetConsoleCP(m_stdin_bak_code_page);
        }
    }

    if (continue_configuring)
        m_unicode_ready = true;
#else
    bool continue_configuring{true};

    if (isatty(fileno(stdout)))
        continue_configuring = true;

    if (continue_configuring)
        m_configured_for_custom_actions = true;
#endif
}

Console::~Console() {
#if defined(_WIN32) || defined(_WIN64)
    ::SetConsoleMode(m_stdin_handle, m_stdin_bak_mode);
    ::SetConsoleMode(m_stdout_handle, m_stdout_bak_mode);
    ::SetConsoleCP(m_stdin_bak_code_page);
    ::SetConsoleOutputCP(m_stdout_bak_code_page);
#endif
}

auto Console::clear() const noexcept -> void {
    if (m_configured_for_custom_actions) {
#if defined(_WIN32) || defined(_WIN64)
        // Clear screen
        std::print("\x1b[2J");

        // Clear scrolloff
        std::print("\x1b[3J");

        // Position cursor at 1;1
        std::print("\x1b[H");
#else
        // Clear screen
        std::print("\x1b[2J");

        // Clear scrolloff
        std::print("\x1b[3J");

        // Position cursor at 1;1
        std::print("\x1b[H");
#endif
    } else {
        std::print("\n\n\nClearing screen...\n\n\n");
    }
}

auto Console::getAnyKeyPress() const noexcept -> void {
    if (m_configured_for_custom_actions) {
#if defined(_WIN32) || defined(_WIN64)
        INPUT_RECORD rec{};
        DWORD records_count{};

        while (true) {
            ::ReadConsoleInput(m_stdin_handle, &rec, 1, &records_count);

            if (rec.EventType == KEY_EVENT && rec.Event.KeyEvent.bKeyDown)
                break;
        }
#else
    termios old_settings{}, new_settings{};

    tcgetattr(STDIN_FILENO, &old_settings); // save terminal settings

    new_settings = old_settings;

    new_settings.c_lflag &= ~(ICANON | ECHO); // disable canonical mode and echo

    tcsetattr(STDIN_FILENO, TCSANOW, &new_settings); // apply new settings

    std::getchar(); // wait for single key press

    tcsetattr(STDIN_FILENO, TCSANOW, &old_settings); // restore terminal settings
#endif
    } else {
        std::string sink{};
        std::getline(std::cin, sink);
    }
}
