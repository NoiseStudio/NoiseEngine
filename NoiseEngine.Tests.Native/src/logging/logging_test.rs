use noise_engine_native::logging::prelude::*;

#[no_mangle]
extern "C" fn logging_interop_logging_test_debug() {
    log::debug("debug");
}

#[no_mangle]
extern "C" fn logging_interop_logging_test_trace() {
    log::trace("trace");
}

#[no_mangle]
extern "C" fn logging_interop_logging_test_info() {
    log::info("info");
}

#[no_mangle]
extern "C" fn logging_interop_logging_test_warning() {
    log::warning("warning");
}

#[no_mangle]
extern "C" fn logging_interop_logging_test_error() {
    log::error("error");
}

#[no_mangle]
extern "C" fn logging_interop_logging_test_fatal() {
    log::fatal("fatal");
}
