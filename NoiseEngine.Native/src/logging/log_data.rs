use crate::interop::prelude::InteropReadOnlySpan;

use super::log_level::LogLevel;

#[repr(C)]
pub struct LogData<'a> {
    pub level: LogLevel,
    pub message: InteropReadOnlySpan<'a, u8>,
}
