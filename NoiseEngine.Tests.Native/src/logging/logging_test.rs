use noise_engine_native::logging::prelude::*;

#[no_mangle]
extern "C" fn logging_interop_logging_test_debug() {
    debug!("debug");
}

#[no_mangle]
extern "C" fn logging_interop_logging_test_trace() {
    trace!("trace");
}

#[no_mangle]
extern "C" fn logging_interop_logging_test_info() {
    info!("info");
}

#[no_mangle]
extern "C" fn logging_interop_logging_test_warning() {
    warn!("warning");
}

#[no_mangle]
extern "C" fn logging_interop_logging_test_error() {
    error!("error");
}
