// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Sava Alexandru-Andrei
// License: GNU AGPL v3 or later - see LICENSE file

#pragma once

#include <vector>
#include <thread>

namespace gw::con {
class AppController;
class MessageQueue;

class ThreadPool {
public:
    /*! @brief Constructs thread pool with given number of threads, after which passes messageQueue and appController to worker threads
     *  @param threadCount Amount of threads to have in pool
     *  @param appController
     *  @param messageQueue
     */
    explicit ThreadPool(int threadCount, AppController& appController, MessageQueue& messageQueue) noexcept;
private:
    std::vector<std::jthread> _threads;
    std::stop_source _sharedStopTokenSource;
};
} // namespace gw::con