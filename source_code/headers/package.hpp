#pragma once

#include "config.hpp"
#include "console.hpp"
#include "entry_manager.hpp"
#include "menu_id.hpp"

namespace gw {
class Package {
  public:
    bool keep_app_running;
    gw::MenuId active_menu_id;
    EntryManager* entry_manager;
    Console* console;
    Config* cfg;

    Package() noexcept;
};
} // namespace gw