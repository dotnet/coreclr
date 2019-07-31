The files in this folders were used to create `amd64InstrDecode.h`

The following process was executed on an amd64 Linux host in this directory.

```bash
gcc createOpcodes.cpp -o createOpcodes
./createOpcodes > opcodes.cpp
gcc -g opcodes.cpp -o opcodes
gdb opcodes -batch -ex "set disassembly-flavor intel" -ex "disass /r opcodes" > opcodes.intel
cat opcodes.intel | dotnet run > ../amd64InstrDecode.h 
```
