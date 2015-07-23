//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

/*++

  Module Name:
  
  corenet.cpp

  Abstract:

  Implementation of Networking APIs meant to be consumed by CoreFX (net) libraries.

  The API present here are meant to be wrappers around networking API of the underlying OS such that,
  (a) the arguments passed are Marshaller-friendly (eg: getaddrinfo involves complex structures (a linked list of addrinfo), while
  the wrappers takes simple datatypes and returns an array of relatively simple structure)
  (b) there is less dependency on platform specific constants like AF_INET
  --*/

#include "pal/palinternal.h"
#include "pal/dbgmsg.h"
#include "pal/module.h"
#include <corefxnet.h>

#include <sys/types.h>
#include <sys/stat.h>
#include <errno.h>
#include <unistd.h>
#include <dlfcn.h>

#ifdef __APPLE__
#include <mach-o/dyld.h>
#endif // __APPLE__

#ifdef PLATFORM_UNIX
#include <sys/types.h>
#include <sys/socket.h>
#include <netdb.h>
#include <stdlib.h>
#endif // PLATFORM_UNIX

SET_DEFAULT_DEBUG_CHANNEL(MISC);

static int
addrInfoNodeToIPAddress(
                        struct addrinfo *addrInfo,
                        struct IPAddress *ipAddresses,
                        int index)
{
    struct IPAddress ipAddress = ipAddresses[index];
  
    if (addrInfo == NULL) {
        return 0;
    }
    if (AF_INET == addrInfo->ai_family) {
        struct sockaddr_in *ip4Addr = NULL;
        long l;

        ip4Addr = (struct sockaddr_in *)(addrInfo->ai_addr);
        ipAddress.isIPV4 = 1;
        ipAddress.port = ip4Addr->sin_port;
        ipAddress.scopeId = 0;
        l = ip4Addr->sin_addr.s_addr;    
        (ipAddress.bytes)[0] = (l >> 24) & 0xFF;
        (ipAddress.bytes)[1] = (l >> 16) & 0xFF;
        (ipAddress.bytes)[2] = (l >> 8) & 0xFF;
        (ipAddress.bytes)[3] = l & 0xFF;
    }
    else if (AF_INET6 == addrInfo->ai_family) {
        struct sockaddr_in6 *ip6Addr = NULL;
        ip6Addr = (struct sockaddr_in6 *)(addrInfo->ai_addr);
        ipAddress.isIPV4 = 0;
        ipAddress.port = ip6Addr->sin6_port;
        ipAddress.scopeId = ip6Addr->sin6_scope_id;
        memcpy(ipAddress.bytes, &(ip6Addr->sin6_addr), 16); 
    }
    else {
        return 0;
    }
    return 1;
}

/*++
  Function:
  GetIPAddresses

  Used by System.Net.Dns to resolve hostname

  This function invokes getaddrinfo and traverses the resultant list of addrinfo to create an array of IPAddresses.

  The function returns COREFX_NET_SUCCESS in case of success and COREFX_NET_INVALID_PARAM in case of failure.
  It sets:
  the out-parameter, canonicalName, to the value of ai_canonname.
  the out-parameter, result, to the array of created ip addresses
  the addressCount to the number of entries in the array of returning addresses.
  --*/

int
PALAPI
GetIPAddresses(const char* hostName, // host name
               char **canonicalName, //canonicalName 
               struct IPAddress **result,// array of ipAddresses
               int *addressCount) // number of addresses returned
{
    int success = COREFX_NET_SUCCESS;
    struct addrinfo *addrInfo = NULL;
    struct addrinfo *node = NULL;
    struct addrinfo hints;
    int err;
    int index;

    PERF_ENTRY(GetIPAddresses);
    ENTRY("GetIPAddresses (hostName=%p (%s))\n",
          hostName, hostName ? hostName : "NULL");

    // Validate arguments
    if (NULL == hostName) {
        ASSERT("hostName should not be NULL\n");
        success = COREFX_NET_INVALID_PARAM;
        goto done;
    }

    hints.ai_family = AF_UNSPEC; // IPV6 and IPV4
    hints.ai_socktype = 0;
    hints.ai_protocol = 0;
    hints.ai_flags = AI_CANONNAME;
    err = getaddrinfo(hostName, NULL, &hints, &addrInfo);

    if ((err != 0) || (NULL == addrInfo)) {
        success = COREFX_NET_INVALID_PARAM;
        goto done;
    }

    // copy the name, if it is not null 
    if (NULL == addrInfo->ai_canonname) {
        *canonicalName = NULL;
    }
    else {
        *canonicalName = (char *)calloc(strlen(addrInfo->ai_canonname) + 1,sizeof(char));
        strcpy(*canonicalName, addrInfo->ai_canonname);
    }

    // iterate over addrInfo, twice
    // first get the length
    node = addrInfo;
    index = 0;
    while (NULL != node) {
        if ((AF_INET6 == node->ai_family) || (AF_INET == node->ai_family)) {
            index++;
        }
        node = node->ai_next;
    }

    // now do the actual parsing
    *addressCount = index;
    if (0 != *addressCount) {
        index = 0;
        *result = (struct IPAddress *)calloc(*addressCount, sizeof(struct IPAddress));
        node = addrInfo;
        while (NULL != node) {
            if (0 != addrInfoNodeToIPAddress(node, *result, index)) {
                index++;
            }
            node = node->ai_next;
        }
    }
    
 done:
    // free addrinfo regardless of the result
    freeaddrinfo(addrInfo);
    if (COREFX_NET_SUCCESS != success) {
        if (*canonicalName != NULL) {
            free(*canonicalName);
        }
        *addressCount=0;
        *result = NULL;
        *canonicalName  = NULL;
    }
    return success;
}
