// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2026 Sava Alexandru-Andrei
// License: GNU AGPL v3 or later - see LICENSE file

#pragma once

#if defined(_WIN32)
    #if !defined(UNICODE)
        #define UNICODE
    #endif

    #if !defined(_UNICODE)
        #define _UNICODE
    #endif

    #if !defined(WIN32_LEAN_AND_MEAN)
        #define WIN32_LEAN_AND_MEAN
    #endif

    #if !defined(NOGDI)
        #define NOGDI
    #endif

    #if !defined(NOMINMAX)
        #define NOMINMAX
    #endif

    #if !defined(INCLUDED_WINDOWS_HEADER)
        #define INCLUDED_WINDOWS_HEADER
        #include <Windows.h>
    #endif
#elif defined(__linux__)
    #include <termios.h> // for tcgetattr, termios, ECHO, ICANON, etc.
    #include <unistd.h> // readlink
#elif defined(__APPLE__)
    #include <termios.h> // for tcgetattr, termios, ECHO, ICANON, etc.
    #include <unistd.h> // getpid (readlink not needed here)
    #include <libproc.h> // proc_pidpath, PROC_PIDPATHINFO_MAXSIZE
#endif