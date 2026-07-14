#include "headers/file.hpp"

TEST(FileTest, IsConstructible) {
  constexpr auto is_constructible = std::is_constructible_v<
    gw::file, gw::file_opts>;

  EXPECT_TRUE(is_constructible);
}

TEST(FileTest, GetDiffCobinationsOfPath) {
  const auto f1 = gw::file{{}};
  const auto f2 = gw::file{{.dir = gw::dir{"folder1"} / "folder2"}};
  const auto f3 = gw::file{{.ext = ".json"}};
  const auto f4 = gw::file{{.stem = "file_name"}};
  const auto f5 = gw::file{{.stem = "file_name", .ext = ".json"}};
  const auto f6 = gw::file{{.dir = gw::dir{"folder1"}, .ext = ".json"}};
  const auto f7 = gw::file{{.dir = gw::dir{"folder1"}, .stem = "file_name"}};
  const auto f8 = gw::file{
      {.dir = gw::dir{"folder1"}, .stem = "file_name", .ext = ".json"}};

  EXPECT_EQ(f1.path(), std::nullopt);
  EXPECT_EQ(f2.path(), "folder1/folder2");
  EXPECT_EQ(f3.path(), ".json");
  EXPECT_EQ(f4.path(), "file_name");
  EXPECT_EQ(f5.path(), "file_name.json");
  EXPECT_EQ(f6.path(), "folder1/.json");
  EXPECT_EQ(f7.path(), "folder1/file_name");
  EXPECT_EQ(f8.path(), "folder1/file_name.json");
}

TEST(FileTest, GetAllFilledComponents) {
  const auto file = gw::file{gw::file_opts{
      .dir = gw::dir{"folder1"} / "folder2" / "folder3",
      .stem = "file_name",
      .ext = ".json"
  }};

  EXPECT_EQ(file.path(), "folder1/folder2/folder3/file_name.json");
  EXPECT_EQ(file.parent()->path(), "folder1/folder2/folder3");
  EXPECT_EQ(file.dir()->path(), "folder1/folder2/folder3");
  EXPECT_EQ(*file.stem(), "file_name");
  EXPECT_EQ(*file.ext(), ".json");
}

TEST(FileTest, GetAllEmptyComponents) {
  const auto file = gw::file{{}};

  EXPECT_EQ(file.path(), std::nullopt);
  EXPECT_EQ(file.parent(), nullptr);
  EXPECT_EQ(file.dir(), nullptr);
  EXPECT_EQ(file.stem(), nullptr);
  EXPECT_EQ(file.ext(), nullptr);
}

TEST(FileMockTest, LoadTextFromDisk) {
  gw::mock_file file;

  file.set_mock_contents("Hello from file contents!");

  const auto contents = file.read_contents();

  EXPECT_EQ(contents, "Hello from file contents!");
}