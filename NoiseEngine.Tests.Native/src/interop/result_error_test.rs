use std::{str::{FromStr, ParseBoolError}, error::Error, fmt::Display};

use noise_engine_native::interop::result_error::ResultError;

#[no_mangle]
extern "C" fn interop_result_error_test_unmanaged_inner_error() -> ResultError {
    ResultError::new(&InnerError {
        inner: <bool as FromStr>::from_str("invalid").unwrap_err()
    })
}

#[no_mangle]
extern "C" fn interop_result_error_test_unmanaged_parse_bool() -> ResultError {
    ResultError::new(&<bool as FromStr>::from_str("invalid").unwrap_err())
}

#[derive(Debug)]
struct InnerError {
    inner: ParseBoolError
}

impl Error for InnerError {
    fn source(&self) -> Option<&(dyn Error + 'static)> {
        Some(&self.inner)
    }
}

impl Display for InnerError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "InnerError is here!")
    }
}
