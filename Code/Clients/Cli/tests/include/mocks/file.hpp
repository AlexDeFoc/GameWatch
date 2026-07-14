#pragma once

#include "core/ifile.hpp"

namespace gw {
class mock_file : public ifile {
  // This owns the lifetime of the stream in memory
  std::stringstream mock_stream_storage_;

public:
  mock_file() noexcept : ifile{mock_stream_storage_} {
  }

  void set_mock_contents(std::string contents) noexcept {
    mock_stream_storage_.str(std::move(contents));
    mock_stream_storage_.clear(); // Reset error state flags (like EOF)
  }

  auto dir() const noexcept -> const gw::dir * override { return {}; }

  auto stem() const noexcept -> const std::string * override { return {}; }

  auto ext() const noexcept -> const std::string * override { return {}; }

  auto parent() const noexcept -> const gw::dir * override { return {}; }

  auto path() const noexcept -> std::optional<std::string> override {
    return {};
  }
};
}