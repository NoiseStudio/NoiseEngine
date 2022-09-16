use noise_engine_native::interop::interop_span::InteropSpan;

#[no_mangle]
extern "C" fn interop_interop_span_test_unmanaged_read(span: InteropSpan<i32>, index: i32) -> i32 {
    let slice: &mut [i32] = span.into();
    slice[index as usize]
}

#[no_mangle]
extern "C" fn interop_interop_span_test_unmanaged_write(span: InteropSpan<i32>, index: i32, value: i32) {
    let slice: &mut [i32] = span.into();
    slice[index as usize] = value;
}