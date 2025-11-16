#include "menus.hpp"

#include <iostream>
#include <print>
#include <string>

auto gw::menus::Main::getOptionAddr(int opt_id) noexcept -> gw::Option* {
    static auto to_int = [](gw::OptionId id) { return static_cast<int>(id); };

    switch (opt_id) {
    case to_int(gw::OptionId::listEntries):
        return &m_list_entries_opt;
    case to_int(gw::OptionId::toggleEntryClock):
        return &m_toggle_entry_clock_opt;
    case to_int(gw::OptionId::editEntries):
        return &m_edit_entries_opt;
    case to_int(gw::OptionId::addNewEntry):
        return &m_add_new_entry_opt;
    case to_int(gw::OptionId::settings):
        return &m_settings_opt;
    case to_int(gw::OptionId::checkForUpdates):
        return &m_check_for_updates_opt;
    case to_int(gw::OptionId::exitApp):
        return &m_exit_app_opt;
    default:
        return nullptr;
    }
}

gw::menus::Main::Main() noexcept
    : m_list_entries_opt{}
    , m_toggle_entry_clock_opt{}
    , m_edit_entries_opt{}
    , m_add_new_entry_opt{}
    , m_settings_opt{}
    , m_check_for_updates_opt{}
    , m_exit_app_opt{} {}

auto gw::menus::Main::displayOptions(const Package& pkg) noexcept -> void {
    std::println("{}. {}",
                 m_list_entries_opt.getDisplayId(),
                 m_list_entries_opt.getDisplayText(pkg));

    std::println("{}. {}",
                 m_toggle_entry_clock_opt.getDisplayId(),
                 m_toggle_entry_clock_opt.getDisplayText(pkg));

    std::println("{}. {}",
                 m_edit_entries_opt.getDisplayId(),
                 m_edit_entries_opt.getDisplayText(pkg));

    std::println("{}. {}",
                 m_add_new_entry_opt.getDisplayId(),
                 m_add_new_entry_opt.getDisplayText(pkg));

    std::println("{}. {}",
                 m_settings_opt.getDisplayId(),
                 m_settings_opt.getDisplayText(pkg));

    std::println("{}. {}",
                 m_check_for_updates_opt.getDisplayId(),
                 m_check_for_updates_opt.getDisplayText(pkg));

    std::println("{}. {}",
                 m_exit_app_opt.getDisplayId(),
                 m_exit_app_opt.getDisplayText(pkg));
}

auto gw::menus::EditEntries::getOptionAddr(int opt_id) noexcept -> gw::Option* {
    static auto to_int = [](gw::OptionId id) { return static_cast<int>(id); };

    switch (opt_id) {
    case to_int(gw::OptionId::changeEntryTitle):
        return &m_change_entry_title_opt;
    case to_int(gw::OptionId::resetEntryClock):
        return &m_reset_entry_clock_opt;
    case to_int(gw::OptionId::deleteEntry):
        return &m_delete_entry_opt;
    case to_int(gw::OptionId::goBackToMainMenu):
        return &m_go_back_opt;
    default:
        return nullptr;
    }
}

gw::menus::EditEntries::EditEntries() noexcept
    : m_change_entry_title_opt{}
    , m_reset_entry_clock_opt{}
    , m_delete_entry_opt{}
    , m_go_back_opt{} {}

auto gw::menus::EditEntries::displayOptions(const Package& pkg) noexcept
    -> void {
    std::println("{}. {}",
                 m_change_entry_title_opt.getDisplayId(),
                 m_change_entry_title_opt.getDisplayText(pkg));

    std::println("{}. {}",
                 m_reset_entry_clock_opt.getDisplayId(),
                 m_reset_entry_clock_opt.getDisplayText(pkg));

    std::println("{}. {}",
                 m_delete_entry_opt.getDisplayId(),
                 m_delete_entry_opt.getDisplayText(pkg));

    std::println("{}. {}",
                 m_go_back_opt.getDisplayId(),
                 m_go_back_opt.getDisplayText(pkg));
}

auto gw::menus::Settings::getOptionAddr(int opt_id) noexcept -> gw::Option* {
    static auto to_int = [](gw::OptionId id) { return static_cast<int>(id); };

    switch (opt_id) {
    case to_int(gw::OptionId::toggleClockAutosave):
        return &m_toggle_clock_autosave_opt;
    case to_int(gw::OptionId::adjustAutosaveInterval):
        return &m_adjust_autosave_interval_opt;
    case to_int(gw::OptionId::goBackToMainMenu):
        return &m_go_back_opt;
    default:
        return nullptr;
    }
}

gw::menus::Settings::Settings() noexcept
    : m_toggle_clock_autosave_opt{}
    , m_go_back_opt{} {}

auto gw::menus::Settings::displayOptions(const Package& pkg) noexcept -> void {
    std::println("{}. {}",
                 m_toggle_clock_autosave_opt.getDisplayId(),
                 m_toggle_clock_autosave_opt.getDisplayText(pkg));

    std::println("{}. {}",
                 m_adjust_autosave_interval_opt.getDisplayId(),
                 m_adjust_autosave_interval_opt.getDisplayText(pkg));

    std::println("{}. {}",
                 m_go_back_opt.getDisplayId(),
                 m_go_back_opt.getDisplayText(pkg));
}
