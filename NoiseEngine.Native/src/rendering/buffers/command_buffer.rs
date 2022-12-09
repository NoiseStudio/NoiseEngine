use crate::{interop::prelude::InteropResult, rendering::fence::GraphicsFence};

pub trait GraphicsCommandBuffer<'init> {
    fn execute<'cbuf>(&'cbuf self) -> InteropResult<Box<Box<dyn GraphicsFence + 'cbuf>>>;
}
