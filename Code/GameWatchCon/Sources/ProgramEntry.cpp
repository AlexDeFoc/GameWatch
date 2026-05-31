// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Sava Alexandru-Andrei
// License: GNU AGPL v3 or later - see LICENSE file

#include "Pch.hpp"
#include "Core/AppController.hpp"
#include "Core/ThreadPool.hpp"
#include "Core/MessageQueue.hpp"

// Temporary
#include "Messages/Quit.hpp"
#include "Messages/BlockingMessageTest.hpp"


int main() {
    gw::con::AppController appController{};
    gw::con::MessageQueue messageQueue{};
    gw::con::ThreadPool pool(5, appController, messageQueue);
    messageQueue.Push(std::make_unique<gw::con::msg::Quit>());
    messageQueue.Push(std::make_unique<gw::con::msg::BlockingMessageTest>());
}