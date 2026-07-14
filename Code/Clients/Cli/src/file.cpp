#include "core/file.hpp"

auto gw::file::resolve_path(const file_opts &opts) noexcept -> std::string {
  auto should_add_dir{false};
  auto should_add_stem{false};
  auto should_add_ext{false};

  gw::dir const *dir{nullptr};
  std::string const *stem{nullptr};
  std::string const *ext{nullptr};

  if (opts.dir.has_value()) {
    should_add_dir = true;
    dir = &opts.dir.value();
  }

  if (opts.stem.has_value()) {
    should_add_stem = true;
    stem = &opts.stem.value();
  }

  if (opts.ext.has_value()) {
    should_add_ext = true;
    ext = &opts.ext.value();
  }

  try {
    if (should_add_dir) {
      if (should_add_stem) {
        if (should_add_ext) {
          return std::format("{}/{}{}", dir->path(), *stem, *ext);
        }

        return std::format("{}/{}", dir->path(), *stem);
      }

      if (should_add_ext) {
        return std::format("{}/{}", dir->path(), *ext);
      }

      return dir->path();
    }

    if (should_add_stem) {
      if (should_add_ext) {
        return std::format("{}{}", *stem, *ext);
      }

      return *stem;
    }

    if (should_add_ext) {
      return *ext;
    }
  } catch (...) {
    return "";
  }

  return "";
}

gw::file::file(file_opts opts) noexcept : ifile{stream_storage_},
                                          dir_{std::move(opts.dir)},
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