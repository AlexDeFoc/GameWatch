#include "file_management.hpp"

#include <expected>
#include <filesystem>
#include <format>
#include <fstream>
#include <string>

#if defined(_WIN32) || defined(_WIN64)
#include <Windows.h>
#elif defined(__APPLE__)
#include <mach-o/dyld.h>
#endif

namespace {
constexpr const char* entries_filename{"entries.ini"};
constexpr const char* config_filename{"config.ini"};

[[nodiscard]] auto getExecutablePath() noexcept
    -> std::expected<std::filesystem::path, bool> {

#if defined(_WIN32) || defined(_WIN64)
    std::wstring buffer{MAX_PATH, L'\0'};

    while (true) {
        DWORD filepath_read_len =
            ::GetModuleFileNameW(nullptr,
                                 buffer.data(),
                                 static_cast<DWORD>(buffer.size()));

        if (filepath_read_len == 0) {
            return std::unexpected<bool>(false);
        }

        if (filepath_read_len < buffer.size() - 1) {
            buffer.resize(filepath_read_len);
            break;
        }

        buffer.resize(buffer.size() * 2);
    }

    return std::filesystem::path{buffer}.parent_path();
#elif defined(__linux__)
    return std::filesystem::path{std::filesystem::canonical("/proc/self/exe")}
        .parent_path();
#elif defined(__APPLE__) // @todo untested
    std::string buffer(PATH_MAX, '\0');
    uint32_t size = static_cast<uint32_t>(buffer.size());

    if (_NSGetExecutablePath(buffer.data(), &size) != 0) {
        buffer.resize(size);

        if (_NSGetExecutablePath(buffer.data(), &size) != 0) {
            return std::unexpected<bool>(false);
        }
    }

    return std::filesystem::path{std::filesystem::canonical(buffer.c_str())}
        .parent_path();
#endif
}

// @note
// Returns file path relative to the exe or the cwd if 'getExecutablePath'
// failed
[[nodiscard]] auto getEntriesFilePath() noexcept -> std::filesystem::path {
    auto exe_path = getExecutablePath();

    if (exe_path.has_value()) {
        return exe_path.value() / entries_filename;
    } else {
        return std::filesystem::path{entries_filename};
    }
}

// @note
// Returns file path relative to the exe or the cwd if 'getExecutablePath'
// failed
[[nodiscard]] auto getConfigFilePath() noexcept -> std::filesystem::path {
    auto exe_path = getExecutablePath();

    if (exe_path.has_value()) {
        return exe_path.value() / config_filename;
    } else {
        return std::filesystem::path{config_filename};
    }
}
} // namespace

auto gw::loadEntriesFromDisk() noexcept -> std::vector<Entry> {
    const auto entries_filepath = getEntriesFilePath();

    if (!std::filesystem::exists(entries_filepath)) {
        return {};
    }

    std::ifstream entries_file{entries_filepath};

    if (!entries_file.is_open()) {
        return {};
    }

    std::vector<Entry> entries{};
    Entry entry{};
    std::string line{};
    std::int64_t entry_index{0};

    while (std::getline(entries_file, line)) {
        if (line.starts_with("Title")) {
            auto colon_pos = line.find(':');

            if (colon_pos == std::string::npos) {
                entry.setTitle(
                    std::format("ENTRY #{} CONTAINS INVALID TITLE FIELD",
                                entry_index + 1));
                continue;
            }

            entry.setTitle(line.substr(colon_pos + 2));
        } else if (line.starts_with("Clock")) {
            auto colon_pos = line.find(':');

            if (colon_pos == std::string::npos) {
                entry.resetClock();
                continue;
            }

            auto clock_segment = line.substr(colon_pos + 2);

            auto first_colon = clock_segment.find(':');
            auto second_colon = clock_segment.find(':', first_colon + 1);

            if (first_colon == std::string::npos ||
                second_colon == std::string::npos) {
                entry.resetClock();
                continue;
            }

            auto hours_segment = clock_segment.substr(0, first_colon);

            auto minutes_segment =
                clock_segment.substr(first_colon + 1,
                                     second_colon - first_colon - 1);

            auto seconds_segment = clock_segment.substr(second_colon + 1);

            try {
                entry.addHours(std::stoul(hours_segment));
                entry.addMinutes(std::stoul(minutes_segment));
                entry.addSeconds(std::stoul(seconds_segment));
            } catch (...) {
                entry.setTitle(
                    std::format("ENTRY #{} CONTAINS INVALID TIME FIELD",
                                entry_index + 1));
                entry.resetClock();
            }
        } else if (line.starts_with("[END ENTRY]")) {
            entries.push_back(entry);
            entry.setTitle("");
            entry.resetClock();
            ++entry_index;
        }
    }

    return entries;
}

auto gw::loadConfigFromDisk() noexcept -> Config {
    const auto config_filepath = getConfigFilePath();

    if (!std::filesystem::exists(config_filepath)) {
        return {};
    }

    std::ifstream config_file{config_filepath};

    if (!config_file.is_open()) {
        return {};
    }

    gw::Config cfg{};
    std::string line{};

    while (std::getline(config_file, line)) {
        if (line.starts_with("AutosaveDisabled")) {
            const auto colon_pos = line.find(':');

            if (colon_pos == std::string::npos) {
                // leave cfg.autosave_disabled = default;
                continue;
            }

            const auto extracted_value = line.substr(colon_pos + 2);

            if (extracted_value == "true")
                cfg.autosave_disabled.store(true, std::memory_order_relaxed);
            else if (extracted_value == "false")
                cfg.autosave_disabled.store(false, std::memory_order_relaxed);
            else {
                // leave cfg.autosave_disabled = default;
            }
        } else if (line.starts_with("AutosaveSecondsInterval")) {
            const auto colon_pos = line.find(':');

            if (colon_pos == std::string::npos) {
                // leave cfg.autosave_seconds_interval = default;
                continue;
            }

            const auto extracted_value = line.substr(colon_pos + 2);
            std::uint64_t extracted_value_as_number{};

            try {
                extracted_value_as_number = std::stoull(extracted_value);

                cfg.autosave_seconds_interval.store(
                    gw::SecondsU64{extracted_value_as_number},
                    std::memory_order_relaxed);
            } catch (...) {
                // leave cfg.autosave_seconds_interval = default;
            }
        }
    }

    return cfg;
}

auto gw::saveEntriesToDisk(std::mutex& mutex,
                           const std::vector<Entry>& entries_list) noexcept
    -> void {
    const auto entries_path = getEntriesFilePath();

    std::ofstream entries_file{entries_path};

    if (!entries_file.is_open()) {
        return;
    }

    std::uint64_t h{}, m{}, s{};

    for (const auto& e : entries_list) {
        {
            std::lock_guard lock{mutex};
            h = e.getHours();
            m = e.getMinutes();
            s = e.getSeconds();
        }

        entries_file << "[START ENTRY]\n";
        entries_file << std::format("Title: {}\n", e.getTitle());
        entries_file << std::format("Clock: {:02}:{:02}:{:02}\n", h, m, s);
        entries_file << "[END ENTRY]\n\n";
    }
}

auto gw::saveConfigToDisk(const Config& cfg) noexcept -> void {
    const auto config_path = getConfigFilePath();

    std::ofstream config_file{config_path};

    if (!config_file.is_open()) {
        return;
    }

    config_file << std::format(
        "AutosaveDisabled: {}\n",
        cfg.autosave_disabled.load(std::memory_order_relaxed));

    config_file << std::format(
        "AutosaveSecondsInterval: {}\n",
        cfg.autosave_seconds_interval.load(std::memory_order_relaxed).count());
}
