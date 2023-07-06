use crate::logging::{log_data, logger};

use crate::interop::prelude::InteropResult;

#[no_mangle]
extern "C" fn logging_logging_initialize(
    handler: unsafe extern "C" fn(log_data::LogData),
) -> InteropResult<()> {
    let logger = logger::Logger { handler };
    logger::initialize(logger).into()
}

#[no_mangle]
extern "C" fn logging_logging_terminate() {
    logger::terminate();
}
