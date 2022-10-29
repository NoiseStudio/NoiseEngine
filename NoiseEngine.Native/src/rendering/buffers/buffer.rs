use crate::interop::prelude::InteropResult;

pub trait GraphicsBuffer {
    fn host_read(&self, buffer: &mut [u8], start: u64) -> InteropResult<()>;
    fn host_write(&self, data: &[u8], start: u64) -> InteropResult<()>;
}
