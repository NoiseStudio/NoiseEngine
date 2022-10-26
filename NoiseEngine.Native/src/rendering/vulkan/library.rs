pub(crate) fn create() -> Result<ash::Entry, ash::LoadingError> {
    unsafe { ash::Entry::load() }
}
