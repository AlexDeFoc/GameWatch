#pragma once

#include <string>
#include <vector>

namespace gw {
class dir {
  const std::vector<std::string> levels;

  explicit dir(std::vector<std::string> &&existing_levels) noexcept;

public:
  explicit dir(std::string path) noexcept;

  [[nodiscard]] auto path() const noexcept -> std::string;

  [[nodiscard]] auto parent_path() const noexcept -> std::string;

  [[nodiscard]] auto parent() const noexcept -> dir;

  [[nodiscard]] auto operator/(const char *new_level) const noexcept -> dir;

  [[nodiscard]] auto operator/(
      const std::string &new_level) const noexcept -> dir;
};
}