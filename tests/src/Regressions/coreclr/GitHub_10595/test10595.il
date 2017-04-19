
.assembly extern legacy library System.Runtime as SomethingElse {}

.assembly test10595 { }

.class _simple extends [SomethingElse]System.Object {
    .method static int32 main(class [SomethingElse]System.String[]) {
    .entrypoint
    .maxstack	100
    newobj instance void [SomethingElse]System.Object::.ctor()
	// newobj instance void [SomethingElse]System.ValueType::.ctor() ; Neither ValueType nor Enum could be created directly using default ctor because it is protected (inaccessible for us here)
	// newobj instance void [SomethingElse]System.Enum::.ctor()
	pop
    ldc.i4 100
    ret
    }
}