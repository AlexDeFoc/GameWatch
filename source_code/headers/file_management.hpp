#pragma once

#include "config.hpp"
#include "entry.hpp"

#include <mutex>
#include <vector>

namespace gw {
[[nodiscard]] auto loadEntriesFromDisk() noexcept -> std::vector<Entry>;
[[nodiscard]] auto loadConfigFromDisk() noexcept -> Config;
auto saveEntriesToDisk(std::mutex&, const std::vector<Entry>&) noexcept -> void;
auto saveConfigToDisk(const Config&) noexcept -> void;
} // namespace gw