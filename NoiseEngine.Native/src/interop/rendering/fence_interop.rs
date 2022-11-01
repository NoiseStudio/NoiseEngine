use crate::{
    rendering::fence::GraphicsFence,
    interop::{prelude::InteropResult, interop_read_only_span::InteropReadOnlySpan}
};

#[no_mangle]
extern "C" fn rendering_fence_interop_destroy(_handle: Box<Box<dyn GraphicsFence>>) {
}

#[no_mangle]
extern "C" fn rendering_fence_interop_wait(fence: &&dyn GraphicsFence, timeout: u64) -> InteropResult<bool> {
    fence.wait(timeout)
}

#[no_mangle]
extern "C" fn rendering_fence_interop_is_signaled(fence: &&dyn GraphicsFence) -> InteropResult<bool> {
    fence.is_signaled()
}

#[no_mangle]
extern "C" fn rendering_fence_interop_wait_multiple(
    fences: InteropReadOnlySpan<&&dyn GraphicsFence>, wait_all: bool, timeout: u64
) -> InteropResult<bool> {
    let f: &[&&dyn GraphicsFence] = fences.into();

    match unsafe {
        f[0].wait_multiple(f, wait_all, timeout)
    } {
        Ok(is_signaled) => InteropResult::with_ok(is_signaled),
        Err(err) => InteropResult::with_err(err)
    }
}
