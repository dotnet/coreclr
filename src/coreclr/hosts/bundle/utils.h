// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef UTILS_H
#define UTILS_H

#include "pal.h"

pal::string_t get_directory(const pal::string_t& path);
pal::string_t get_filename(const pal::string_t& path);

#endif
