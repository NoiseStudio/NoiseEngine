#[repr(C)]
pub struct TextureSamplerCreateInfo {
    pub max_anisotropy: f32,
}

pub trait TextureSampler {}
