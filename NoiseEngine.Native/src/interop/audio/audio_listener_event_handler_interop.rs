use crate::{
    interop::prelude::InteropResult,
    audio::audio_listener_event_handler::{self, AudioListenerEventHandler},
};

#[no_mangle]
extern "C" fn audio_audio_listener_event_handler_initialize(
    handler: AudioListenerEventHandler
) -> InteropResult<()> {
    audio_listener_event_handler::initialize(handler).into()
}
