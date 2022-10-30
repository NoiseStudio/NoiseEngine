use crate::interop::prelude::InteropResult;

pub trait GraphicsCommandBuffer {
    fn execute(&self) -> InteropResult<()>;
}
