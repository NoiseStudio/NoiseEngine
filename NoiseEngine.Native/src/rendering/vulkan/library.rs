use ash::vk;
use libc::c_void;

pub(crate) fn create(symbol: *const c_void) -> Result<ash::Entry, ash::LoadingError> {
    let function = vk::StaticFn::load_checked(|_| {
        symbol
    })?;

    Ok(unsafe {
        ash::Entry::from_static_fn(function)
    })
}
