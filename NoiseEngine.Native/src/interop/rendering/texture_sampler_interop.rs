use std::sync::Arc;

use crate::rendering::texture_sampler::TextureSampler;

#[no_mangle]
extern "C" fn rendering_texture_sampler_interop_destroy(_handle: Box<Arc<dyn TextureSampler>>) {}
