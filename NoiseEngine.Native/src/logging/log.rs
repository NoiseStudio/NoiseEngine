use super::{logger, log_level::LogLevel};

pub fn debug(message: &str) {
    logger::log(LogLevel::Debug, message);
}

pub fn trace(message: &str) {
    logger::log(LogLevel::Trace, message);
}

pub fn info(message: &str) {
    logger::log(LogLevel::Info, message);
}

pub fn warning(message: &str) {
    logger::log(LogLevel::Warning, message);
}

pub fn error(message: &str) {
    logger::log(LogLevel::Error, message);
}

pub fn fatal(message: &str) {
    logger::log(LogLevel::Fatal, message);
}
