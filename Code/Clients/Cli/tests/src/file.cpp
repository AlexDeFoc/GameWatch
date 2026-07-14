#include "headers/file.hpp"

TEST(FileTest, IsConstructible) {
  constexpr auto is_constructible = std::is_constructible_v<
    gw::file, gw::file_opts>;

  EXPECT_TRUE(is_constructible);
}

TEST(FileTest, ConstructFromStringLiterals) {
  const auto file = gw::file{gw::file_opts{
    .dir = gw::dir{"folder1"} / "folder2" / "folder3",
    .stem = "file_name",
    .ext = ".json"
  }};

  EXPECT_EQ(file.path(), "folder1/folder2/folder3/file_name.json");
}