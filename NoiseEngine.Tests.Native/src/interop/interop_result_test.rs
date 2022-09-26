use noise_engine_native::interop::prelude::{InteropResult, InteropString};

#[no_mangle]
extern "C" fn interop_interop_result_test_unmanaged_create_value(value: i32) -> InteropResult<i32, InteropString> {
    Ok::<i32, String>(value).into()
}

#[no_mangle]
extern "C" fn interop_interop_result_test_unmanaged_create_error(value: InteropString) ->
    InteropResult<i32, InteropString>
{
    Err::<i32, String>(value.into()).into()
}