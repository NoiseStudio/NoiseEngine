use std::sync::Arc;

use crate::{
    rendering::{buffers::command_buffer::GraphicsCommandBuffer, fence::GraphicsFence},
    interop::prelude::InteropResult
};

#[no_mangle]
extern "C" fn rendering_buffers_command_buffer_interop_destroy(_handle: Box<Box<dyn GraphicsCommandBuffer>>) {
}

#[no_mangle]
extern "C" fn rendering_buffers_command_buffer_interop_execute<'init: 'cbuf, 'cbuf: 'fence, 'fence>(
    command_buffer: &&'cbuf dyn GraphicsCommandBuffer<'init>
) -> InteropResult<Box<Arc<dyn GraphicsFence + 'fence>>> {
    command_buffer.execute()
}
