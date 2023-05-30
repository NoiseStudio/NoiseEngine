use ash::vk;

use crate::interop::prelude::InteropArray;

#[repr(C)]
pub enum TextureFileFormat {
    Png,
    Jpeg,
    Webp,
}

impl From<TextureFileFormat> for image::ImageFormat {
    fn from(format: TextureFileFormat) -> Self {
        match format {
            TextureFileFormat::Png => image::ImageFormat::Png,
            TextureFileFormat::Jpeg => image::ImageFormat::Jpeg,
            TextureFileFormat::Webp => image::ImageFormat::WebP,
        }
    }
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

    fn pixel_size(format: vk::Format) -> usize {
        match format {
            vk::Format::R8_UINT => 1,
            vk::Format::R8G8_UINT => 2,
            vk::Format::R8G8B8_UINT => 3,
            vk::Format::R8G8B8A8_UINT => 4,
            vk::Format::R16_UINT => 2,
            vk::Format::R16G16_UINT => 4,
            vk::Format::R16G16B16_UINT => 6,
            vk::Format::R16G16B16A16_UINT => 8,
            _ => unimplemented!("unsupported format: {:?}", format),
        }
    }
}
