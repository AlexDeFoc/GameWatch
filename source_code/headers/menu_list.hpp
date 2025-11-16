#pragma once

#include "menus.hpp"

namespace gw {
struct MenuList {
    gw::menus::Main* main_menu;
    gw::menus::EditEntries* edit_entries_menu;
    gw::menus::Settings* settings_menu;
};
} // namespace gw