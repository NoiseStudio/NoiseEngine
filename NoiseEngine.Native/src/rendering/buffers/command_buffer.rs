use crate::{interop::prelude::InteropResult, rendering::fence::GraphicsFence};

pub trait GraphicsCommandBuffer<'init> {
    fn execute(&'init self) -> InteropResult<Box<Box<dyn GraphicsFence + 'init>>>;
}
