use crate::audio::audio_listener;
use crate::interop::prelude::InteropResult;

#[no_mangle]
extern "C" fn audio_audio_listener_create(id: u64) {
    let audio_listener = audio_listener::AudioListener::create(id);
}
