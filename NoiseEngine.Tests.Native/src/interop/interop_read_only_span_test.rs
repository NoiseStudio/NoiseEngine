use noise_engine_native::interop::interop_read_only_span::InteropReadOnlySpan;

#[no_mangle]
extern "C" fn interop_interop_read_only_span_test_unmanaged_read(span: InteropReadOnlySpan<i32>, index: i32) -> i32 {
    let slice: &[i32] = span.into();
    slice[index as usize]
}
