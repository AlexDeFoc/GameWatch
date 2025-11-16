#pragma once

#include "menu_base.hpp"
#include "options.hpp"

namespace gw {
namespace menus {
class Main final : public gw::Menu {
  protected:
    [[nodiscard]] auto getOptionAddr(int opt_id) noexcept
        -> gw::Option* override;

  private:
    gw::opts::ListEntries m_list_entries_opt;
    gw::opts::ToggleEntryClock m_toggle_entry_clock_opt;
    gw::opts::EditEntries m_edit_entries_opt;
    gw::opts::AddNewEntry m_add_new_entry_opt;
    gw::opts::Settings m_settings_opt;
    gw::opts::ExitApp m_exit_app_opt;

  public:
    Main() noexcept;
    auto displayOptions(const gw::Package&) noexcept -> void override final;
};

class EditEntries final : public gw::Menu {
  protected:
    [[nodiscard]] auto getOptionAddr(int opt_id) noexcept
        -> gw::Option* override;

  private:
    gw::opts::ChangeEntryTitle m_change_entry_title_opt;
    gw::opts::ResetEntryClock m_reset_entry_clock_opt;
    gw::opts::DeleteEntry m_delete_entry_opt;
    gw::opts::GoBackToMainMenu m_go_back_opt;

  public:
    EditEntries() noexcept;
    auto displayOptions(const gw::Package&) noexcept -> void override final;
};

class Settings final : public gw::Menu {
  protected:
    [[nodiscard]] auto getOptionAddr(int opt_id) noexcept
        -> gw::Option* override;

  private:
    gw::opts::ToggleClockAutosave m_toggle_clock_autosave_opt;
    gw::opts::AdjustAutosaveInterval m_adjust_autosave_interval_opt;
    gw::opts::GoBackToMainMenu m_go_back_opt;

  public:
    Settings() noexcept;
    auto displayOptions(const gw::Package&) noexcept -> void override final;
};
} // namespace menus
} // namespace gw