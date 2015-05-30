//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 
//

/*=============================================================================
**
** Source: localhostaddrs.c
**
** Purpose: Test to ensure GetIPAddresses is working well for local host
**
** Dependencies: PAL_Initialize
**               GetIPAddresses
** 

**
**===========================================================================*/
#include <palsuite.h>
#include "corefxnet.h"

void checkValidAddress(struct IPAddress *ipAddresses, int index)
{
    struct IPAddress address;
    int i = 0;

    address = ipAddresses[index];
    if (address.isIPV4) {
	if (address.scopeId != 0) {
	    Fail("Unexpected scope Id for IP v4 addresses: %du" , address.scopeId);
	}
	for (i=4;i<16;i++) {
	    if (address.bytes[i] != 0) {
		Fail("Unexpected non-zero byte %x at index %d for IP v4 address", address.bytes[i], i);
	    }
	}
    }
    if (address.port < 0) {
	Fail("negative value of port: %d", address.port);
    }
}

/**
 * main
 *
 * executable entry point
 */
int __cdecl main(int argc, const char * const* argv )
{
    const char *localHost = "localhost";
    int addrCount = 0;
    struct IPAddress *addresses = NULL;
    char *canonicalName = NULL;
    int ret,index;

    /*PAL initialization */
    if ((PAL_Initialize(argc, argv)) != 0 ) {
	    return FAIL;
    }

    ret = GetIPAddresses(localHost, &canonicalName, &addresses, &addrCount);
    if (COREFX_NET_SUCCESS != ret) {
	Fail("GetIPAddresses failed for %s ", localHost);
    }
    printf("addrCount = %d\n",addrCount);
    for (index = 0; index < addrCount; index++)
	checkValidAddress(addresses, index);

    if (canonicalName) {
        /* use some string api to ensure that the string has been copied properly */
        printf("%d is the length of %s\n", strlen(canonicalName), canonicalName);
    }
    else {
        Fail("CanonicalName is null ");
    }
  
    free(addresses);
    free(canonicalName);

    PAL_Terminate();
    return PASS;
}
