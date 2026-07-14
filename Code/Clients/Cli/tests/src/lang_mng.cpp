#include "headers/lang_mng.hpp"

TEST(LangMngTest, CanBeCreatedOnlyOnce) {
  EXPECT_TRUE(std::is_constructible_v<gw::lang_mng>);

  gw::lang_mng();

  EXPECT_DEATH(gw::lang_mng(),
               "Cannot create multiple instances");
}