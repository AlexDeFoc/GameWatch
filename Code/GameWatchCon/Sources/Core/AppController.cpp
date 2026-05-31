// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Sava Alexandru-Andrei
// License: GNU AGPL v3 or later - see LICENSE file

#include "Pch.hpp"
#include "Core/AppController.hpp"

// Messages
#include "Messages/Quit.hpp"
#include "Messages/BlockingMessageTest.hpp"
// Messages

void gw::con::AppController::Handle(const msg::Quit&) noexcept {}
void gw::con::AppController::Handle(const msg::BlockingMessageTest&) noexcept {
    std::string _{};
    std::getline(std::cin, _);
}