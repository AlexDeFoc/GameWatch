#include "options.hpp"

#include "file_management.hpp"
#include "internet_management.hpp"
#include "print_msgs.hpp"

#include <iostream>
#include <string>

gw::opts::ListEntries::ListEntries() noexcept
    : Option(gw::OptionId::listEntries, "List entries") {}

auto gw::opts::ListEntries::action(gw::Package& pkg) noexcept -> void {
    if (pkg.entry_manager->getEntryCount() == 0) {
        pkg.console->clear();
        pkg.console->println(gw::PrintTag::Error,
                             gw::print_msg::no_entries_found);
        return;
    }

    pkg.console->clear();

    pkg.entry_manager->printEntries();

    pkg.console->println(gw::PrintTag::Info,
                         gw::print_msg::press_any_key_to_go_back);
    pkg.console->getAnyKeyPress();

    pkg.console->clear();
}

gw::opts::ToggleEntryClock::ToggleEntryClock() noexcept
    : Option(gw::OptionId::toggleEntryClock, "Start entry clock") {}

auto gw::opts::ToggleEntryClock::getDisplayText(const gw::Package& pkg) noexcept
    -> std::string_view {
    if (pkg.entry_manager->any_active_entry.load(std::memory_order_relaxed) ==
        true) {
        if (m_display_text == "Start entry clock")
            m_display_text =
                std::format("Stop entry: {}",
                            pkg.entry_manager->getActiveEntryTitle());
    } else if (m_display_text != "Start entry clock") {
        m_display_text = "Start entry clock";
    }

    return m_display_text;
}

auto gw::opts::ToggleEntryClock::action(gw::Package& pkg) noexcept -> void {
    if (pkg.entry_manager->getEntryCount() == 0) {
        pkg.console->clear();
        pkg.console->println(gw::PrintTag::Error,
                             gw::print_msg::no_entries_found);
        return;
    }

    if (pkg.entry_manager->any_active_entry.load(std::memory_order_relaxed)) {
        pkg.console->clear();
        pkg.console->println(
            gw::PrintTag::Info,
            "{}",
            std::format("Entry '{}' clock stopped.",
                        pkg.entry_manager->getActiveEntryTitle()));

        pkg.entry_manager->saveTimeSinceLastTimeSnapshot();

        pkg.entry_manager->saveEntriesToDisk();

        pkg.entry_manager->toggleActiveEntryStatus();
    } else {
        pkg.console->clear();

        bool action_canceled{false};

        std::string input{};
        std::uint64_t selected_entry_id{};
        int cancel_id{0};

        while (true) {
            pkg.entry_manager->printEntries();

            pkg.console->println(
                gw::PrintTag::Tip,
                "{}",
                std::format(gw::print_msg::enter_n_to_cancel_this_action,
                            cancel_id));

            pkg.console->print(gw::PrintTag::Request,
                               gw::print_msg::enter_entry_id);

            std::getline(std::cin, input);

            try {
                selected_entry_id = std::stoull(input);
            } catch (const std::invalid_argument&) {
                pkg.console->clear();
                pkg.console->print(gw::PrintTag::Error,
                                   gw::print_msg::input_is_not_a_number);
                continue;
            } catch (const std::out_of_range&) {
                pkg.console->clear();
                pkg.console->print(gw::PrintTag::Error,
                                   gw::print_msg::id_is_out_of_range);
                continue;
            }

            if (selected_entry_id == cancel_id) {
                action_canceled = true;
            }

            break;
        }

        if (action_canceled) {
            pkg.console->clear();
            pkg.console->println(gw::PrintTag::Info,
                                 gw::print_msg::action_canceled);
        } else {
            pkg.console->clear();

            pkg.entry_manager->toggleActiveEntryStatus();
            pkg.entry_manager->takeTimeSnapshot();
            pkg.entry_manager->setActiveEntryIndex(selected_entry_id - 1);

            pkg.console->println(
                gw::PrintTag::Info,
                "{}",
                std::format("Entry '{}' clock started.",
                            pkg.entry_manager->getActiveEntryTitle()));
        }
    }
}

gw::opts::EditEntries::EditEntries() noexcept
    : Option(gw::OptionId::editEntries, "Edit entries") {}

auto gw::opts::EditEntries::action(gw::Package& pkg) noexcept -> void {
    pkg.console->clear();
    pkg.active_menu_id = gw::MenuId::editEntries;
}

gw::opts::AddNewEntry::AddNewEntry() noexcept
    : Option(gw::OptionId::addNewEntry, "Add new entry") {}

auto gw::opts::AddNewEntry::action(gw::Package& pkg) noexcept -> void {
    pkg.console->clear();

    bool action_canceled{false};
    std::string input{};

    pkg.console->println(gw::PrintTag::Tip,
                         gw::print_msg::enter_without_any_input_to_cancel);

    pkg.console->print(gw::PrintTag::Request,
                       gw::print_msg::enter_new_entry_title);

    std::getline(std::cin, input);

    if (input == "")
        action_canceled = true;

    if (action_canceled) {
        pkg.console->clear();
        pkg.console->println(gw::PrintTag::Info,
                             gw::print_msg::action_canceled);
    } else {
        pkg.console->clear();
        pkg.console->println(gw::PrintTag::Info, gw::print_msg::entry_created);
        pkg.entry_manager->addNewEntry(std::move(input));
        pkg.entry_manager->saveEntriesToDisk();
    }
}

gw::opts::Settings::Settings() noexcept
    : Option(gw::OptionId::settings, "Settings") {}

auto gw::opts::Settings::action(gw::Package& pkg) noexcept -> void {
    pkg.console->clear();
    pkg.active_menu_id = gw::MenuId::settings;
}

gw::opts::CheckForUpdates::CheckForUpdates() noexcept
    : Option(gw::OptionId::checkForUpdates, "Check for updates") {}

auto gw::opts::CheckForUpdates::action(gw::Package& pkg) noexcept -> void {
    pkg.console->clear();

    constexpr auto current_app_ver_major{1};
    constexpr auto current_app_ver_minor{0};
    constexpr auto current_app_ver_patch{0};

    pkg.console->println(
        gw::PrintTag::Info,
        "{}",
        std::format(gw::print_msg::current_version_found_on_github,
                    current_app_ver_major,
                    current_app_ver_minor,
                    current_app_ver_patch));

    bool user_can_connect_to_github = gw::attemptGithubConnection();

    if (user_can_connect_to_github) {
        const auto result = gw::getLatestReleaseTagFromGithub();

        if (!result.has_value()) {
            pkg.console->println(
                gw::PrintTag::Info,
                gw::print_msg::latest_version_found_on_github_unknown);

            pkg.console->println(
                gw::PrintTag::Error,
                gw::print_msg::failed_to_get_version_from_github);
        } else {
            const auto& [major, minor, patch] = result.value();

            pkg.console->println(
                gw::PrintTag::Info,
                "{}",
                std::format(gw::print_msg::current_version_found_on_github,
                            major,
                            minor,
                            patch));
        }
    } else {
        pkg.console->println(
            gw::PrintTag::Info,
            gw::print_msg::latest_version_found_on_github_unknown);

        pkg.console->println(gw::PrintTag::Error,
                             gw::print_msg::cannot_connect_to_github);
    }

    pkg.console->println(gw::PrintTag::Info,
                         gw::print_msg::press_any_key_to_go_back);
    pkg.console->getAnyKeyPress();

    pkg.console->clear();
}

gw::opts::ExitApp::ExitApp() noexcept
    : Option(gw::OptionId::exitApp, "Exit app") {}

auto gw::opts::ExitApp::action(gw::Package& pkg) noexcept -> void {
    pkg.keep_app_running = false;

    if (pkg.entry_manager->any_active_entry.load(std::memory_order_relaxed)) {
        pkg.entry_manager->saveTimeSinceLastTimeSnapshot();
        pkg.entry_manager->saveEntriesToDisk();
        pkg.entry_manager->toggleActiveEntryStatus();
    }

    pkg.console->println(gw::PrintTag::Info, gw::print_msg::exiting_app);
}

gw::opts::GoBackToMainMenu::GoBackToMainMenu() noexcept
    : Option(gw::OptionId::goBackToMainMenu, "Go back") {}

auto gw::opts::GoBackToMainMenu::action(gw::Package& pkg) noexcept -> void {
    pkg.console->clear();
    pkg.active_menu_id = gw::MenuId::main;
}

gw::opts::ChangeEntryTitle::ChangeEntryTitle() noexcept
    : Option(gw::OptionId::changeEntryTitle, "Change entry title") {}

auto gw::opts::ChangeEntryTitle::action(gw::Package& pkg) noexcept -> void {
    pkg.console->clear();

    bool action_canceled{false};

    std::string input{};
    std::uint64_t selected_entry_id{};
    int cancel_id{0};

    while (true) {
        pkg.entry_manager->printEntries();

        pkg.console->println(
            gw::PrintTag::Tip,
            "{}",
            std::format(gw::print_msg::enter_n_to_cancel_this_action,
                        cancel_id));

        pkg.console->print(gw::PrintTag::Request,
                           gw::print_msg::enter_entry_id);

        std::getline(std::cin, input);

        try {
            selected_entry_id = std::stoull(input);
        } catch (const std::invalid_argument&) {
            pkg.console->clear();
            pkg.console->print(gw::PrintTag::Error,
                               gw::print_msg::input_is_not_a_number);
            continue;
        } catch (const std::out_of_range&) {
            pkg.console->clear();
            pkg.console->print(gw::PrintTag::Error,
                               gw::print_msg::id_is_out_of_range);
            continue;
        }

        if (selected_entry_id == cancel_id) {
            action_canceled = true;
        }

        break;
    }

    if (action_canceled) {
        pkg.console->clear();
        pkg.console->println(gw::PrintTag::Info,
                             gw::print_msg::action_canceled);
        return;
    }

    pkg.console->clear();

    pkg.console->println(gw::PrintTag::Tip,
                         gw::print_msg::enter_without_any_input_to_cancel);

    pkg.console->print(gw::PrintTag::Request,
                       gw::print_msg::enter_new_entry_title);

    std::getline(std::cin, input);

    if (input == "")
        action_canceled = true;

    if (action_canceled) {
        pkg.console->clear();
        pkg.console->println(gw::PrintTag::Info,
                             gw::print_msg::action_canceled);
    } else {
        pkg.console->clear();

        pkg.console->println(
            gw::PrintTag::Info,
            "{}",
            std::format("Entry with title '{}' to '{}'.",
                        pkg.entry_manager->getEntryTitle(selected_entry_id - 1),
                        input));

        pkg.entry_manager->setEntryTitle(selected_entry_id - 1,
                                         std::move(input));

        pkg.entry_manager->saveEntriesToDisk();
    }
}

gw::opts::ResetEntryClock::ResetEntryClock() noexcept
    : Option(gw::OptionId::resetEntryClock, "Reset entry clock") {}

auto gw::opts::ResetEntryClock::action(gw::Package& pkg) noexcept -> void {
    pkg.console->clear();

    bool action_canceled{false};

    std::string input{};
    std::uint64_t selected_entry_id{};
    int cancel_id{0};

    while (true) {
        pkg.entry_manager->printEntries();

        pkg.console->println(
            gw::PrintTag::Tip,
            "{}",
            std::format(gw::print_msg::enter_n_to_cancel_this_action,
                        cancel_id));

        pkg.console->print(gw::PrintTag::Request,
                           gw::print_msg::enter_entry_id);

        std::getline(std::cin, input);

        try {
            selected_entry_id = std::stoull(input);
        } catch (const std::invalid_argument&) {
            pkg.console->clear();
            pkg.console->print(gw::PrintTag::Error,
                               gw::print_msg::input_is_not_a_number);
            continue;
        } catch (const std::out_of_range&) {
            pkg.console->clear();
            pkg.console->print(gw::PrintTag::Error,
                               gw::print_msg::id_is_out_of_range);
            continue;
        }

        if (selected_entry_id == cancel_id) {
            action_canceled = true;
        }

        break;
    }

    if (action_canceled) {
        pkg.console->clear();
        pkg.console->println(gw::PrintTag::Info,
                             gw::print_msg::action_canceled);
        return;
    }

    pkg.console->clear();

    while (true) {
        std::println("Are you sure?");
        std::println("1. Yes");
        std::println("2. No");

        std::getline(std::cin, input);
        std::uint64_t selected_option_id{};

        try {
            selected_option_id = std::stoull(input);
        } catch (const std::invalid_argument&) {
            pkg.console->clear();
            pkg.console->print(gw::PrintTag::Error,
                               gw::print_msg::input_is_not_a_number);
            continue;
        } catch (const std::out_of_range&) {
            pkg.console->clear();
            pkg.console->print(gw::PrintTag::Error,
                               gw::print_msg::id_is_out_of_range);
            continue;
        }

        if (selected_option_id == 2)
            action_canceled = true;
        else if (selected_option_id != 1) {
            pkg.console->clear();
            pkg.console->print(gw::PrintTag::Error,
                               gw::print_msg::id_is_out_of_range);
            continue;
        }

        break;
    }

    if (action_canceled) {
        pkg.console->clear();
        pkg.console->println(gw::PrintTag::Info,
                             gw::print_msg::action_canceled);
    } else {
        pkg.console->clear();

        pkg.console->println(gw::PrintTag::Info,
                             "{}",
                             std::format("Entry '{}' clock reset.",
                                         pkg.entry_manager->getEntryTitle(
                                             selected_entry_id - 1)));

        pkg.entry_manager->resetEntryClock(selected_entry_id - 1);

        pkg.entry_manager->saveEntriesToDisk();
    }
}

gw::opts::DeleteEntry::DeleteEntry() noexcept
    : Option(gw::OptionId::deleteEntry, "Delete entry") {}

auto gw::opts::DeleteEntry::action(gw::Package& pkg) noexcept -> void {
    pkg.console->clear();

    bool action_canceled{false};

    std::string input{};
    std::uint64_t selected_entry_id{};
    int cancel_id{0};

    while (true) {
        pkg.entry_manager->printEntries();

        pkg.console->println(
            gw::PrintTag::Tip,
            "{}",
            std::format(gw::print_msg::enter_n_to_cancel_this_action,
                        cancel_id));

        pkg.console->print(gw::PrintTag::Request,
                           gw::print_msg::enter_entry_id);

        std::getline(std::cin, input);

        try {
            selected_entry_id = std::stoull(input);
        } catch (const std::invalid_argument&) {
            pkg.console->clear();
            pkg.console->print(gw::PrintTag::Error,
                               gw::print_msg::input_is_not_a_number);
            continue;
        } catch (const std::out_of_range&) {
            pkg.console->clear();
            pkg.console->print(gw::PrintTag::Error,
                               gw::print_msg::id_is_out_of_range);
            continue;
        }

        if (selected_entry_id == cancel_id) {
            action_canceled = true;
        } else if (pkg.entry_manager->any_active_entry.load(
                       std::memory_order_relaxed) &&
                   selected_entry_id ==
                       pkg.entry_manager->getActiveEntryId() + 1) {
            pkg.console->clear();
            pkg.console->println(
                gw::PrintTag::Error,
                gw::print_msg::cannot_delete_entry_with_running_clock);
            continue;
        }

        break;
    }

    if (action_canceled) {
        pkg.console->clear();
        pkg.console->println(gw::PrintTag::Info,
                             gw::print_msg::action_canceled);
        return;
    }

    pkg.console->clear();

    while (true) {
        std::println("Are you sure?");
        std::println("1. Yes");
        std::println("2. No");

        std::getline(std::cin, input);
        std::uint64_t selected_option_id{};

        try {
            selected_option_id = std::stoull(input);
        } catch (const std::invalid_argument&) {
            pkg.console->clear();
            pkg.console->print(gw::PrintTag::Error,
                               gw::print_msg::input_is_not_a_number);
            continue;
        } catch (const std::out_of_range&) {
            pkg.console->clear();
            pkg.console->print(gw::PrintTag::Error,
                               gw::print_msg::id_is_out_of_range);
            continue;
        }

        if (selected_option_id == 2)
            action_canceled = true;
        else if (selected_option_id != 1) {
            pkg.console->clear();
            pkg.console->print(gw::PrintTag::Error,
                               gw::print_msg::id_is_out_of_range);
            continue;
        }

        break;
    }

    if (action_canceled) {
        pkg.console->clear();
        pkg.console->println(gw::PrintTag::Info,
                             gw::print_msg::action_canceled);
    } else {
        pkg.console->clear();
        pkg.console->println(gw::PrintTag::Info,
                             "{}",
                             std::format("Entry '{}' deleted.",
                                         pkg.entry_manager->getEntryTitle(
                                             selected_entry_id - 1)));

        pkg.entry_manager->removeEntry(selected_entry_id - 1);
        pkg.entry_manager->saveEntriesToDisk();
    }
}

gw::opts::ToggleClockAutosave::ToggleClockAutosave() noexcept
    : Option(gw::OptionId::toggleClockAutosave, "Enable clock autosave") {}

auto gw::opts::ToggleClockAutosave::getDisplayText(
    const gw::Package& pkg) noexcept -> std::string_view {

    if (pkg.cfg->autosave_disabled.load(std::memory_order_relaxed)) {
        if (m_display_text == "Disable clock autosave")
            m_display_text = "Enable clock autosave";
    } else if (m_display_text == "Enable clock autosave") {
        m_display_text = "Disable clock autosave";
    }

    return m_display_text;
}

auto gw::opts::ToggleClockAutosave::action(gw::Package& pkg) noexcept -> void {
    pkg.console->clear();

    pkg.cfg->autosave_disabled.store(
        !pkg.cfg->autosave_disabled.load(std::memory_order_acquire),
        std::memory_order_release);

    saveConfigToDisk(*(pkg.cfg));
}

gw::opts::AdjustAutosaveInterval::AdjustAutosaveInterval() noexcept
    : Option(gw::OptionId::adjustAutosaveInterval, "Adjust autosave interval") {
}

auto gw::opts::AdjustAutosaveInterval::action(gw::Package& pkg) noexcept
    -> void {
    pkg.console->clear();

    std::string input{};
    std::uint64_t interval_in_sec{0};
    bool action_canceled{false};
    const std::uint64_t current_val =
        pkg.cfg->autosave_seconds_interval.load(std::memory_order_relaxed)
            .count();

    int cancel_id{0};

    while (true) {
        pkg.console->println(
            gw::PrintTag::Info,
            "{}",
            std::format("Current value is: {} h : {} min : {} s",
                        current_val / 3600,
                        (current_val % 3600) / 60,
                        (current_val % 3600) % 60));

        pkg.console->println(
            gw::PrintTag::Tip,
            "{}",
            std::format(gw::print_msg::enter_n_to_cancel_this_action,
                        cancel_id));

        pkg.console->print(gw::PrintTag::Request,
                           gw::print_msg::enter_autosave_interval);

        std::getline(std::cin, input);

        try {
            interval_in_sec = std::stoull(input);
        } catch (const std::invalid_argument&) {
            pkg.console->clear();
            pkg.console->print(gw::PrintTag::Error,
                               gw::print_msg::input_is_not_a_number);
            continue;
        } catch (const std::out_of_range&) {
            pkg.console->clear();
            pkg.console->print(gw::PrintTag::Error,
                               gw::print_msg::id_is_out_of_range);
            continue;
        }

        if (interval_in_sec == 0)
            action_canceled = true;
        else if (interval_in_sec < 30) {
            pkg.console->clear();
            pkg.console->print(
                gw::PrintTag::Error,
                gw::print_msg::autosave_interval_input_is_too_small);
            continue;
        }

        break;
    }

    if (action_canceled) {
        pkg.console->clear();
        pkg.console->println(gw::PrintTag::Info,
                             gw::print_msg::action_canceled);
    } else {
        pkg.cfg->autosave_seconds_interval.store(
            gw::SecondsU64{interval_in_sec},
            std::memory_order_relaxed);

        saveConfigToDisk(*(pkg.cfg));
    }
}
