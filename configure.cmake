include(CheckCXXSourceCompiles)
include(CheckCXXCompilerFlag)

# VC++ guarantees support for LTCG (LTO's equivalent)
if(NOT WIN32)
  # Function required to give CMAKE_REQUIRED_* local scope
  function(check_have_lto)
    set(CMAKE_REQUIRED_FLAGS -flto)
    set(CMAKE_REQUIRED_LIBRARIES -flto -fuse-ld=gold)
    check_cxx_source_compiles("int main() { return 0; }" HAVE_LTO)
  endfunction(check_have_lto)
  check_have_lto()

  check_cxx_compiler_flag(-faligned-new COMPILER_SUPPORTS_F_ALIGNED_NEW)

  # check if compiler is over-sensitive about -Wtautological-constant-out-of-range-compare
  # this fails to clean compile on clang < v6
  check_cxx_source_compiles("
    #include <cassert>
    enum X : unsigned char { A, B };
    int main()
    {
      X x = X::A;
      static const char* y[] { \"1\", \"2\", \"3\" };
      assert(x < sizeof(y)/sizeof(y[0]));
    }"
    COMPILER_HAS_ROBUST_TAUTOLOGICAL_COMPARISON_CHECKS
    FAIL_REGEX ".*Wtautological-constant-out-of-range-compare.*")
endif(NOT WIN32)
