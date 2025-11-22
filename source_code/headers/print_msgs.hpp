#pragma once

namespace gw::print_msg {
inline static constexpr const char* const no_entries_found{"No entries found!"};

inline static constexpr const char* const entry_created{"Entry created."};

inline static constexpr const char* const press_any_key_to_go_back{
    "Press any key to go back."};

inline static constexpr const char* const enter_n_to_cancel_this_action{
    "Enter \"{}\" to cancel this action."};

inline static constexpr const char* const enter_entry_id{"Enter entry id: "};

inline static constexpr const char* const enter_option_id{"Enter option id: "};

inline static constexpr const char* const enter_autosave_interval{
    "Enter autosave interval in seconds: "};

inline static constexpr const char* const enter_new_entry_title{
    "Enter new entry title: "};

inline static constexpr const char* const input_is_not_a_number{
    "Input is not a number!"};

inline static constexpr const char* const id_is_out_of_range{
    "Id is out of range!"};

inline static constexpr const char* const autosave_interval_input_is_too_small{
    "Input is too low! The interval has to at least 30 seconds."};

inline static constexpr const char* const action_canceled{"Action canceled."};

inline static constexpr const char* const enter_without_any_input_to_cancel{
    "Enter without any input to cancel."};

inline static constexpr const char* const current_version_found_on_github{
    "Current version: {}.{}.{}"};

inline static constexpr const char* const latest_version_found_on_github{
    "Latest version: {}.{}.{}"};

inline static constexpr const char* const
    latest_version_found_on_github_unknown{"Latest version found: Unknown"};

inline static constexpr const char* const failed_to_get_version_from_github{
    "Failed to get version from GitHub!"};

inline static constexpr const char* const cannot_connect_to_github{
    "Cannot connect to GitHub! Check your internet connection!"};

inline static constexpr const char* const
    cannot_delete_entry_with_running_clock{
        "Cannot delete entry with a running clock, stop it first!"};
} // namespace gw::print_msg