#include "config.hpp"
#include "console.hpp"
#include "entry_manager.hpp"
#include "file_management.hpp"
#include "menu_helpers.hpp"

#include <memory>

int main() {
    // Init
    gw::Package pkg{};
    gw::Config config = gw::loadConfigFromDisk();

    gw::Console console{};
    gw::EntryManager entry_manager{};
    gw::MenuList menu_list{};
    gw::Menu* current_menu{};

    // Allocate
    auto main_menu = std::make_unique<gw::menus::Main>();
    auto edit_entries_menu = std::make_unique<gw::menus::EditEntries>();
    auto settings_menu = std::make_unique<gw::menus::Settings>();

    // Config
    menu_list.main_menu = main_menu.get();
    menu_list.edit_entries_menu = edit_entries_menu.get();
    menu_list.settings_menu = settings_menu.get();

    pkg.entry_manager = &entry_manager;
    pkg.console = &console;
    pkg.cfg = &config;

    entry_manager.cfg = &config;
    entry_manager.prepareAutosave();

    // App loop
    while (pkg.keep_app_running == true) {
        current_menu = loadActiveMenu(pkg, menu_list);

        current_menu->displayOptions(pkg);
        current_menu->requestInput(pkg);
        current_menu->executeSelectedOptAction(pkg);
    }

    return 0;
}