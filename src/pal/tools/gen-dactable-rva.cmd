
echo on
echo gen-dactable-rva.cmd %1 %2 %3 %4 %5

set coreclrPath=%1
set targetOS=%2
set targetArch=%3
set buildType=%4
set dactablervaPath=%5

set target=%targetOS%-%targetArch%-%buildType%

echo target=%target%

rem The following code is a hardcoded table of offsets for 3.1.0-3.1.3
rem The offset doesn't change often, but in the 2.1.x history it changed several times
rem this needs to be replaced by an automated mechanism to get the table offset

if "%target%" == "Linux-arm-Release" (
    echo #define DAC_TABLE_RVA 0x00542644 > %dactablervaPath%
    exit 0
)
if "%target%" == "Linux-arm64-Release" (
    echo #define DAC_TABLE_RVA 0x00775f08 > %dactablervaPath%
    exit 0
)
if "%target%" == "alpine-arm64-Release" (
    echo #define DAC_TABLE_RVA 0x0076de88 > %dactablervaPath%
    exit 0
)
if "%target%" == "Linux-x64-Release" (
    echo #define DAC_TABLE_RVA 0x00765688 > %dactablervaPath%
    exit 0
)
if "%target%" == "alpine-x64-Release" (
    echo #define DAC_TABLE_RVA 0x0099fb88 > %dactablervaPath%
    exit 0
)

rem Set a bogus address for other cases to allow compilation
echo #define DAC_TABLE_RVA 0xdeadbeef > %dactablervaPath%
exit 0
