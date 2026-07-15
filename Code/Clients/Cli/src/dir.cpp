#include "core/dir.hpp"

gw::dir::dir(std::vector<std::string> &&existing_levels) noexcept : levels(
    std::move(existing_levels)) {
}

gw::dir::dir(std::string path) noexcept : levels{std::move(path)} {
}

auto gw::dir::path() const noexcept -> std::string {
  try {
    return levels | std::views::join_with('/') | std::ranges::to<std::string>();
  } catch (...) {
    return {};
  }
}

auto gw::dir::parent_path() const noexcept -> std::string {
  if (levels.size() - 1 == 0) {
    return {};
  }

  return levels
         | std::views::take(levels.size() - 1)
         | std::views::join
         | std::ranges::to<std::string>();
}

auto gw::dir::parent() const noexcept -> dir {
  if (levels.size() - 1 == 0) {
    return dir{std::string{}};
  }

  auto new_levels = levels
                    | std::views::take(levels.size() - 1)
                    | std::ranges::to<std::vector<std::string> >();

  return dir{std::move(new_levels)};
}

auto gw::dir::operator/(const char *new_level) const noexcept -> dir {
  auto levels_cpy = levels;
  levels_cpy.emplace_back(new_level);
  return dir{std::move(levels_cpy)};
}

auto gw::dir::operator/(const std::string &new_level) const noexcept -> dir {
  auto levels_cpy = this->levels;
  levels_cpy.push_back(new_level);
  return dir{std::move(levels_cpy)};
}