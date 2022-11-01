use crate::{interop::prelude::InteropResult, rendering::fence::GraphicsFence};

pub trait GraphicsCommandBuffer {
    fn execute(&self) -> InteropResult<Box<Box<dyn GraphicsFence + '_>>>;
}
