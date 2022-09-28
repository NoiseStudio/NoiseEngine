use std::{error::Error, fmt::Display};

use noise_engine_native::interop::prelude::{InteropResult, InteropString};

#[no_mangle]
extern "C" fn interop_interop_result_test_unmanaged_create_value(value: i32) -> InteropResult<i32> {
    Ok::<i32, MockError>(value).into()
}

#[no_mangle]
extern "C" fn interop_interop_result_test_unmanaged_create_error(value: InteropString) -> InteropResult<i32> {
    Err::<i32, MockError>(MockError {
        message: value.into()
    }).into()
}

#[derive(Debug)]
struct MockError {
    message: String
}

impl Error for MockError {
}

impl Display for MockError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "{}", self.message)
    }
}
