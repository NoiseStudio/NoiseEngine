use noise_engine_native::{logging::prelude::*, interop::prelude::InteropString};

#[no_mangle]
extern "C" fn logging_interop_logging_test_debug(message: InteropString) {
    log::debug(String::from(message).as_str());
}

#[no_mangle]
extern "C" fn logging_interop_logging_test_trace(message: InteropString) {
    log::trace(String::from(message).as_str());
}

#[no_mangle]
extern "C" fn logging_interop_logging_test_info(message: InteropString) {
    log::info(String::from(message).as_str());
}

#[no_mangle]
extern "C" fn logging_interop_logging_test_warning(message: InteropString) {
    log::warning(String::from(message).as_str());
}

#[no_mangle]
extern "C" fn logging_interop_logging_test_error(message: InteropString) {
    log::error(String::from(message).as_str());
}

#[no_mangle]
extern "C" fn logging_interop_logging_test_fatal(message: InteropString) {
    log::fatal(String::from(message).as_str());
}
