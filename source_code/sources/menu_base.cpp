#include "menu_base.hpp"

#include <iostream>
#include <print>
#include <string>

gw::Menu::Menu() noexcept : m_selected_opt_id{0} {}

auto gw::Menu::requestInput(const Package& pkg) noexcept -> void {
    while (true) {
        pkg.console->print(gw::PrintTag::Request,
                           gw::print_msg::enter_option_id);

        std::string input{};
        std::getline(std::cin, input);

        try {
            m_selected_opt_id = std::stoi(input);
        } catch (const std::invalid_argument&) {
            pkg.console->clear();
            pkg.console->println(gw::PrintTag::Error,
                                 gw::print_msg::input_is_not_a_number);

            this->displayOptions(pkg);
            continue;
        } catch (const std::out_of_range&) {
            pkg.console->clear();
            pkg.console->println(gw::PrintTag::Error,
                                 gw::print_msg::id_is_out_of_range);

            this->displayOptions(pkg);
            continue;
        }

        if (getOptionAddr(m_selected_opt_id) == nullptr) {
            pkg.console->clear();
            pkg.console->println(gw::PrintTag::Error,
                                 gw::print_msg::id_is_out_of_range);

            this->displayOptions(pkg);
            continue;
        }

        break;
    }
}

auto gw::Menu::executeSelectedOptAction(gw::Package& pkg) noexcept -> void {
    auto* const opt = getOptionAddr(m_selected_opt_id);
    opt->action(pkg);
}
