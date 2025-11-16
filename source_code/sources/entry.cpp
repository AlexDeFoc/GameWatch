#include "entry.hpp"

gw::Entry::Entry() noexcept : m_title{""}, m_seconds{0} {}

gw::Entry::Entry(std::string&& title) noexcept
    : m_title{std::move(title)}
    , m_seconds{0} {}

auto gw::Entry::getTitle() const noexcept -> std::string_view {
    return m_title;
}

auto gw::Entry::getSeconds() const noexcept -> std::uint64_t {
    return (m_seconds % 3600) % 60;
}

auto gw::Entry::getMinutes() const noexcept -> std::uint64_t {
    return (m_seconds % 3600) / 60;
}

auto gw::Entry::getHours() const noexcept -> std::uint64_t {
    return m_seconds / 3600;
}

auto gw::Entry::setTitle(const std::string& new_title) noexcept -> void {
    m_title = new_title;
}

auto gw::Entry::setTitle(std::string&& new_title) noexcept -> void {
    m_title = std::move(new_title);
}

auto gw::Entry::addSeconds(std::uint64_t new_seconds) noexcept -> void {
    m_seconds += new_seconds;
}

auto gw::Entry::addMinutes(std::uint64_t new_minutes) noexcept -> void {
    m_seconds += new_minutes * 60;
}

auto gw::Entry::addHours(std::uint64_t new_hours) noexcept -> void {
    m_seconds += new_hours * 3600;
}

auto gw::Entry::resetClock() noexcept -> void { m_seconds = 0; }