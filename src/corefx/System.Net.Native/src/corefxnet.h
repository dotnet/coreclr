//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

/*++

Module Name:

    pal_corefxnet.h

Abstract:

    Header file for functions meant to be consumed by the CoreFX Net libraries.

--*/

#ifndef __PAL_COREFX_NET_H__
#define __PAL_COREFX_NET_H__

#ifdef  __cplusplus
extern "C" {
#endif

#define COREFX_NET_SUCCESS 0x00000000
#define COREFX_NET_INVALID_PARAM 0xc000000d

  
  struct IPAddress {
    unsigned char isIPV4;
    int port;
    unsigned int scopeId;
    unsigned char bytes[16];
  };

  PALIMPORT
  int
  GetIPAddresses(const char* hostName, // host name
		 char **canonicalName, //hostname
		 struct IPAddress **result, //array of ipAddresses
		 int *addressCount); // number of addresses returned
  
#ifdef  __cplusplus
} // extern "C"
#endif

#endif // __PAL_COREFX_NET_H__
