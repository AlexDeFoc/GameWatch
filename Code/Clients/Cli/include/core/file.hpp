#pragma once

#include "core/file_opts.hpp"

namespace gw {
class file {
  const std::optional<dir> dir_;
  const std::string stem_;
  const std::optional<std::string> ext_;

  // Private static helper to extract the path
  [[nodiscard]] static auto resolve_path(
      const file_opts &opts) noexcept -> std::string;

public:
  explicit file(file_opts opts) noexcept;

  [[nodiscard]] auto path() const noexcept -> std::string;
};
}