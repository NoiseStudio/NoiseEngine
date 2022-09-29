pub mod prelude;

pub mod logger;
pub mod log_level;
pub mod log_data;

/// # Panics
/// This function can be called only once throughout the application's lifetime,
/// even after a call to [`logging_terminate`]; subsequent calls will panic.
#[no_mangle]
extern "C" fn logging_initialize(
    handler: unsafe extern "C" fn(log_data::LogData),
) {
    let logger = logger::Logger { handler };
    log::set_boxed_logger(Box::new(logger)).unwrap();
    log::set_max_level(log::LevelFilter::Error);
}

#[no_mangle]
extern "C" fn logging_terminate() {
    log::set_max_level(log::LevelFilter::Off);
}
