#pragma once

#include "core/dir.hpp"

namespace gw {
struct file_opts {
  std::optional<dir> dir = std::nullopt;
  std::string stem;
  std::optional<std::string> ext = std::nullopt;
};
}