// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
#pragma once
// This file allows customization of the strongname key used to replace the ECMA key

static const BYTE g_rbTheKey[] = 
{
0x00,0x24,0x00,0x00,0x04,0x80,0x00,0x00,0x94,0x00,0x00,0x00,0x06,0x02,0x00,0x00,
0x00,0x24,0x00,0x00,0x52,0x53,0x41,0x31,0x00,0x04,0x00,0x00,0x01,0x00,0x01,0x00,
0x07,0xd1,0xfa,0x57,0xc4,0xae,0xd9,0xf0,0xa3,0x2e,0x84,0xaa,0x0f,0xae,0xfd,0x0d,
0xe9,0xe8,0xfd,0x6a,0xec,0x8f,0x87,0xfb,0x03,0x76,0x6c,0x83,0x4c,0x99,0x92,0x1e,
0xb2,0x3b,0xe7,0x9a,0xd9,0xd5,0xdc,0xc1,0xdd,0x9a,0xd2,0x36,0x13,0x21,0x02,0x90,
0x0b,0x72,0x3c,0xf9,0x80,0x95,0x7f,0xc4,0xe1,0x77,0x10,0x8f,0xc6,0x07,0x77,0x4f,
0x29,0xe8,0x32,0x0e,0x92,0xea,0x05,0xec,0xe4,0xe8,0x21,0xc0,0xa5,0xef,0xe8,0xf1,
0x64,0x5c,0x4c,0x0c,0x93,0xc1,0xab,0x99,0x28,0x5d,0x62,0x2c,0xaa,0x65,0x2c,0x1d,
0xfa,0xd6,0x3d,0x74,0x5d,0x6f,0x2d,0xe5,0xf1,0x7e,0x5e,0xaf,0x0f,0xc4,0x96,0x3d,
0x26,0x1c,0x8a,0x12,0x43,0x65,0x18,0x20,0x6d,0xc0,0x93,0x34,0x4d,0x5a,0xd2,0x93
};

static const BYTE g_rbTheKeyToken[] = {0xb0,0x3f,0x5f,0x7f,0x11,0xd5,0x0a,0x3a};


static const BYTE g_rbTheSilverlightPlatformKey[] = 
{
0x00,0x24,0x00,0x00,0x04,0x80,0x00,0x00,0x94,0x00,0x00,0x00,0x06,0x02,0x00,0x00,
0x00,0x24,0x00,0x00,0x52,0x53,0x41,0x31,0x00,0x04,0x00,0x00,0x01,0x00,0x01,0x00,
0x8d,0x56,0xc7,0x6f,0x9e,0x86,0x49,0x38,0x30,0x49,0xf3,0x83,0xc4,0x4b,0xe0,0xec,
0x20,0x41,0x81,0x82,0x2a,0x6c,0x31,0xcf,0x5e,0xb7,0xef,0x48,0x69,0x44,0xd0,0x32,
0x18,0x8e,0xa1,0xd3,0x92,0x07,0x63,0x71,0x2c,0xcb,0x12,0xd7,0x5f,0xb7,0x7e,0x98,
0x11,0x14,0x9e,0x61,0x48,0xe5,0xd3,0x2f,0xba,0xab,0x37,0x61,0x1c,0x18,0x78,0xdd,
0xc1,0x9e,0x20,0xef,0x13,0x5d,0x0c,0xb2,0xcf,0xf2,0xbf,0xec,0x3d,0x11,0x58,0x10,
0xc3,0xd9,0x06,0x96,0x38,0xfe,0x4b,0xe2,0x15,0xdb,0xf7,0x95,0x86,0x19,0x20,0xe5,
0xab,0x6f,0x7d,0xb2,0xe2,0xce,0xef,0x13,0x6a,0xc2,0x3d,0x5d,0xd2,0xbf,0x03,0x17,
0x00,0xae,0xc2,0x32,0xf6,0xc6,0xb1,0xc7,0x85,0xb4,0x30,0x5c,0x12,0x3b,0x37,0xab 
};

static const BYTE g_rbTheSilverlightPlatformKeyToken[] = {0x7c,0xec,0x85,0xd7,0xbe,0xa7,0x79,0x8e};

static const BYTE g_rbTheSilverlightKey[] = 
{
0x00,0x24,0x00,0x00,0x04,0x80,0x00,0x00,0x94,0x00,0x00,0x00,0x06,0x02,0x00,0x00,
0x00,0x24,0x00,0x00,0x52,0x53,0x41,0x31,0x00,0x04,0x00,0x00,0x01,0x00,0x01,0x00,
0xb5,0xfc,0x90,0xe7,0x02,0x7f,0x67,0x87,0x1e,0x77,0x3a,0x8f,0xde,0x89,0x38,0xc8,
0x1d,0xd4,0x02,0xba,0x65,0xb9,0x20,0x1d,0x60,0x59,0x3e,0x96,0xc4,0x92,0x65,0x1e,
0x88,0x9c,0xc1,0x3f,0x14,0x15,0xeb,0xb5,0x3f,0xac,0x11,0x31,0xae,0x0b,0xd3,0x33,
0xc5,0xee,0x60,0x21,0x67,0x2d,0x97,0x18,0xea,0x31,0xa8,0xae,0xbd,0x0d,0xa0,0x07,
0x2f,0x25,0xd8,0x7d,0xba,0x6f,0xc9,0x0f,0xfd,0x59,0x8e,0xd4,0xda,0x35,0xe4,0x4c,
0x39,0x8c,0x45,0x43,0x07,0xe8,0xe3,0x3b,0x84,0x26,0x14,0x3d,0xae,0xc9,0xf5,0x96,
0x83,0x6f,0x97,0xc8,0xf7,0x47,0x50,0xe5,0x97,0x5c,0x64,0xe2,0x18,0x9f,0x45,0xde,
0xf4,0x6b,0x2a,0x2b,0x12,0x47,0xad,0xc3,0x65,0x2b,0xf5,0xc3,0x08,0x05,0x5d,0xa9
};

static const BYTE g_rbTheSilverlightKeyToken[] = {0x31,0xBF,0x38,0x56,0xAD,0x36,0x4E,0x35};

#ifdef FEATURE_WINDOWSPHONE

static const BYTE g_rbTheMicrosoftPhoneKey[] = 
{
0x00,0x24,0x00,0x00,0x04,0x80,0x00,0x00,0x14,0x01,0x00,0x00,0x06,0x02,0x00,0x00,
0x00,0x24,0x00,0x00,0x52,0x53,0x41,0x31,0x00,0x08,0x00,0x00,0x01,0x00,0x01,0x00,
0xCB,0x8F,0x76,0xDA,0xE0,0x5D,0x35,0x2C,0x11,0x7A,0x84,0x67,0xEF,0x32,0x60,0xE0,
0x02,0x10,0x8A,0xA9,0xE7,0x09,0x56,0xE7,0x05,0xF6,0x43,0x0F,0x6C,0xBC,0xFE,0x35,
0x9D,0x02,0x11,0x75,0x63,0x40,0x00,0x17,0xD1,0x00,0x4D,0x68,0xD1,0x3F,0xE5,0x74,
0xA2,0x56,0x49,0x58,0x0E,0xD1,0xEE,0x22,0x5A,0xA4,0xBB,0xC0,0x93,0x06,0x2D,0xA0,
0xB1,0x68,0xBB,0xEA,0x3F,0x57,0x95,0x50,0x8F,0xCB,0xD7,0x53,0x13,0x74,0xC7,0x45,
0xCE,0x63,0x27,0xA9,0x1A,0xC7,0x24,0x35,0x71,0xF2,0xB4,0xE8,0xE4,0xFC,0x24,0x8C,
0xA1,0xA2,0xF3,0xC8,0x73,0xE1,0x49,0xB3,0x43,0x0B,0x37,0xBD,0x51,0xA6,0xA3,0xAD,
0xC9,0x5F,0x68,0x5C,0x91,0x74,0x57,0x69,0x13,0x0F,0xA1,0x0F,0xBF,0xC7,0xB2,0x28,
0x2F,0x25,0xDF,0xFD,0x4B,0xA2,0xC0,0xBD,0x7C,0x82,0x88,0xCD,0xE4,0x80,0x6C,0x8B,
0xDC,0x81,0x7D,0x12,0x6C,0x7E,0x54,0x36,0x10,0xD3,0x47,0x6A,0x63,0x17,0xB5,0xD5,
0x48,0x76,0xA0,0xE3,0xB3,0xAD,0x42,0xB1,0x2C,0x51,0x38,0xDB,0x2F,0xFB,0x66,0x9F,
0x1B,0xE6,0x31,0x4B,0xC9,0x58,0x7A,0xC1,0xAC,0xE4,0x75,0x1D,0x6C,0x58,0x84,0xDB,
0x04,0x81,0x15,0x90,0x7C,0x09,0x0D,0xBB,0xDF,0x98,0xC6,0x5E,0xD4,0xC0,0x42,0xC3,
0x77,0x5A,0xCA,0xA7,0xD6,0x04,0x23,0x07,0xFA,0xE2,0xAA,0xFA,0xE7,0x12,0x8E,0x17,
0x65,0x81,0x92,0x44,0x6C,0x07,0x40,0x0D,0x22,0xC5,0xBA,0x43,0xF3,0xA1,0xE7,0xA3,
0xE5,0xAF,0x81,0xBE,0x0E,0x65,0xA5,0x01,0x6C,0x5F,0x0C,0x62,0xA3,0xC0,0xDE,0xB3 
};

static const BYTE g_rbTheMicrosoftPhoneKeyToken[] = {0x24,0xEE,0xC0,0xD8,0xC8,0x6C,0xDA,0x1E};


static const BYTE g_rbTheMicrosoftXNAKey[] =
{
0x00,0x24,0x00,0x00,0x04,0x80,0x00,0x00,0x94,0x00,0x00,0x00,0x06,0x02,0x00,0x00,
0x00,0x24,0x00,0x00,0x52,0x53,0x41,0x31,0x00,0x04,0x00,0x00,0x01,0x00,0x01,0x00,
0x0F,0xC5,0x99,0x3E,0x0F,0x51,0x1A,0xD5,0xE1,0x6E,0x8B,0x22,0x65,0x53,0x49,0x3E,
0x09,0x06,0x7A,0xFC,0x41,0x03,0x9F,0x70,0xDA,0xEB,0x94,0xA9,0x68,0xD6,0x64,0xF4,
0x0E,0x69,0xA4,0x6B,0x61,0x7D,0x15,0xD3,0xD5,0x32,0x8B,0xE7,0xDB,0xED,0xD0,0x59,
0xEB,0x98,0x49,0x5A,0x3B,0x03,0xCB,0x4E,0xA4,0xBA,0x12,0x74,0x44,0x67,0x1C,0x3C,
0x84,0xCB,0xC1,0xFD,0xC3,0x93,0xD7,0xE1,0x0B,0x5E,0xE3,0xF3,0x1F,0x5A,0x29,0xF0,
0x05,0xE5,0xEE,0xD7,0xE3,0xC9,0xC8,0xAF,0x74,0xF4,0x13,0xF0,0x00,0x4F,0x0C,0x2C,
0xAB,0xB2,0x2F,0x9D,0xD4,0xF7,0x5A,0x6F,0x59,0x97,0x84,0xE1,0xBA,0xB7,0x09,0x85,
0xEF,0x81,0x74,0xCA,0x6C,0x68,0x42,0x78,0xBE,0x82,0xCE,0x05,0x5A,0x03,0xEB,0xAF
};

static const BYTE g_rbTheMicrosoftXNAKeyToken[] = {0xCC,0xAC,0x92,0xED,0x87,0x3B,0x18,0x5C};

#endif
// for FEATURE_WINDOWSPHONE, we can add the Microsoft.Phone key and the Xna key to the list of blessed keys...


