use std::{error::Error, fmt::Display};

#[derive(Debug)]
pub(crate) struct LoggingError {
    kind: LoggingErrorKind,
}

#[derive(Debug)]
pub(crate) enum LoggingErrorKind {
    AlreadyInitialized,
}

impl LoggingError {
    pub(crate) fn new(kind: LoggingErrorKind) -> Self {
        Self { kind }
    }
}

impl Display for LoggingError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match &self.kind {
            LoggingErrorKind::AlreadyInitialized => write!(f, "logger is already initialized"),
        }
    }
}

impl Error for LoggingError {}
