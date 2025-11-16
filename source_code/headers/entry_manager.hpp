#pragma once

#include "config.hpp"
#include "entry.hpp"

#include <atomic>
#include <mutex>
#include <thread>
#include <vector>

namespace gw {
class EntryManager {
  public:
    std::atomic<bool> any_active_entry;
    Config* cfg;

  private:
    std::uint64_t m_active_entry_id;
    std::chrono::steady_clock::time_point m_last_time_snapshot;
    std::vector<Entry> m_entries_list;
    mutable std::mutex m_mutex;
    std::thread m_autosave_thread;
    std::atomic<bool> m_should_autosave_thread_stop;

  public:
    EntryManager() noexcept;

    [[nodiscard]] auto getEntryCount() const noexcept -> std::size_t;

    auto printEntries() const noexcept -> void;

    auto resetEntryClock(std::uint64_t entry_index) noexcept -> void;

    auto addNewEntry(std::string&& entry_title) noexcept -> void;

    [[nodiscard]] auto getEntryTitle(std::uint64_t entry_index) const noexcept
        -> std::string_view;

    auto setEntryTitle(std::uint64_t entry_index,
                       std::string&& new_title) noexcept -> void;

    auto saveEntriesToDisk() const noexcept -> void;

    [[nodiscard]] auto getActiveEntryId() const noexcept -> std::uint64_t;

    auto removeEntry(std::uint64_t entry_index) noexcept -> void;

    [[nodiscard]] auto getActiveEntryTitle() const noexcept -> std::string_view;

    auto saveTimeSinceLastTimeSnapshot() noexcept -> void;

    auto toggleActiveEntryStatus() noexcept -> void;

    auto takeTimeSnapshot() noexcept -> void;

    auto setActiveEntryIndex(std::uint64_t entry_index) noexcept -> void;

    auto prepareAutosave() noexcept -> void;

    ~EntryManager();
};
} // namespace gw