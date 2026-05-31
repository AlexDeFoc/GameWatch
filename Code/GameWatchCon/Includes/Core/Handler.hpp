// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Sava Alexandru-Andrei
// License: GNU AGPL v3 or later - see LICENSE file

#pragma once

namespace gw::con::msg {
class Quit;
class BlockingMessageTest;
} //

namespace gw::con {
class Handler {
public:
    virtual ~Handler() = default;
    virtual void Handle(const msg::Quit&) noexcept = 0;
    virtual void Handle(const msg::BlockingMessageTest&) noexcept = 0;
};
} // namespace gw::con