use std::mem;

#[repr(C)]
struct AlignmentTestStructA {
    a: u8,
    b: f64,
    c: bool,
    d: i32
}

#[no_mangle]
extern "C" fn interop_interop_memory_helper_test_alignment_of_i32() -> usize {
    mem::align_of::<i32>()
}

#[no_mangle]
extern "C" fn interop_interop_memory_helper_test_alignment_of_u8() -> usize {
    mem::align_of::<u8>()
}

#[no_mangle]
extern "C" fn interop_interop_memory_helper_test_alignment_of_struct_a() -> usize {
    mem::align_of::<AlignmentTestStructA>()
}
