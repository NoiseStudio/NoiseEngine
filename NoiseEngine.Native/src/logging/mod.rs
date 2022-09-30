pub mod prelude;

pub(crate) mod logger;
pub(crate) mod log_level;
pub(crate) mod log_data;

// NOTE: The following functions are not caching logs to be sent from the worker thread;
//       they are sending them immediately through interop. In the future we may want to change that.

pub fn debug(message: &str) {
    logger::log(log_level::LogLevel::Debug, message);
}

pub fn trace(message: &str) {
    logger::log(log_level::LogLevel::Trace, message);
}

pub fn info(message: &str) {
    logger::log(log_level::LogLevel::Info, message);
}

pub fn warning(message: &str) {
    logger::log(log_level::LogLevel::Warning, message);
}

pub fn error(message: &str) {
    logger::log(log_level::LogLevel::Error, message);
}

pub fn fatal(message: &str) {
    logger::log(log_level::LogLevel::Fatal, message);
}
