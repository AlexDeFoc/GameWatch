#pragma once

#include "print_msgs.hpp"
#include "print_tag.hpp"

#include <print>
#include <string>

#if defined(_WIN32) || defined(_WIN64)
#include <Windows.h>
#endif

namespace gw {
class Console {
  private:
    //! @brief
    //! Used to determine if the console can for e.g. clear the console screen
    //! with os dependent tools
    bool m_configured_for_custom_actions;

    bool m_unicode_ready;

#if defined(_WIN32) || defined(_WIN64)
    HANDLE m_stdin_handle;
    HANDLE m_stdout_handle;
    DWORD m_stdin_bak_mode;
    DWORD m_stdout_bak_mode;
    DWORD m_stdin_current_mode;
    DWORD m_stdout_current_mode;
    UINT m_stdin_bak_code_page;
    UINT m_stdout_bak_code_page;
#endif

  private:
    auto configureForNumberInput() noexcept -> bool;
    auto configureForTextInput() noexcept -> bool;

  public:
    Console() noexcept;
    ~Console();

    auto clear() const noexcept -> void;
    auto getAnyKeyPress() const noexcept -> void;

    template <typename... Args>
    auto print(PrintTag,
               std::format_string<Args...> fmt,
               Args&&... args) const noexcept -> void;

    template <typename... Args>
    auto println(PrintTag,
                 std::format_string<Args...> fmt,
                 Args&&... args) const noexcept -> void;
};
} // namespace gw

// Templates definitions
template <typename... Args>
auto gw::Console::print(gw::PrintTag tag,
                        std::format_string<Args...> fmt,
                        Args&&... args) const noexcept -> void {
    if (m_configured_for_custom_actions) {
        switch (tag) {
        case gw::PrintTag::Error: {
            std::print("{}{}{}: ",
                       gw::error_tag_color,
                       gw::error_tag,
                       gw::reset_color);
            break;
        }

        case gw::PrintTag::Request: {
            std::print("{}{}{}: ",
                       gw::request_tag_color,
                       gw::request_tag,
                       gw::reset_color);
            break;
        }

        case gw::PrintTag::Tip: {
            std::print("{}{}{}: ",
                       gw::tip_tag_color,
                       gw::tip_tag,
                       gw::reset_color);
            break;
        }

        case gw::PrintTag::Info: {
            std::print("{}{}{}: ",
                       gw::info_tag_color,
                       gw::info_tag,
                       gw::reset_color);
            break;
        }
        }
    } else {
        switch (tag) {
        case gw::PrintTag::Error: {
            std::print("{}: ", gw::error_tag);
            break;
        }

        case gw::PrintTag::Request: {
            std::print("{}: ", gw::request_tag);
            break;
        }

        case gw::PrintTag::Tip: {
            std::print("{}: ", gw::tip_tag);
            break;
        }

        case gw::PrintTag::Info: {
            std::print("{}: ", gw::info_tag);
            break;
        }
        }
    }

    std::print(fmt, std::forward<Args>(args)...);
}

template <typename... Args>
auto gw::Console::println(gw::PrintTag tag,
                          std::format_string<Args...> fmt,
                          Args&&... args) const noexcept -> void {
    gw::Console::print(tag, fmt, std::forward<Args>(args)...);
    std::println();
}
