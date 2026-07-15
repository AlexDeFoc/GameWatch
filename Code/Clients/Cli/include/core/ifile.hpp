#pragma once

#include <string>
#include <optional>
#include "core/file_opts.hpp"

namespace gw {
class ifile {
protected:
  std::istream &file_reader_stream;

  explicit ifile(std::istream &file_reader_stream) noexcept;

public:
  [[nodiscard]] auto virtual dir() const noexcept -> const dir * = 0;

  [[nodiscard]] auto virtual stem() const noexcept -> const std::string * = 0;

  [[nodiscard]] auto virtual ext() const noexcept -> const std::string * = 0;

  [[nodiscard]] auto virtual parent() const noexcept -> const gw::dir * = 0;

  [[nodiscard]] auto virtual
  path() const noexcept -> std::optional<std::string> = 0;

  [[nodiscard]] auto virtual read_contents() noexcept -> std::string;

  virtual ~ifile() = default;
};
}