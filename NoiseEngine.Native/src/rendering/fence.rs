use std::sync::Arc;

use crate::interop::prelude::{InteropResult, ResultError};

pub trait GraphicsFence {
    fn wait(&self, timeout: u64) -> InteropResult<bool>;
    fn is_signaled(&self) -> InteropResult<bool>;

    /// Self fence is ignored in waiting.
    /// # Safety
    /// All fences must be from the same API and device.
    unsafe fn wait_multiple(
        &self, fences: &[&Arc<dyn GraphicsFence>], wait_all: bool, timeout: u64
    ) -> Result<bool, ResultError>;
}
