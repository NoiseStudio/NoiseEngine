use crate::{rendering::buffers::command_buffer::GraphicsCommandBuffer, interop::prelude::InteropResult};

#[no_mangle]
extern "C" fn rendering_buffers_command_buffer_interop_destroy(_handle: Box<Box<dyn GraphicsCommandBuffer>>) {
}

#[no_mangle]
extern "C" fn rendering_buffers_command_buffer_interop_execute(
    command_buffer: &&dyn GraphicsCommandBuffer
) -> InteropResult<()> {
    command_buffer.execute()
}
