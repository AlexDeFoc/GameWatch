#include "config.hpp"

using namespace std::chrono_literals;

gw::Config::Config() noexcept
    : autosave_disabled{false}
    , autosave_seconds_interval{15min} {}

gw::Config::Config(const Config& other) noexcept
    : autosave_disabled{other.autosave_disabled.load(std::memory_order_acquire)}
    , autosave_seconds_interval{
          other.autosave_seconds_interval.load(std::memory_order_acquire)} {}

gw::Config::Config(Config&& other) noexcept
    : autosave_disabled{other.autosave_disabled.load(std::memory_order_acquire)}
    , autosave_seconds_interval{
          other.autosave_seconds_interval.load(std::memory_order_acquire)} {}

auto gw::Config::operator=(const Config& other) noexcept -> Config& {
    if (this == &other)
        return *this;

    autosave_disabled.store(
        other.autosave_disabled.load(std::memory_order_acquire),
        std::memory_order_release);

    autosave_seconds_interval.store(
        other.autosave_seconds_interval.load(std::memory_order_acquire),
        std::memory_order_release);

    return *this;
}

auto gw::Config::operator=(Config&& other) noexcept -> Config& {
    if (this == &other)
        return *this;

    autosave_disabled.store(
        other.autosave_disabled.load(std::memory_order_acquire),
        std::memory_order_release);

    autosave_seconds_interval.store(
        other.autosave_seconds_interval.load(std::memory_order_acquire),
        std::memory_order_release);

    return *this;
}
