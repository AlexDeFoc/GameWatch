#pragma once

#include <optional>
#include <string>
#include "core/dir.hpp"

namespace gw {
struct file_opts {
  std::optional<dir> dir = std::nullopt;
  std::optional<std::string> stem = std::nullopt;
  std::optional<std::string> ext = std::nullopt;
};
}