use cpal::{available_hosts, default_host};
use cpal::traits::{DeviceTrait, HostTrait, StreamTrait};
use crate::audio::audio_listener_event_handler::AudioListenerEventHandler;

pub struct AudioListener {
    id: u64,
}

impl AudioListener {
    pub(crate) fn create(id: u64) -> AudioListener {
        let device = default_host().default_output_device().unwrap();
        let config = device.default_output_config().unwrap().config();
        let sample_rate = config.sample_rate.0 as f32;
        let channels = config.channels as usize;

        // Produce a sinusoid of maximum amplitude.
        let mut sample_clock = 0f32;
        let mut next_value = move || {
            sample_clock = (sample_clock + 1.0) % sample_rate;
            (sample_clock * 440.0 * 2.0 * std::f32::consts::PI / sample_rate).sin()
        };

        let err_fn = |err| eprintln!("an error occurred on stream: {}", err);

        // let event_handler = AudioListenerEventHandler::get();
        let stream = device.build_output_stream(
            &config,
            move |data: &mut [f32], _: &cpal::OutputCallbackInfo| {
                // unsafe {
                //     (event_handler.sample_handler)(0u64, data.into());
                // }
                write_data(data, channels, &mut next_value)
            },
            err_fn,
            None,
        ).unwrap();
        stream.play();
        Self { id }
    }
}

fn write_data(output: &mut [f32], channels: usize, next_sample: &mut dyn FnMut() -> f32)
{
    for frame in output.chunks_mut(channels) {
        let value = next_sample();
        for sample in frame.iter_mut() {
            *sample = value;
        }
    }
}
