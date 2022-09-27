use log::Log;

use crate::interop::prelude::InteropReadOnlySpan;

use super::{log_level::LogLevel, log_data::LogData};

pub(super) struct Logger {
    pub(super) handler: unsafe extern "C" fn(LogData),
}

impl Log for Logger {
    fn enabled(&self, _metadata: &log::Metadata) -> bool {
        true
    }

    fn log(&self, record: &log::Record) {
        if let Some(message) = record.args().as_str() {
            let message = InteropReadOnlySpan::from(message.as_bytes());
            let data = LogData {
                level: LogLevel::from(record.level()),
                message,
            };

            // SAFETY: The handler executes managed code handling the message.
            //         If the handler fails, application crash is desired (implementation error).
            unsafe {
                (self.handler)(data);
            }
        }
    }

    fn flush(&self) {
    }
}
