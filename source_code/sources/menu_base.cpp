#include "menu_base.hpp"

#include <iostream>
#include <print>
#include <string>

gw::Menu::Menu() noexcept : m_selected_opt_id{0} {}

auto gw::Menu::requestInput() noexcept -> void {
    while (true) {
        std::println("Enter option id: ");

        std::string input{};
        std::getline(std::cin, input);

        try {
            m_selected_opt_id = std::stoi(input);
        } catch (const std::invalid_argument&) {
            std::println("Input is not a number!");
            continue;
        } catch (const std::out_of_range&) {
            std::println("Id is out of range!");
            continue;
        }

        if (getOptionAddr(m_selected_opt_id) == nullptr) {
            std::println("Id is out of range!");
            continue;
        }

        break;
    }
}

auto gw::Menu::executeSelectedOptAction(gw::Package& pkg) noexcept -> void {
    auto* const opt = getOptionAddr(m_selected_opt_id);
    opt->action(pkg);
}
