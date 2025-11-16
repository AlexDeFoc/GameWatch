#pragma once

#include "menu_list.hpp"

namespace gw {
[[nodiscard]] auto loadActiveMenu(const Package&, const MenuList&) noexcept
    -> Menu*;
}