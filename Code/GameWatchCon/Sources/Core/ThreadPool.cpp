// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Sava Alexandru-Andrei
// License: GNU AGPL v3 or later - see LICENSE file

#include "Pch.hpp"
#include "Core/ThreadPool.hpp"
#include "Core/AppController.hpp"
#include "Core/MessageQueue.hpp"
#include "Messages/Quit.hpp"

gw::con::ThreadPool::ThreadPool(int threadCount, AppController& appController, MessageQueue& messageQueue) noexcept {
    auto sharedSTk = _sharedStopTokenSource.get_token();
    
    for (int i{0}; i < threadCount; ++i)
        _threads.emplace_back([this, &appController, &messageQueue, sharedSTk]() {
            while (true) {
                auto msg = messageQueue.Wait(sharedSTk);

                if (!msg)
                    break;

                if (auto* quitMsg = dynamic_cast<gw::con::msg::Quit*>(msg.get())) {
                    _sharedStopTokenSource.request_stop();
                    messageQueue.WakeAll();
                }
                else
                    msg->Dispatch(appController);
            }
        });
}