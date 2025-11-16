#pragma once

#include "seconds_type.hpp"

#include <atomic>

namespace gw {
struct Config {
  public:
    std::atomic<bool> autosave_disabled;
    std::atomic<gw::SecondsU64> autosave_seconds_interval;

  public:
    Config() noexcept;
    Config(const Config&) noexcept;
    Config(Config&&) noexcept;
    [[nodiscard]] auto operator=(const Config&) noexcept -> Config&;
    [[nodiscard]] auto operator=(Config&&) noexcept -> Config&;
};
} // namespace gw