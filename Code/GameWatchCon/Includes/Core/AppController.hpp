// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Sava Alexandru-Andrei
// License: GNU AGPL v3 or later - see LICENSE file

#pragma once

#include "Core/Handler.hpp"

namespace gw::con {
class AppController : public Handler {
public:
    void Handle(const msg::Quit&) noexcept override;
    void Handle(const msg::BlockingMessageTest&) noexcept override;
};
} // namespace gw::con