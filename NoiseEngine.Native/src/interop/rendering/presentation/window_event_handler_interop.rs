use crate::{interop::prelude::InteropResult, rendering::presentation::window_event_handler::{WindowEventHandler, self}};

#[no_mangle]
extern "C" fn rendering_presentation_window_event_handler_interop_initialize(
    handler: WindowEventHandler
) -> InteropResult<()> {
    window_event_handler::initialize(handler).into()
}
