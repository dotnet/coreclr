Reporting Variable Location
===================
Table of Contents
-----------------

[Debug Info](#debug-info)

[Context](#context)

[Debug Code vs Optimized Code](#debug-code-vs-optimized-code)

[siScope / psiScope Structures](#siscope-psiscope-structure)

[Generating siScope / psiScope info](#siscope-psiscope)

[Variable Live Range Structure](#variable-live-range-structure)

[Generating Variable Live Range](#variable-live-range)

[Turning On Debug Info](#debug-indo-flags)

[Dumps and Debugging Support](#dumps-and-debugging-support)

[Future Extensions and Enhancements](#future-extensions-and-enhancements)

Debug Info
--------

The debugger expects to receive an array of `VarResultInfo` which indicates:

-   IL variable number

-   Location

-   First native offset this location is valid

-   First native offset this location is no longer valid

There could be more than one per variable.
There should be no overlap in terms of starting/ending native offsets for the same variable.
Given a native offset, no two variables should share the same location.

Context  
--------

We generate variables debug info while we generate the assembly intructions of the code. This is happening in `genCodeForBBList()` and `genFnProlog()`.

If we are jitting release code, information about variable liveness will be included in `GenTree`s and `BasicBlock` when `genCodeForBBList` is reached.
For each When `BasicBlock`:

-   `bbLiveIn`: alive at the beginning

-   `bbLiveOut`: alive at the end

Also each `GenTree` has a mask `gtFlags` indicating will include if a variable:

-   is being born

-   is becoming death

-   is being spilled

-   has been spilled

When generating each instruction, this flags are used to know if we need to start/end tracking a variable or update its location (which means in practice "end and start").
The blocks set of live variables are also used to start/end variables tracking information. 

Once the code is done we know the native offset of every assembly instruction, which is required for the debug info.
While we are generating code we don't work with native offsets but `emitLocation`s that the `emitter` can generate, and once the code is done we can get the native offset corresponding to each of them.

Debug Code vs Optimized Code
--------

Variables on debug code:
-   are alive during the whole method

-   live in a fixed location on the stack the whole method

-   `bbLiveIn` and `bbLiveOut` sets are empty

-   there is no flag for spill/unspill variables

Variables on optimized code:

-   could change their location during code execution

-   could change their liveness during code execution

siScope / psiScope Structures
--------

#### siScope

This struct is used to represent the ranges where a variable is alive during code generation.

It has two field to denote the native offset where it is valid:

-   `scStartLoc`: an emitLocation indicating from which instruction

-   `scStartLoc`: an emitLocation indication to which instruction

and more fields to indicate from which variable is this scope.

It doesn't have information about the location of the variable, only the stack level of the variable in the case it was on the stack.

#### psiScope

This struct is used to represente the ranges where a variable is alive during the prolog.
It holds the same information than siScope with the addition of a way of describing the location.
It describes the variable location with two registers or a base register and the offset.

Generating siScope / psiScope info
--------

In order to use Scope Info for tracking variables location, the Flag `USING_SCOPE_INFO` in codegeninterface.h should be defined.

#### For Prolog

We open a `psiScope` for every paramater indicating its location at the beginning of the prolog. We then close them all at the end of the prolog.

#### For Method Code

We start generating code for every `BasicBlock`:

-   checking at the beginning of each one if there is a variable which hasn't an open siScope and has a `varScope` with an starting IL offset lower or equal to the beginning of the block.

-   closing a siScope if a variable is last used.

and we close all the open `siScope`s when the last block is reached.

##### Reporting Information

Once the code for the block and each of the `BasicBlock` is done, `genSetScopeInfo()` is called.
In the case of having the flag `USING_SCOPE_INFO`, 

All the `psiScope` list is iterated filling the debug info structure.
Then the list of `siScope` created is iterated, using the `LclVarDsc` class to get the location of the variable, which would hold the description of the last position it has.
This is not a problem for debug code because variables live in a fixed location the whole method. 

This is done in `CodeGen::genSetScopeInfoUsingsiScope()`

Variable Live Range Structure
--------

### VariableLiveRange

This class is used to represent an uninterruptible portion of variable live in one location.
Owns two emitLocations indicating the first instructions that make it valid and invalid respectively.
It also has the location of the variable in this range, which is stored at the moment of being created.

To save space, we save the ending emitLocation only when the variable is being moved or its liveness change, which could happen many blocks after it was being born.
This means a `VariableLiveRange` native offsets range could be longer than a `BasicBlock`.

### VariableLiveDescriptor

This class is used to represent the liveness of a variable.
It has a list of `VariableLiveRange`s and common operations to update the variable liveness.

### VariableLiveKeeper

It holds an array of `VariableLiveDescriptor`s, one per variable that is being tracked.
The index of each variable inside this array is the same as in `Compiler::lvaTable`.

We could have `VariableLiveDescriptor` inside each LclVarDsc or an array of `VariableLiveDescriptor` inside `Compiler`, but the intention is to move code out of `Compiler` and not increse it.

Generating Variable Live Range
--------

In order to use Variable Live Range for tracking variables location, the Flag `USING_VARIABLE_LIVE_RANGE` in codegeninterface.h should be defined.

### For optimized code

In `genCodeForBBList()`, we start generating code for each block dealing with some cases.

On `BasicBlock` boundaries:

-   `BasicBlock`s beginning:

    -   If a variable has an open `VariableLiveRange` but the location is different than what is expected to be in this block we update it (close the existing one and create another).
        This could happen because a variable could be in the `bbLiveOut` of a block and in the `BasicBlock`s `bbLiveOut` of the next one, but that doesn't mean that the execution thread would go from one inmeadiatly to the next one.
        For this kind of cases another block that moves the variable from its original to the expected position is created.
        This is handled in `LinearScan::recordVarLocationsAtStartOfBB(BasicBlock* bb)`.

    -   If a variable has not an open `VariableLiveRange` and is in `bbLiveIn`, we open one.
        This is done in `genUpdateLife` inmeadiatly after the the previous method is called.

    -   If a variable has an open `VariableLiveRange` and is not in `bbLiveIn`, we close it.
        This is handled considered in `genUpdateLife` too.

-   last `BasicBlock`s ending:

    -   We close every open `VariableLiveRange`.
        This is handled in `genCodeForBBList` when iterating the blocks, after the code for each block is done.

For each instruction in `BasicBlock`:
-   a `VariableLiveRange` is opened for each variable that is being born and not is becoming dead at the same instruction
    Handled in `TreeLifeUpdater::UpdateLifeVar(GenTree* tree)`

-   a `VariableLiveRange` is closed and another one opened for each variable that is being spill, unspill or copy and is not becoming death at the same instruction.
    Spills and copies are handled in `TreeLifeUpdater::UpdateLifeVar(GenTree* tree)`, unspills in `CodeGen::genUnspillRegIfNeeded(GenTree* tree)`.

-   a `VariableLiveRange` is closed for each variable that is becoming death.
    Handled in `TreeLifeUpdater::UpdateLifeVar(GenTree* tree)`

We are not reporing the cases where a variable liveness is being modify and also becoming dead to save memory.

### For debug code

As no flag is being added `GenTree` that we can consume and no variable is included in `bbLiveIn` or `bbLiveOut`, we are currently reporting a variable as being born the same way is done for siScope info in `siBeginBlock` each time before `BasicBlock`s code is generated.
The death of a variable is handled at the end of the last `BasicBlock` as variable live during the whole method.

### Reporting Information

We just iterate throught all the `VariableLiveRange`s of all the variables that are tracked in `CodeGen::genSetScopeInfoUsingVariableRanges()`.

Turning On Debug Info
--------

There is a flag to turn on each of this ways of tracking variable debug info:
-   : for `siScope` and `psiScope`

-   : for `VariableLiveRange`

In case only one of them is defined, that one will be sent to the debugger.
If both are defined, Scope info is sent to the debugger.
If none is defined, no info is sent to the debugger.

Both flags can be found at the beginnig of `codegeninterace.h`

Dumps and Debugging Support
--------

#### Variable Live Ranges activity during a BasicBlock

If we have the flag for `VariableLiveRange`s we would get on the jitdump a verbose message after each BasicBlock is donde indicating the changes for each variable.
For example: 

```
////////////////////////////////////////
////////////////////////////////////////
Var History Dump for Block 44 
Var 1:
[esi [ (G_M8533_IG29,ins#2,ofs#3), NON_CLOSED_RANGE ]; ]
Var 12:
[ebp[-44] (1 slot) [ (G_M8533_IG29,ins#2,ofs#3), NON_CLOSED_RANGE ]; ]
Var 17:
[ebp[-56] (1 slot) [ (G_M8533_IG32,ins#2,ofs#7), (G_M8533_IG33,ins#10,ofs#32) ]; edx [ (G_M8533_IG33,ins#10,ofs#32), (G_M8533_IG33,ins#10,ofs#32) ]; ]
////////////////////////////////////////
////////////////////////////////////////
```

indicating that:

-   Variable with index number 1 is living in register esi since instruction group 29 and it is still living there at the end of the block.

-   Variable with index number 12 in a similar situation as 1 but living on the stack.

-   Variable with index number 17 was living on the stack since instruction group 32 and it was unspill on ig 33, which is the instruction group of this BasicBlock, and it isn't alive at the end of the block.

-   Those are the only variables that are being tracked in this method and were alive during part or the whole method. 

Each `VariableLiveRange` is dumped as:
```
Location [ starting_emit_location, ending_emit_location )
```
and a list of them for a variable.

Something to consider is that as we don't have the final native offsets while we are generating code, we are just dumping `emitLocation`s.

#### All the Variable Live Ranges

We also get all the `VariableLiveRange`s dumped for all the variables once the code for the whole method is done with the native offsets in place of `emitLocation`s.

The information follows the same pattern as before.

```
////////////////////////////////////////
////////////////////////////////////////
PRINTING REGISTER LIVE RANGES:
[esi [3C , 270 )esi [275 , 2BE )esi [2DA , 390 )]
IL Var Num 12:
[ebp[-44] (1 slot) [200 , 270 )ebp[-44] (1 slot) [275 , 28A )ebp[-44] (1 slot) [292 , 2BE )ebp[-44] (1 slot) [2DA , 401 )ebp[-44] (1 slot) [406 , 449 )ebp[-44] (1 slot) [465 , 468 )edi [468 , 468 )]
IL Var Num 17:
[ebp[-56] (1 slot) [331 , 373 )edx [373 , 373 )]
////////////////////////////////////////
////////////////////////////////////////
```

#### Debug Info sent to the debugger

The information sent to the debugger is dumped to as:
```
*************** In genSetScopeInfo()
VarLocInfo count is 95
*************** Variable debug info
3 live ranges
  0(   UNKNOWN) : From 00000000h to 0000001Ah, in ecx
  1(   UNKNOWN) : From 0000003Ch to 00000270h, in esi
  1(   UNKNOWN) : From 00000275h to 000002BEh, in esi
```

Future Extensions and Enhancements
--------

There are many things we can do to improve optimized debugging:

-   Inline functions: If you crash inside one, you get no info of your variables.
    Currently we don't have the IL offset of them.
    And this is broadly used to improve code performance.

-   [Promoted structs](https://github.com/dotnet/coreclr/issues/23542): There is no debug support for fields of promoted structs, we just report the struct itself.

-   [Reduce space used for VariableLiveDescriptor](https://github.com/dotnet/coreclr/issues/23544): we are currently using a `jitstd::list`, which is a double linked list. 
    We could use a simple single linked list with push_back(), head(), tail(), size() operations and an iterator and we would be saving memory.
