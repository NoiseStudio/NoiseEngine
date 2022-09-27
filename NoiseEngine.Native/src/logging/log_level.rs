use log::Level;

#[repr(u8)]
#[derive(Clone, Copy)]
pub enum LogLevel {
    Debug = 1 << 0,
    Trace = 1 << 1,
    Info = 1 << 2,
    Warning = 1 << 3,
    Error = 1 << 4,
}

impl From<Level> for LogLevel {
    fn from(level: Level) -> Self {
        match level {
            Level::Debug => LogLevel::Debug,
            Level::Trace => LogLevel::Trace,
            Level::Info => LogLevel::Info,
            Level::Warn => LogLevel::Warning,
            Level::Error => LogLevel::Error,
        }
    }
}
