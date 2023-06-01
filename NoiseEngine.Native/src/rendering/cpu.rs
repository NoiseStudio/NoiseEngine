use ash::vk;

use crate::interop::prelude::InteropArray;

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
        let size =
            extent_x * extent_y * extent_z * Self::pixel_size(format) as u32;
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
        match format {
            vk::Format::R8_SRGB => 1,
            vk::Format::R8G8_SRGB => 2,
            vk::Format::R8G8B8_SRGB => 3,
            vk::Format::R8G8B8A8_SRGB => 4,
            vk::Format::R16_UNORM => 2,
            vk::Format::R16G16_UNORM => 4,
            vk::Format::R16G16B16_UNORM => 6,
            vk::Format::R16G16B16A16_UNORM => 8,
            vk::Format::R32G32B32_SFLOAT => 12,
            vk::Format::R32G32B32A32_SFLOAT => 16,
            _ => unimplemented!("unsupported format: {:?}", format),
        }
    }
}
