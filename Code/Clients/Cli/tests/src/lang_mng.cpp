#include "headers/lang_mng.hpp"

TEST(LangMngTest, ClassCannotBeCreatedWithConstructor) {
  EXPECT_FALSE(std::is_constructible_v<lang_mng>);
}