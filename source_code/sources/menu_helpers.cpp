#include "menu_helpers.hpp"

auto gw::loadActiveMenu(const gw::Package& pkg,
                        const gw::MenuList& menu_list) noexcept -> Menu* {
    switch (pkg.active_menu_id) {
    case gw::MenuId::main:
        return menu_list.main_menu;
    case gw::MenuId::editEntries:
        return menu_list.edit_entries_menu;
    case gw::MenuId::settings:
        return menu_list.settings_menu;
    default:
        return nullptr;
    }
}