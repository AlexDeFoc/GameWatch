#include "options.hpp"

#include "file_management.hpp"

#include <iostream>
#include <print>
#include <string>

gw::opts::ListEntries::ListEntries() noexcept
    : Option(gw::OptionId::listEntries, "List entries") {}

auto gw::opts::ListEntries::action(gw::Package& pkg) noexcept -> void {
    if (pkg.entry_manager->getEntryCount() == 0) {
        pkg.console->clear();
        std::println("No entries found!");
        return;
    }

    pkg.console->clear();

    pkg.entry_manager->printEntries();

    std::println("Press any key to go back");
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
            m_display_text = std::format("Stop entry: {}", "tmp");
    } else if (m_display_text != "Start entry clock") {
        m_display_text = "Start entry clock";
    }

    return m_display_text;
}

auto gw::opts::ToggleEntryClock::action(gw::Package& pkg) noexcept -> void {
    if (pkg.entry_manager->getEntryCount() == 0) {
        pkg.console->clear();
        std::println("No entries found!");
        return;
    }

    if (pkg.entry_manager->any_active_entry.load(std::memory_order_relaxed)) {
        pkg.console->clear();
        std::println("Entry '{}' clock stopped.",
                     pkg.entry_manager->getActiveEntryTitle());

        pkg.entry_manager->saveTimeSinceLastTimeSnapshot();

        pkg.entry_manager->saveEntriesToDisk();

        pkg.entry_manager->toggleActiveEntryStatus();
    } else {
        pkg.console->clear();

        bool action_canceled{false};

        std::string input{};
        std::uint64_t selected_entry_id{};

        while (true) {
            pkg.entry_manager->printEntries();

            std::println("Enter '0' to cancel this action.");
            std::print("Enter entry id: ");

            std::getline(std::cin, input);

            try {
                selected_entry_id = std::stoull(input);
            } catch (const std::invalid_argument&) {
                pkg.console->clear();
                std::println("Input is not a number!");
                continue;
            } catch (const std::out_of_range&) {
                pkg.console->clear();
                std::println("Id is out of range!");
                continue;
            }

            if (selected_entry_id == 0) {
                action_canceled = true;
            }

            break;
        }

        if (action_canceled) {
            pkg.console->clear();
            std::println("Action canceled.");
        } else {
            pkg.console->clear();

            pkg.entry_manager->toggleActiveEntryStatus();
            pkg.entry_manager->takeTimeSnapshot();
            pkg.entry_manager->setActiveEntryIndex(selected_entry_id - 1);

            std::println("Entry '{}' clock started.",
                         pkg.entry_manager->getActiveEntryTitle());
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

    std::println("Enter without any input to cancel.");
    std::print("Enter new entry title: ");

    std::getline(std::cin, input);

    if (input == "")
        action_canceled = true;

    if (action_canceled) {
        pkg.console->clear();
        std::println("Action canceled.");
    } else {
        pkg.console->clear();
        std::println("Entry created.");
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

gw::opts::ExitApp::ExitApp() noexcept
    : Option(gw::OptionId::exitApp, "Exit app") {}

auto gw::opts::ExitApp::action(gw::Package& pkg) noexcept -> void {
    pkg.keep_app_running = false;

    if (pkg.entry_manager->any_active_entry.load(std::memory_order_relaxed)) {
        pkg.entry_manager->saveTimeSinceLastTimeSnapshot();
        pkg.entry_manager->saveEntriesToDisk();
        pkg.entry_manager->toggleActiveEntryStatus();
    }
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

    while (true) {
        pkg.entry_manager->printEntries();

        std::println("Enter '0' to cancel this action.");
        std::print("Enter entry id: ");

        std::getline(std::cin, input);

        try {
            selected_entry_id = std::stoull(input);
        } catch (const std::invalid_argument&) {
            pkg.console->clear();
            std::println("Input is not a number!");
            continue;
        } catch (const std::out_of_range&) {
            pkg.console->clear();
            std::println("Id is out of range!");
            continue;
        }

        if (selected_entry_id == 0) {
            action_canceled = true;
        }

        break;
    }

    if (action_canceled) {
        pkg.console->clear();
        std::println("Action canceled.");
        return;
    }

    pkg.console->clear();
    std::println("Enter without any input to cancel.");
    std::print("Enter new entry title: ");

    std::getline(std::cin, input);

    if (input == "")
        action_canceled = true;

    if (action_canceled) {
        pkg.console->clear();
        std::println("Action canceled.");
    } else {
        pkg.console->clear();

        std::println("Entry with title '{}' to '{}'.",
                     pkg.entry_manager->getEntryTitle(selected_entry_id - 1),
                     input);

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

    while (true) {
        pkg.entry_manager->printEntries();

        std::println("Enter '0' to cancel this action.");
        std::print("Enter entry id: ");

        std::getline(std::cin, input);

        try {
            selected_entry_id = std::stoull(input);
        } catch (const std::invalid_argument&) {
            pkg.console->clear();
            std::println("Input is not a number!");
            continue;
        } catch (const std::out_of_range&) {
            pkg.console->clear();
            std::println("Id is out of range!");
            continue;
        }

        if (selected_entry_id == 0) {
            action_canceled = true;
        }

        break;
    }

    if (action_canceled) {
        pkg.console->clear();
        std::println("Action canceled.");
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
            std::println("Input is not a number!");
            continue;
        } catch (const std::out_of_range&) {
            pkg.console->clear();
            std::println("Id is out of range!");
            continue;
        }

        if (selected_option_id == 2)
            action_canceled = true;
        else if (selected_option_id != 1) {
            pkg.console->clear();
            std::println("Id is out of range!");
            continue;
        }

        break;
    }

    if (action_canceled) {
        pkg.console->clear();
        std::println("Action canceled.");
    } else {
        pkg.console->clear();

        std::println("Entry '{}' clock reset.",
                     pkg.entry_manager->getEntryTitle(selected_entry_id - 1));

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

    while (true) {
        pkg.entry_manager->printEntries();

        std::println("Enter '0' to cancel this action.");
        std::print("Enter entry id: ");

        std::getline(std::cin, input);

        try {
            selected_entry_id = std::stoull(input);
        } catch (const std::invalid_argument&) {
            pkg.console->clear();
            std::println("Input is not a number!");
            continue;
        } catch (const std::out_of_range&) {
            pkg.console->clear();
            std::println("Id is out of range!");
            continue;
        }

        if (selected_entry_id == 0) {
            action_canceled = true;
        } else if (pkg.entry_manager->any_active_entry.load(
                       std::memory_order_relaxed) &&
                   selected_entry_id == pkg.entry_manager->getActiveEntryId()) {
            pkg.console->clear();
            std::println(
                "Cannot delete entry with a running clock, stop it first!");
            continue;
        }

        break;
    }

    if (action_canceled) {
        pkg.console->clear();
        std::println("Action canceled.");
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
            std::println("Input is not a number!");
            continue;
        } catch (const std::out_of_range&) {
            pkg.console->clear();
            std::println("Id is out of range!");
            continue;
        }

        if (selected_option_id == 2)
            action_canceled = true;
        else if (selected_option_id != 1) {
            pkg.console->clear();
            std::println("Id is out of range!");
            continue;
        }

        break;
    }

    if (action_canceled) {
        pkg.console->clear();
        std::println("Action canceled.");
    } else {
        pkg.console->clear();
        std::println("Entry '{}' deleted.",
                     pkg.entry_manager->getEntryTitle(selected_entry_id - 1));

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

    while (true) {
        std::println("To cancel this action enter '0'");
        std::print("Enter autosave interval in seconds: ");

        std::getline(std::cin, input);

        try {
            interval_in_sec = std::stoull(input);
        } catch (const std::invalid_argument&) {
            pkg.console->clear();
            std::println("Input is not a number!");
            continue;
        } catch (const std::out_of_range&) {
            pkg.console->clear();
            std::println("Input is too large!");
            continue;
        }

        if (interval_in_sec == 0)
            action_canceled = true;
        else if (interval_in_sec < 30) {
            pkg.console->clear();
            std::println("Input is too small! The interval has to be at least "
                         "30 seconds.");
            continue;
        }

        break;
    }

    if (action_canceled) {
        pkg.console->clear();
        std::println("Action canceled.");
    } else {
        pkg.cfg->autosave_seconds_interval.store(
            gw::SecondsU64{interval_in_sec},
            std::memory_order_relaxed);

        saveConfigToDisk(*(pkg.cfg));
    }
}
