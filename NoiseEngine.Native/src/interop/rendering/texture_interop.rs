use std::sync::Arc;

use crate::rendering::texture::Texture;

#[no_mangle]
extern "C" fn rendering_texture_interop_destroy(_handle: Box<Arc<dyn Texture>>) {
}
