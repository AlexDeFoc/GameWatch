#include "package.hpp"

gw::Package::Package() noexcept
    : keep_app_running{true}
    , active_menu_id{gw::MenuId::main}
    , entry_manager{nullptr}
    , console{nullptr}
    , cfg{nullptr} {}