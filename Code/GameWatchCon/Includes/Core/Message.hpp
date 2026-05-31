// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Sava Alexandru-Andrei
// License: GNU AGPL v3 or later - see LICENSE file

#pragma once

namespace gw::con {
class Handler;

class Message {
public:
    virtual ~Message() = default;
    virtual void Dispatch(Handler&) noexcept = 0;
};
} // namespace gw::con