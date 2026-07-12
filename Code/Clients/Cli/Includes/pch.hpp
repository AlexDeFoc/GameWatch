#ifndef _GW_CLIENT_CLI_PCH_HPP_
#define _GW_CLIENT_CLI_PCH_HPP_

// 1. Save the current warning state
#if defined(_MSC_VER)
    #pragma warning(push, 0) // Push and set warning level to 0 (turns off all warnings)
#elif defined(__clang__)
    #pragma clang diagnostics push
    #pragma clang diagnostics ignored "-Wall"
    #pragma clang diagnostics ignored "-Wextra"
#elif defined(__GNUC__)
    #pragma GCC diagnostics push
    #pragma GCC diagnostics ignored "-Wall"
    #pragma GCC diagnostics ignored "-Wextra"
#endif

// ==========================================
// --- NO WARNING ZONE ---
#include <array>
#include <cassert>
#include <cstdint>
#include <print>
#include <string>
#include <string_view>
#include <vector>
// ==========================================

// 2. Restore the original warning state
#if defined(_MSC_VER)
    #pragma warning(pop)
#elif defined(__clang__)
    #pragma clang diagnostics pop
#elif defined(__GNUC__)
    #pragma GCC diagnostics pop
#endif

#endif