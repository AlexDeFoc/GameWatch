#pragma once

namespace gw {
enum class PrintTag { Error, Request, Tip, Info };

inline static constexpr const char* error_tag{"[ERROR]"};
inline static constexpr const char* request_tag{"[REQUEST]"};
inline static constexpr const char* tip_tag{"[TIP]"};
inline static constexpr const char* info_tag{"[INFO]"};

inline static constexpr const char* error_tag_color{"\x1b[38;2;173;21;23m"};
inline static constexpr const char* request_tag_color{"\x1b[38;2;180;0;255m"};
inline static constexpr const char* tip_tag_color{"\x1b[38;2;0;200;255m"};
inline static constexpr const char* info_tag_color{"\x1b[38;2;255;255;255m"};
inline static constexpr const char* reset_color{"\x1b[0m"};
} // namespace gw