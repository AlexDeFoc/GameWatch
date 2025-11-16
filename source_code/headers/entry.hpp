#pragma once

#include <chrono>
#include <string>

using namespace std::chrono_literals;

namespace gw {
class Entry {
  private:
    std::string m_title;
    std::uint64_t m_seconds;

  public:
    Entry() noexcept;
    Entry(std::string&& title) noexcept;

    auto getTitle() const noexcept -> std::string_view;
    auto getSeconds() const noexcept -> std::uint64_t;
    auto getMinutes() const noexcept -> std::uint64_t;
    auto getHours() const noexcept -> std::uint64_t;

    auto setTitle(const std::string& new_title) noexcept -> void;
    auto setTitle(std::string&& new_title) noexcept -> void;
    auto addSeconds(std::uint64_t new_seconds) noexcept -> void;
    auto addMinutes(std::uint64_t new_minutes) noexcept -> void;
    auto addHours(std::uint64_t new_hours) noexcept -> void;

    auto resetClock() noexcept -> void;
};
} // namespace gw