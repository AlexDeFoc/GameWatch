// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Sava Alexandru-Andrei
// License: GNU AGPL v3 or later - see LICENSE file

#include "Core/Message.hpp"
#include "Core/MessageQueue.hpp"

void gw::con::MessageQueue::Push(std::unique_ptr<Message> msg) noexcept {
    std::lock_guard lck{_mtx};

    _queue.push(std::move(msg));

    _cv.notify_all();
}

std::unique_ptr<gw::con::Message> gw::con::MessageQueue::Wait(const std::stop_token& stk) noexcept {
    std::unique_lock lck{_mtx};
    
    _cv.wait(lck, [this, &stk] { return !_queue.empty() || stk.stop_requested(); });

    if (!_queue.empty()) {
        std::unique_ptr<gw::con::Message> msg = std::move(_queue.front());
        _queue.pop();
        return msg;
    }

    return nullptr;
}

void gw::con::MessageQueue::WakeAll() noexcept
{
    _cv.notify_all();
}