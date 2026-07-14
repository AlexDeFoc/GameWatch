#include "core/file.hpp"

gw::file::file(file_opts opts) noexcept : dir_{std::move(opts.dir)},
                                                stem_{std::move(opts.stem)},
                                                ext_{std::move(opts.ext)} {
}

auto gw::file::path() const noexcept -> std::string {
  std::string result;

  if (dir_.has_value()) {
    result += dir_.value().path();
  }

  result += '/';
  result += stem_;

  if (ext_.has_value()) {
    result += ext_.value();
  }

  return result;
}