use ash::vk;

use crate::interop::prelude::InteropArray;

#[repr(C)]
pub enum TextureFileFormat {
    Png,
    Jpeg,
    WebP,
}

pub fn vk_format_to_color_type(vk_format: vk::Format) -> Option<image::ColorType> {
    type F = vk::Format;

    #[rustfmt::skip]
    let result = match vk_format {
        F::R8_UNORM | F::R8_SNORM | F::R8_USCALED | F::R8_SSCALED | F::R8_UINT | F::R8_SINT | F::R8_SRGB => {
            Some(image::ColorType::L8)
        },
        F::R8G8_UNORM | F::R8G8_SNORM | F::R8G8_USCALED | F::R8G8_SSCALED | F::R8G8_UINT | F::R8G8_SINT | F::R8G8_SRGB => {
            Some(image::ColorType::La8)
        },
        F::R8G8B8_UNORM | F::R8G8B8_SNORM | F::R8G8B8_USCALED | F::R8G8B8_SSCALED | F::R8G8B8_UINT | F::R8G8B8_SINT | F::R8G8B8_SRGB => {
            Some(image::ColorType::Rgb8)
        },
        F::R8G8B8A8_UNORM | F::R8G8B8A8_SNORM | F::R8G8B8A8_USCALED | F::R8G8B8A8_SSCALED | F::R8G8B8A8_UINT | F::R8G8B8A8_SINT | F::R8G8B8A8_SRGB => {
            Some(image::ColorType::Rgba8)
        },
        F::R16_UNORM | F::R16_SNORM | F::R16_USCALED | F::R16_SSCALED | F::R16_UINT | F::R16_SINT => {
            Some(image::ColorType::L16)
        },
        F::R16G16_UNORM | F::R16G16_SNORM | F::R16G16_USCALED | F::R16G16_SSCALED | F::R16G16_UINT | F::R16G16_SINT => {
            Some(image::ColorType::La16)
        },
        F::R16G16B16_UNORM | F::R16G16B16_SNORM | F::R16G16B16_USCALED | F::R16G16B16_SSCALED | F::R16G16B16_UINT | F::R16G16B16_SINT => {
            Some(image::ColorType::Rgb16)
        },
        F::R16G16B16A16_UNORM | F::R16G16B16A16_SNORM | F::R16G16B16A16_USCALED | F::R16G16B16A16_SSCALED | F::R16G16B16A16_UINT | F::R16G16B16A16_SINT => {
            Some(image::ColorType::Rgba16)
        },
        F::R32G32B32_SFLOAT => {
            Some(image::ColorType::Rgb32F)
        },
        F::R32G32B32A32_SFLOAT => {
            Some(image::ColorType::Rgba32F)
        },
        _ => None,
    };
    result
}

#[repr(C)]
pub struct CpuTextureData {
    extent_x: u32,
    extent_y: u32,
    extent_z: u32,
    format: vk::Format,
    data: InteropArray<u8>,
}

impl CpuTextureData {
    pub fn new(
        extent_x: u32,
        extent_y: u32,
        extent_z: u32,
        format: vk::Format,
        data: InteropArray<u8>,
    ) -> Self {
        assert_ne!(extent_x, 0);
        assert_ne!(extent_y, 0);
        assert_ne!(extent_z, 0);
        let size = extent_x * extent_y * extent_z * Self::pixel_size(format) as u32;
        assert_eq!(size, data.as_slice().len() as u32);

        Self {
            extent_x,
            extent_y,
            extent_z,
            format,
            data,
        }
    }

    pub fn extent_x(&self) -> u32 {
        self.extent_x
    }

    pub fn extent_y(&self) -> u32 {
        self.extent_y
    }

    pub fn extent_z(&self) -> u32 {
        self.extent_z
    }

    pub fn format(&self) -> &vk::Format {
        &self.format
    }

    pub fn data(&self) -> &InteropArray<u8> {
        &self.data
    }

    pub fn pixel_size(format: vk::Format) -> usize {
        type F = vk::Format;

        #[rustfmt::skip]
        let result = match format {
            F::R8_UNORM | F::R8_SNORM | F::R8_USCALED | F::R8_SSCALED | F::R8_UINT | F::R8_SINT | F::R8_SRGB => {
                1
            },
            F::R8G8_UNORM | F::R8G8_SNORM | F::R8G8_USCALED | F::R8G8_SSCALED | F::R8G8_UINT | F::R8G8_SINT | F::R8G8_SRGB => {
                2
            },
            F::R8G8B8_UNORM | F::R8G8B8_SNORM | F::R8G8B8_USCALED | F::R8G8B8_SSCALED | F::R8G8B8_UINT | F::R8G8B8_SINT | F::R8G8B8_SRGB => {
                3
            },
            F::R8G8B8A8_UNORM | F::R8G8B8A8_SNORM | F::R8G8B8A8_USCALED | F::R8G8B8A8_SSCALED | F::R8G8B8A8_UINT | F::R8G8B8A8_SINT | F::R8G8B8A8_SRGB => {
                4
            },
            F::R16_UNORM | F::R16_SNORM | F::R16_USCALED | F::R16_SSCALED | F::R16_UINT | F::R16_SINT => {
                2
            },
            F::R16G16_UNORM | F::R16G16_SNORM | F::R16G16_USCALED | F::R16G16_SSCALED | F::R16G16_UINT | F::R16G16_SINT => {
                4
            },
            F::R16G16B16_UNORM | F::R16G16B16_SNORM | F::R16G16B16_USCALED | F::R16G16B16_SSCALED | F::R16G16B16_UINT | F::R16G16B16_SINT => {
                6
            },
            F::R16G16B16A16_UNORM | F::R16G16B16A16_SNORM | F::R16G16B16A16_USCALED | F::R16G16B16A16_SSCALED | F::R16G16B16A16_UINT | F::R16G16B16A16_SINT => {
                8
            },
            F::R32G32B32_SFLOAT => {
                12
            },
            F::R32G32B32A32_SFLOAT => {
                16
            },
            _ => unimplemented!("unsupported format: {:?}", format),
        };
        result
    }
}
