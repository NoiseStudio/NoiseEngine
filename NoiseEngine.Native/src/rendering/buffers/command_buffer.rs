use crate::{interop::prelude::InteropResult, rendering::fence::GraphicsFence};

pub trait GraphicsCommandBuffer<'a> {
    fn execute(&'a self) -> InteropResult<Box<Box<dyn GraphicsFence + 'a>>>;
}
