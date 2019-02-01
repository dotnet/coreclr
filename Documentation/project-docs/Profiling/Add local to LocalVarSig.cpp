// Adapted from an entry that appeared on David Broman's blog
//
// Parse the local variables signature for the method we're rewriting, create a new
// localvar signature containing one new local, and return the 0-based ordinal for
// that  new local.

UINT AddNewLocal()
{
    // Get the metadata interfaces on the module 
    containing the method being rewritten.

   HRESULT hr = 
   m\_pICorProfilerInfo-\>GetModuleMetaData(

    m\_moduleId, 
    ofRead | ofWrite, IID\_IMetaDataImport, (IUnknown\*\*)&m\_pMetaDataImport);

   
   if (FAILED(hr))

   {


    return 0;

}



hr = 
m\_pMetaDataImport-\>QueryInterface(IID\_IMetaDataEmit, (void\*\*)&m\_pMetaDataEmit);


if (FAILED(hr))

{


    return 0;

}




// Here's a buffer into which we will write out the 
modified signature.  This sample


// code just bails out if it hits signatures that are 
too big.  Just one of many reasons


// why you use this code AT YOUR OWN RISK!

COR\_SIGNATURE 
rgbNewSig[4096];




// Use the signature token to look up the actual 
signature

PCCOR\_SIGNATURE 
rgbOrigSig = NULL;

ULONG cbOrigSig;

hr = 
m\_pMetaDataImport-\>GetSigFromToken(m\_tkLocalVarSig, &rgbOrigSig, &cbOrigSig);


if (FAILED(hr))

{


    return 0;

}




// These are our running indices in the original and 
new signature, respectively

UINT iOrigSig = 0;

UINT iNewSig = 0;




// First byte of signature must identify that it's a 
locals signature!


assert(rgbOrigSig[iOrigSig] == SIG\_LOCAL\_SIG);



    // 
Copy SIG\_LOCAL\_SIG


if (iNewSig + 1 \> 
   sizeof(rgbNewSig))

{


// We'll write one byte below but no room!


    return 0;

}

rgbNewSig[iNewSig++] 
= rgbOrigSig[iOrigSig++];




// Get original count of locals...

ULONG cOrigLocals;

ULONG cbOrigLocals;

ULONG cbNewLocals;

hr = 
CorSigUncompressData(&rgbOrigSig[iOrigSig],

                              4,                    // 
                              [IN] length of the signature

                              &cOrigLocals,         // 
                              [OUT] the expanded data

                              &cbOrigLocals);       // 
[OUT] length of the expanded data   


if (FAILED(hr))

{


    return 0;

}




// ...and write new count of locals (cOrigLocals+1)


if (iNewSig + 4 \> 
   sizeof(rgbNewSig))

{


// CorSigCompressData will write up to 4 bytes but no 
   room!


   return 0;

}

cbNewLocals = 
 CorSigCompressData(cOrigLocals+1,         // [IN] 
   given uncompressed data

   &rgbNewSig[iNewSig]); 
// [OUT] buffer where data will be compressed and 
 stored.  

 iOrigSig += 
 cbOrigLocals;

 iNewSig += 
 cbNewLocals;

 


// Copy the rest


 if (iNewSig + cbOrigSig - iOrigSig \>
    sizeof(rgbNewSig))

 {


// We'll copy cbOrigSig - iOrigSig bytes, but no room!


    return 0;

}

memcpy(&rgbNewSig[iNewSig], 
   &rgbOrigSig[iOrigSig], cbOrigSig-iOrigSig);

iNewSig += 
cbOrigSig-iOrigSig;




// Manually append final local



ULONG cbLocalType;


if (iNewSig + 1 \> 
   sizeof(rgbNewSig))

{


// We'll write one byte below but no room!


    return 0;

}


rgbNewSig[iNewSig++] = ELEMENT\_TYPE\_VALUETYPE;




// You'll need to replace 0x01000002 with the 
appropriate token that describes


// the type of this local (which, in turn, is the type 
of the return value


// you're copying into that local).  This can be 
either a TypeDef or TypeRef,


// and it must be encoded (compressed).


if (iNewSig + 4 \> 
   sizeof(rgbNewSig))

{


// CorSigCompressToken will write up to 4 bytes but no 
   room!


   return 0;

}

cbLocalType = 
CorSigCompressToken(0x01000002,

  &rgbNewSig[iNewSig]);



iNewSig += 
cbLocalType;




// We're done building up the new signature blob.  We 
now need to add it to


// the metadata for this module, so we can get a token 
back for it.

assert(iNewSig \<=
    sizeof(rgbNewSig));

hr = 
m\_pMetaDataEmit-\>GetTokenFromSig(&rgbNewSig[0],      
 // [IN] Signature to define.    

  iNewSig,           
// [IN] Size of signature data.

  &m\_tkLocalVarSig); 
// [OUT] returned signature token. 


if (FAILED(hr))

{


    return 0;

}




// 0-based index of new local = 0-based index of 
original last local + 1


//                            = count of original 
locals


return cOrigLocals;

}

