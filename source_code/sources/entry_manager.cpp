#include "entry_manager.hpp"
#include "file_management.hpp"

#include <print>

gw::EntryManager::EntryManager() noexcept
    : any_active_entry{false}
    , cfg{nullptr}
    , m_active_entry_id{0}
    , m_last_time_snapshot{}
    , m_autosave_thread{}
    , m_should_autosave_thread_stop{false} {
    m_entries_list = loadEntriesFromDisk();
}

auto gw::EntryManager::getEntryCount() const noexcept -> std::size_t {
    return m_entries_list.size();
}

auto gw::EntryManager::printEntries() const noexcept -> void {
    std::uint64_t h{}, m{}, s{};

    for (std::size_t i{1}; const auto& e : m_entries_list) {
        {
            std::lock_guard lock{m_mutex};
            h = e.getHours();
            m = e.getMinutes();
            s = e.getSeconds();
        }

        std::println("{}. {} - {} h : {} min : {} s", i, e.getTitle(), h, m, s);
        ++i;
    }
}

auto gw::EntryManager::resetEntryClock(std::uint64_t entry_index) noexcept
    -> void {
    m_entries_list.at(entry_index).resetClock();
}

auto gw::EntryManager::addNewEntry(std::string&& entry_title) noexcept -> void {
    m_entries_list.emplace_back(std::move(entry_title));
}

auto gw::EntryManager::getEntryTitle(std::uint64_t entry_index) const noexcept
    -> std::string_view {
    return m_entries_list.at(entry_index).getTitle();
}

auto gw::EntryManager::setEntryTitle(std::uint64_t entry_index,
                                     std::string&& new_title) noexcept -> void {
    m_entries_list.at(entry_index).setTitle(std::move(new_title));
}

auto gw::EntryManager::saveEntriesToDisk() const noexcept -> void {
    gw::saveEntriesToDisk(m_mutex, m_entries_list);
}

[[nodiscard]] auto gw::EntryManager::getActiveEntryId() const noexcept
    -> std::uint64_t {
    return m_active_entry_id;
}

auto gw::EntryManager::removeEntry(std::uint64_t entry_index) noexcept -> void {
    m_entries_list.erase(m_entries_list.begin() + entry_index);
}

auto gw::EntryManager::getActiveEntryTitle() const noexcept
    -> std::string_view {
    return m_entries_list.at(m_active_entry_id).getTitle();
}

auto gw::EntryManager::saveTimeSinceLastTimeSnapshot() noexcept -> void {
    std::lock_guard lock{m_mutex};

    const auto now = std::chrono::steady_clock::now();
    const auto delta = now - m_last_time_snapshot;
    const auto delta_seconds =
        std::chrono::duration_cast<gw::SecondsU64>(delta);

    m_entries_list.at(m_active_entry_id).addSeconds(delta_seconds.count());

    m_last_time_snapshot = now;
}

auto gw::EntryManager::toggleActiveEntryStatus() noexcept -> void {
    any_active_entry.store(!any_active_entry.load(std::memory_order_acquire),
                           std::memory_order_release);
}

auto gw::EntryManager::takeTimeSnapshot() noexcept -> void {
    std::lock_guard lock{m_mutex};
    m_last_time_snapshot = std::chrono::steady_clock::now();
}

auto gw::EntryManager::setActiveEntryIndex(std::uint64_t entry_index) noexcept
    -> void {
    m_active_entry_id = entry_index;
}

auto gw::EntryManager::prepareAutosave() noexcept -> void {
    if (m_autosave_thread.joinable())
        return;

    m_autosave_thread = std::thread([this]() noexcept {
        using namespace std::chrono;

        milliseconds ms_passed{0ms};
        const milliseconds step{100ms};

        while (!this->m_should_autosave_thread_stop.load(
            std::memory_order_relaxed)) {
            if (cfg->autosave_disabled.load(std::memory_order_relaxed)) {
                std::this_thread::sleep_for(step);
                continue;
            }

            if (!this->any_active_entry.load(std::memory_order_relaxed)) {
                ms_passed = 0ms;
                std::this_thread::sleep_for(step);
                continue;
            }

            std::this_thread::sleep_for(step);

            ms_passed += step;

            const auto sec =
                cfg->autosave_seconds_interval.load(std::memory_order_relaxed);

            const auto threshold = duration_cast<milliseconds>(sec);

            if (ms_passed >= threshold) {
                this->saveTimeSinceLastTimeSnapshot();
                this->saveEntriesToDisk();
                ms_passed = 0ms;
            }
        }
    });
}

gw::EntryManager::~EntryManager() {
    m_should_autosave_thread_stop.store(true, std::memory_order_relaxed);
    if (m_autosave_thread.joinable()) {
        m_autosave_thread.join();
    }
}
