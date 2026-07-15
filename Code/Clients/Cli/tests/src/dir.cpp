#include "headers/dir.hpp"

TEST(DirTest, IsConstructible) {
  constexpr auto is_constructible = std::is_constructible_v<
    gw::dir, std::string>;

  EXPECT_TRUE(is_constructible);
}

TEST(DirTest, ConstructFromStringLiteral) {
  const gw::dir dir{"folder"};

  EXPECT_EQ(dir.path(), "folder");
}

TEST(DirTest, ConstructFromExistingString) {
  const std::string folder_str{"folder"};

  const gw::dir dir{folder_str};

  EXPECT_EQ(dir.path(), "folder");
}

TEST(DirTest, ConstructFromExistingStringRef) {
  const std::string folder_str{"folder"};

  const gw::dir dir{folder_str};

  EXPECT_EQ(dir.path(), "folder");
}

TEST(DirTest, ConstructFromExistingRValString) {
  std::string folder_str{"folder"};

  const gw::dir dir{std::move(folder_str)};

  EXPECT_EQ(dir.path(), "folder");
}

TEST(DirTest, ExtendDirWithStringLiteral) {
  const auto dir = gw::dir{"folder1"} / "folder2" / "folder3";

  EXPECT_EQ(dir.path(), "folder1/folder2/folder3");
}

TEST(DirTest, ExtendDirWithStringRef) {
  const std::string folder1_str{"folder1"};
  const std::string folder2_str{"folder2"};
  const std::string folder3_str{"folder3"};

  const auto dir = gw::dir{folder1_str} / folder2_str / folder3_str;

  EXPECT_EQ(dir.path(), "folder1/folder2/folder3");
}

TEST(DirTest, ParentPath) {
  const auto dir = gw::dir{"folder1"} / "folder2" / "folder3";

  EXPECT_EQ(dir.parent().path(), "folder1/folder2");

  const auto dir2 = gw::dir{"folder1"};

  EXPECT_EQ(dir2.parent_path(), "");
}