#pragma once

namespace gw {
enum class OptionId {
    // Main menu
    listEntries = 1,
    toggleEntryClock,
    editEntries,
    addNewEntry,
    settings,
    exitApp = 0,

    // Edit entries menu
    changeEntryTitle = 1,
    resetEntryClock,
    deleteEntry,
    goBackToMainMenu = 0,

    // Settings menu
    toggleClockAutosave = 1,
    adjustAutosaveInterval
};
} // namespace gw