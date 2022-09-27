use std::{str::{FromStr, ParseBoolError}, error::Error, fmt::Display};

use noise_engine_native::interop::result_errors::result_error::ResultError;

#[derive(Debug)]
struct InnerError {
    inner: ParseBoolError
}

impl Error for InnerError {
    fn source(&self) -> Option<&(dyn Error + 'static)> {
        Some(&self.inner)
    }

    fn description(&self) -> &str {
        "description() is deprecated; use Display"
    }

    fn cause(&self) -> Option<&dyn Error> {
        self.source()
    }
}

impl Display for InnerError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "InnerError is here!")
    }
}

#[no_mangle]
extern "C" fn interop_result_errors_result_error_test_unmanaged_inner_error() -> ResultError {
    ResultError::new(&InnerError {
        inner: <bool as FromStr>::from_str("invalid").unwrap_err()
    })
}

#[no_mangle]
extern "C" fn interop_result_errors_result_error_test_unmanaged_parse_bool() -> ResultError {
    ResultError::new(&<bool as FromStr>::from_str("invalid").unwrap_err())
}