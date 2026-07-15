#pragma once

#include <optional>
#include <string>
#include "core/ifile.hpp"
#include "core/dir.hpp"

namespace gw {
class file : public ifile {
  // This owns the lifetime of the stream on the disk
  std::ifstream file_reader_stream_storage_;

  const std::optional<gw::dir> dir_;
  const std::optional<std::string> stem_;
  const std::optional<std::string> ext_;

public:
  explicit file(file_opts opts) noexcept;

  [[nodiscard]] auto dir() const noexcept -> const gw::dir * override;

  [[nodiscard]] auto stem() const noexcept -> const std::string * override;

  [[nodiscard]] auto ext() const noexcept -> const std::string * override;

  [[nodiscard]] auto parent() const noexcept -> const gw::dir * override;

  [[nodiscard]] auto read_contents() noexcept -> std::string override;

  [[nodiscard]] auto
  path() const noexcept -> std::optional<std::string> override;
};
}