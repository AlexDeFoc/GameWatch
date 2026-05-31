// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Sava Alexandru-Andrei
// License: GNU AGPL v3 or later - see LICENSE file

#pragma once

#include <memory>
#include <condition_variable>

namespace gw::con {
class Message;

class MessageQueue {
public:

    /*! @brief Pushes message in queue, after which it alerts all waiting threads
     *  @param msg
     */
    void Push(std::unique_ptr<Message> msg) noexcept;

    /*! @brief Used by threads to get the next message from the queue
    * @param stk Token used as additional wait to unlock the thread from waiting for the next msg, thus letting it stop
     */
    std::unique_ptr<Message> Wait(const std::stop_token& stk) noexcept;

    /*! @brief Used only by Threadpool to wake all threads. E.g: when a quit msg is in queue.
    */
    void WakeAll() noexcept;

private:
    std::queue<std::unique_ptr<Message>> _queue;
    std::condition_variable _cv;
    std::mutex _mtx;
};
} // namespace gw::con