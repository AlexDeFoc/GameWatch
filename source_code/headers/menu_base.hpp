#pragma once

#include "option_base.hpp"
#include "package.hpp"

namespace gw {
class Menu {
  protected:
    int m_selected_opt_id;

  protected:
    [[nodiscard]] virtual auto getOptionAddr(int opt_id) noexcept
        -> gw::Option* = 0;

  public:
    Menu() noexcept;
    virtual auto displayOptions(const Package&) noexcept -> void = 0;
    auto requestInput(const Package&) noexcept -> void;
    auto executeSelectedOptAction(gw::Package&) noexcept -> void;
    virtual ~Menu() = default;
};
} // namespace gw