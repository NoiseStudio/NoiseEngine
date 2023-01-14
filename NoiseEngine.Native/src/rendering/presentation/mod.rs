#[cfg(target_os = "windows")]
pub mod windows;

pub mod window_settings;
pub(crate) mod window_event_handler;
pub mod window;
