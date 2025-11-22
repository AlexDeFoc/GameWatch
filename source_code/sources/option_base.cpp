#include "option_base.hpp"

gw::Option::Option(gw::OptionId display_id,
                   std::string_view display_text) noexcept
    : m_display_id{display_id}
    , m_display_text{display_text} {}

auto gw::Option::getDisplayId() const noexcept -> int {
    return static_cast<int>(m_display_id);
}

auto gw::Option::getDisplayText([[maybe_unused]] const gw::Package&) noexcept
    -> std::string_view {
    return m_display_text;
}
