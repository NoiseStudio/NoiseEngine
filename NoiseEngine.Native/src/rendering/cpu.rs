use crate::interop::prelude::InteropArray;

#[repr(C)]
pub enum CpuTextureFormat {
    R8G8B8A8,
    R8G8B8,
    R8,
    R16G16B16A16,
    R16G16B16,
    R16,
}

impl CpuTextureFormat {
    pub fn pixel_size(&self) -> usize {
        match self {
            CpuTextureFormat::R8G8B8A8 => 4,
            CpuTextureFormat::R8G8B8 => 3,
            CpuTextureFormat::R8 => 1,
            CpuTextureFormat::R16G16B16A16 => 8,
            CpuTextureFormat::R16G16B16 => 6,
            CpuTextureFormat::R16 => 2,
        }
    }
}

#[repr(C)]
pub struct CpuTextureData {
    extent_x: u32,
    extent_y: u32,
    extent_z: u32,
    format: CpuTextureFormat,
    data: InteropArray<u8>,
}

impl CpuTextureData {
    pub fn new(
        extent_x: u32,
        extent_y: u32,
        extent_z: u32,
        format: CpuTextureFormat,
        data: InteropArray<u8>,
    ) -> Self {
        assert_ne!(extent_x, 0);
        assert_ne!(extent_y, 0);
        assert_ne!(extent_z, 0);
        let size = extent_x * extent_y * extent_z * format.pixel_size() as u32;
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

    pub fn format(&self) -> &CpuTextureFormat {
        &self.format
    }

    pub fn data(&self) -> &InteropArray<u8> {
        &self.data
    }
}
