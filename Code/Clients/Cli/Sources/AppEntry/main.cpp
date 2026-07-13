#include "ExampleLogic/a.hpp"

auto main() -> int
{
    constexpr static int sub_val = 100;
    return gw::example_logic::get_num1() - sub_val;
}