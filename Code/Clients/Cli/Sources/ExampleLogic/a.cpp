#include "ExampleLogic/a.hpp"

auto gw::example_logic::get_num1() noexcept -> int {
    static constexpr int val = 100;
    return val;
}

auto gw::example_logic::news_tuff() noexcept -> void {
    }

auto gw::example_logic::about_done() noexcept -> void {
}