// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Sava Alexandru-Andrei
// License: GNU AGPL v3 or later - see LICENSE file

#pragma once

#include "Core/Message.hpp"
#include "Core/Handler.hpp"

namespace gw::con::msg {
class BlockingMessageTest : public gw::con::Message {
public:
    void Dispatch(Handler& h) noexcept override { h.Handle(*this); };
};
} // namespace gw::con::msg