use crate::logging::{logger, log_data};

#[no_mangle]
extern "C" fn logging_initialize(
    handler: unsafe extern "C" fn(log_data::LogData),
) {
    let logger = logger::Logger { handler };
    logger::initialize(logger);
}

#[no_mangle]
extern "C" fn logging_terminate() {
    logger::terminate();
}
