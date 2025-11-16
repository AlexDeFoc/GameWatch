#pragma once

#include <string>

#if defined(_WIN32) || defined(_WIN64)
#include <Windows.h>
#endif

namespace gw {
class Console {
  private:
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
    //! @brief
    //! Used to determine if the console can for e.g. clear the console screen
    //! with os dependent tools
    bool m_configured_for_custom_actions;

    bool m_unicode_ready;

  private:
    auto configureForNumberInput() noexcept -> bool;
    auto configureForTextInput() noexcept -> bool;

  public:
    Console() noexcept;
    ~Console();

    auto clear() const noexcept -> void;
    auto getAnyKeyPress() const noexcept -> void;
};
} // namespace gw