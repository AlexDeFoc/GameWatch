#include <gtest/gtest.h>
#include "ExampleLogic/a.hpp"

TEST(CliTest, DummyAssert) { EXPECT_EQ(1, 1); }

TEST(ClionTest, CheckCoreLibExists) {
    auto val = gw::example_logic::get_num1();
    EXPECT_EQ(val, 100);
}