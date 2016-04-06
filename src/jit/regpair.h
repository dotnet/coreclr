// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*****************************************************************************/

#ifndef PAIRBEG
#define PAIRBEG(reg)
#endif

#ifndef PAIRDEF
#define PAIRDEF(r1,r2)
#endif

#ifndef PAIRSTK
#define PAIRSTK(r1,r2) PAIRDEF(r1,r2)
#endif

#if defined(_TARGET_X86_)
/*****************************************************************************/
/*                  The following is for x86                                 */
/*****************************************************************************/

//      rlo rhi

PAIRBEG(EAX    )
PAIRDEF(EAX,ECX)
PAIRDEF(EAX,EDX)
PAIRDEF(EAX,EBX)
PAIRDEF(EAX,EBP)
PAIRDEF(EAX,ESI)
PAIRDEF(EAX,EDI)
PAIRSTK(EAX,STK)

PAIRBEG(ECX    )
PAIRDEF(ECX,EAX)
PAIRDEF(ECX,EDX)
PAIRDEF(ECX,EBX)
PAIRDEF(ECX,EBP)
PAIRDEF(ECX,ESI)
PAIRDEF(ECX,EDI)
PAIRSTK(ECX,STK)

PAIRBEG(EDX    )
PAIRDEF(EDX,EAX)
PAIRDEF(EDX,ECX)
PAIRDEF(EDX,EBX)
PAIRDEF(EDX,EBP)
PAIRDEF(EDX,ESI)
PAIRDEF(EDX,EDI)
PAIRSTK(EDX,STK)

PAIRBEG(EBX    )
PAIRDEF(EBX,EAX)
PAIRDEF(EBX,EDX)
PAIRDEF(EBX,ECX)
PAIRDEF(EBX,EBP)
PAIRDEF(EBX,ESI)
PAIRDEF(EBX,EDI)
PAIRSTK(EBX,STK)

PAIRBEG(EBP    )
PAIRDEF(EBP,EAX)
PAIRDEF(EBP,EDX)
PAIRDEF(EBP,ECX)
PAIRDEF(EBP,EBX)
PAIRDEF(EBP,ESI)
PAIRDEF(EBP,EDI)
PAIRSTK(EBP,STK)

PAIRBEG(ESI    )
PAIRDEF(ESI,EAX)
PAIRDEF(ESI,EDX)
PAIRDEF(ESI,ECX)
PAIRDEF(ESI,EBX)
PAIRDEF(ESI,EBP)
PAIRDEF(ESI,EDI)
PAIRSTK(ESI,STK)

PAIRBEG(EDI    )
PAIRDEF(EDI,EAX)
PAIRDEF(EDI,EDX)
PAIRDEF(EDI,ECX)
PAIRDEF(EDI,EBX)
PAIRDEF(EDI,EBP)
PAIRDEF(EDI,ESI)
PAIRSTK(EDI,STK)

PAIRBEG(STK    )
PAIRSTK(STK,EAX)
PAIRSTK(STK,EDX)
PAIRSTK(STK,ECX)
PAIRSTK(STK,EBX)
PAIRSTK(STK,EBP)
PAIRSTK(STK,ESI)
PAIRSTK(STK,EDI)

#endif

/*****************************************************************************/

#ifdef _TARGET_ARM_
/*****************************************************************************/
/*                  The following is for ARM                                 */
/*****************************************************************************/

//      rlo rhi

PAIRBEG(R0   )
PAIRDEF(R0,R1)
PAIRDEF(R0,R2)
PAIRDEF(R0,R3)
PAIRDEF(R0,R4)
PAIRDEF(R0,R5)
PAIRDEF(R0,R6)
PAIRDEF(R0,R7)
PAIRDEF(R0,R8)
PAIRDEF(R0,R9)
PAIRDEF(R0,R10)
PAIRDEF(R0,R11)
PAIRDEF(R0,R12)
PAIRDEF(R0,LR)
PAIRSTK(R0,STK)

PAIRBEG(R1   )
PAIRDEF(R1,R0)
PAIRDEF(R1,R2)
PAIRDEF(R1,R3)
PAIRDEF(R1,R4)
PAIRDEF(R1,R5)
PAIRDEF(R1,R6)
PAIRDEF(R1,R7)
PAIRDEF(R1,R8)
PAIRDEF(R1,R9)
PAIRDEF(R1,R10)
PAIRDEF(R1,R11)
PAIRDEF(R1,R12)
PAIRDEF(R1,LR)
PAIRSTK(R1,STK)

PAIRBEG(R2   )
PAIRDEF(R2,R0)
PAIRDEF(R2,R1)
PAIRDEF(R2,R3)
PAIRDEF(R2,R4)
PAIRDEF(R2,R5)
PAIRDEF(R2,R6)
PAIRDEF(R2,R7)
PAIRDEF(R2,R8)
PAIRDEF(R2,R9)
PAIRDEF(R2,R10)
PAIRDEF(R2,R11)
PAIRDEF(R2,R12)
PAIRDEF(R2,LR)
PAIRSTK(R2,STK)

PAIRBEG(R3   )
PAIRDEF(R3,R0)
PAIRDEF(R3,R1)
PAIRDEF(R3,R2)
PAIRDEF(R3,R4)
PAIRDEF(R3,R5)
PAIRDEF(R3,R6)
PAIRDEF(R3,R7)
PAIRDEF(R3,R8)
PAIRDEF(R3,R9)
PAIRDEF(R3,R10)
PAIRDEF(R3,R11)
PAIRDEF(R3,R12)
PAIRDEF(R3,LR)
PAIRSTK(R3,STK)

PAIRBEG(R4   )
PAIRDEF(R4,R0)
PAIRDEF(R4,R1)
PAIRDEF(R4,R2)
PAIRDEF(R4,R3)
PAIRDEF(R4,R5)
PAIRDEF(R4,R6)
PAIRDEF(R4,R7)
PAIRDEF(R4,R8)
PAIRDEF(R4,R9)
PAIRDEF(R4,R10)
PAIRDEF(R4,R11)
PAIRDEF(R4,R12)
PAIRDEF(R4,LR)
PAIRSTK(R4,STK)

PAIRBEG(R5   )
PAIRDEF(R5,R0)
PAIRDEF(R5,R1)
PAIRDEF(R5,R2)
PAIRDEF(R5,R3)
PAIRDEF(R5,R4)
PAIRDEF(R5,R6)
PAIRDEF(R5,R7)
PAIRDEF(R5,R8)
PAIRDEF(R5,R9)
PAIRDEF(R5,R10)
PAIRDEF(R5,R11)
PAIRDEF(R5,R12)
PAIRDEF(R5,LR)
PAIRSTK(R5,STK)

PAIRBEG(R6   )
PAIRDEF(R6,R0)
PAIRDEF(R6,R1)
PAIRDEF(R6,R2)
PAIRDEF(R6,R3)
PAIRDEF(R6,R4)
PAIRDEF(R6,R5)
PAIRDEF(R6,R7)
PAIRDEF(R6,R8)
PAIRDEF(R6,R9)
PAIRDEF(R6,R10)
PAIRDEF(R6,R11)
PAIRDEF(R6,R12)
PAIRDEF(R6,LR)
PAIRSTK(R6,STK)

PAIRBEG(R7   )
PAIRDEF(R7,R0)
PAIRDEF(R7,R1)
PAIRDEF(R7,R2)
PAIRDEF(R7,R3)
PAIRDEF(R7,R4)
PAIRDEF(R7,R5)
PAIRDEF(R7,R6)
PAIRDEF(R7,R8)
PAIRDEF(R7,R9)
PAIRDEF(R7,R10)
PAIRDEF(R7,R11)
PAIRDEF(R7,R12)
PAIRDEF(R7,LR)
PAIRSTK(R7,STK)

PAIRBEG(R8   )
PAIRDEF(R8,R0)
PAIRDEF(R8,R1)
PAIRDEF(R8,R2)
PAIRDEF(R8,R3)
PAIRDEF(R8,R4)
PAIRDEF(R8,R5)
PAIRDEF(R8,R6)
PAIRDEF(R8,R7)
PAIRDEF(R8,R9)
PAIRDEF(R8,R10)
PAIRDEF(R8,R11)
PAIRDEF(R8,R12)
PAIRDEF(R8,LR)
PAIRSTK(R8,STK)

PAIRBEG(R9   )
PAIRDEF(R9,R0)
PAIRDEF(R9,R1)
PAIRDEF(R9,R2)
PAIRDEF(R9,R3)
PAIRDEF(R9,R4)
PAIRDEF(R9,R5)
PAIRDEF(R9,R6)
PAIRDEF(R9,R7)
PAIRDEF(R9,R8)
PAIRDEF(R9,R10)
PAIRDEF(R9,R11)
PAIRDEF(R9,R12)
PAIRDEF(R9,LR)
PAIRSTK(R9,STK)

PAIRBEG(R10   )
PAIRDEF(R10,R0)
PAIRDEF(R10,R1)
PAIRDEF(R10,R2)
PAIRDEF(R10,R3)
PAIRDEF(R10,R4)
PAIRDEF(R10,R5)
PAIRDEF(R10,R6)
PAIRDEF(R10,R7)
PAIRDEF(R10,R8)
PAIRDEF(R10,R9)
PAIRDEF(R10,R11)
PAIRDEF(R10,R12)
PAIRDEF(R10,LR)
PAIRSTK(R10,STK)

PAIRBEG(R11   )
PAIRDEF(R11,R0)
PAIRDEF(R11,R1)
PAIRDEF(R11,R2)
PAIRDEF(R11,R3)
PAIRDEF(R11,R4)
PAIRDEF(R11,R5)
PAIRDEF(R11,R6)
PAIRDEF(R11,R7)
PAIRDEF(R11,R8)
PAIRDEF(R11,R9)
PAIRDEF(R11,R10)
PAIRDEF(R11,R12)
PAIRDEF(R11,LR)
PAIRSTK(R11,STK)

PAIRBEG(R12   )
PAIRDEF(R12,R0)
PAIRDEF(R12,R1)
PAIRDEF(R12,R2)
PAIRDEF(R12,R3)
PAIRDEF(R12,R4)
PAIRDEF(R12,R5)
PAIRDEF(R12,R6)
PAIRDEF(R12,R7)
PAIRDEF(R12,R8)
PAIRDEF(R12,R9)
PAIRDEF(R12,R10)
PAIRDEF(R12,R11)
PAIRDEF(R12,LR)
PAIRSTK(R12,STK)

PAIRBEG(LR    )
PAIRDEF(LR ,R0)
PAIRDEF(LR ,R1)
PAIRDEF(LR ,R2)
PAIRDEF(LR ,R3)
PAIRDEF(LR ,R4)
PAIRDEF(LR ,R5)
PAIRDEF(LR ,R6)
PAIRDEF(LR ,R7)
PAIRDEF(LR ,R8)
PAIRDEF(LR ,R9)
PAIRDEF(LR ,R10)
PAIRDEF(LR ,R11)
PAIRDEF(LR ,R12)
PAIRSTK(LR ,STK)

PAIRBEG(STK   )
PAIRSTK(STK,R0)
PAIRSTK(STK,R1)
PAIRSTK(STK,R2)
PAIRSTK(STK,R3)
PAIRSTK(STK,R4)
PAIRSTK(STK,R5)
PAIRSTK(STK,R6)
PAIRSTK(STK,R7)
PAIRSTK(STK,R8)
PAIRSTK(STK,R9)
PAIRSTK(STK,R10)
PAIRSTK(STK,R11)
PAIRSTK(STK,R12)
PAIRSTK(STK,LR)

#endif

/*****************************************************************************/

#undef PAIRBEG
#undef PAIRDEF
#undef PAIRSTK

/*****************************************************************************/
