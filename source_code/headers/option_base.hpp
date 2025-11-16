#pragma once

#include "option_id.hpp"
#include "package.hpp"

#include <string>

namespace gw {
class Option {
  protected:
    gw::OptionId m_display_id;
    std::string m_display_text;

  public:
    Option(gw::OptionId display_id, std::string_view display_text) noexcept;

    virtual auto action(gw::Package&) noexcept -> void = 0;

    [[nodiscard]] auto getDisplayId() const noexcept -> int;

    [[nodiscard]] virtual auto
    getDisplayText([[maybe_unused]] const gw::Package&) noexcept
        -> std::string_view;

    virtual ~Option() = default;
};
} // namespace gw