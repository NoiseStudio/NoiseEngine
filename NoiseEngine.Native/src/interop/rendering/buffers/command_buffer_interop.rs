use crate::{
    rendering::{buffers::command_buffer::GraphicsCommandBuffer, fence::GraphicsFence},
    interop::prelude::InteropResult
};

#[no_mangle]
extern "C" fn rendering_buffers_command_buffer_interop_destroy(_handle: Box<Box<dyn GraphicsCommandBuffer>>) {
}

#[no_mangle]
extern "C" fn rendering_buffers_command_buffer_interop_execute<'a>(
    command_buffer: &&'a dyn GraphicsCommandBuffer
) -> InteropResult<Box<Box<dyn GraphicsFence + 'a>>> {
    command_buffer.execute()
}
