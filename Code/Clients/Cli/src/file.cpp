#include "core/file.hpp"

gw::file::file(file_opts opts) noexcept : ifile{file_reader_stream_storage_},
                                          dir_{std::move(opts.dir)},
                                          stem_{std::move(opts.stem)},
                                          ext_{std::move(opts.ext)} {
}

auto gw::file::dir() const noexcept -> const gw::dir * {
  try {
    return dir_.has_value() ? &dir_.value() : nullptr;
  } catch (...) {
    return nullptr;
  }
}

auto gw::file::stem() const noexcept -> const std::string * {
  try {
    return stem_.has_value() ? &stem_.value() : nullptr;
  } catch (...) {
    return nullptr;
  }
}

auto gw::file::ext() const noexcept -> const std::string * {
  try {
    return ext_.has_value() ? &ext_.value() : nullptr;
  } catch (...) {
    return nullptr;
  }
}

auto gw::file::parent() const noexcept -> const gw::dir * {
  return this->dir();
}

auto gw::file::path() const noexcept -> std::optional<std::string> {
  auto should_add_dir{false};
  auto should_add_stem{false};
  auto should_add_ext{false};

  if (dir_.has_value()) {
    should_add_dir = true;
  }

  if (stem_.has_value()) {
    should_add_stem = true;
  }

  if (ext_.has_value()) {
    should_add_ext = true;
  }

  try {
    if (should_add_dir) {
      if (should_add_stem) {
        if (should_add_ext) {
          return std::format("{}/{}{}", dir_->path(), stem_.value(),
                             ext_.value());
        }

        return std::format("{}/{}", dir_->path(), stem_.value());
      }

      if (should_add_ext) {
        return std::format("{}/{}", dir_->path(), ext_.value());
      }

      return dir_->path();
    }

    if (should_add_stem) {
      if (should_add_ext) {
        return std::format("{}{}", stem_.value(), ext_.value());
      }

      return stem_;
    }

    if (should_add_ext) {
      return ext_;
    }
  } catch (...) {
    return std::nullopt;
  }

  return std::nullopt;
}

auto gw::file::read_contents() noexcept -> std::string {

  const auto filepath_option = this->path();
  std::string filepath;

  if (filepath_option.has_value()) {
    filepath = filepath_option.value();
  }
  else {
    assert(false && "filepath is empty!");
  }

  file_reader_stream_storage_.open(filepath);

  auto file_contents = ifile::read_contents();

  file_reader_stream_storage_.close();

  return file_contents;
}