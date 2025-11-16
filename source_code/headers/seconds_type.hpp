#pragma once

#include <chrono>

namespace gw {
using SecondsU64 = std::chrono::duration<std::uint64_t, std::ratio<1>>;
}