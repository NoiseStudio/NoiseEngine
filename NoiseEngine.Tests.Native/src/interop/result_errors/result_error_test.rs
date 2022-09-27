use std::str::FromStr;

use noise_engine_native::interop::{prelude::{InteropResult, InteropString}, result_errors::result_error::ResultError};

#[no_mangle]
extern "C" fn interop_result_errors_result_error_test_unmanaged_parse_bool(value: InteropString) ->
    InteropResult<bool, ResultError>
{
    <bool as FromStr>::from_str(String::from(value).as_str()).into()
}