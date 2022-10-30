use crate::interop::prelude::InteropResult;

pub trait GraphicsFence {
    fn wait(&self, timeout: u64) -> InteropResult<()>;
    fn is_signaled(&self) -> InteropResult<bool>;

    /// # REMARKS
    /// Self fence is ignored in waiting.
    /// # SAFETY
    /// All fences must be from the same API and device.
    unsafe fn wait_multiple(&self, fences: &[&&dyn GraphicsFence], wait_all: bool, timeout: u64) -> InteropResult<()>;
}
