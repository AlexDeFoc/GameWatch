#pragma once

#include <expected>
#include <tuple>

namespace gw {
[[nodiscard]] auto attemptGithubConnection() noexcept -> bool;
[[nodiscard]] auto getLatestReleaseTagFromGithub() noexcept
    -> std::expected<std::tuple<unsigned int, unsigned int, unsigned int>,
                     bool>;
} // namespace gw