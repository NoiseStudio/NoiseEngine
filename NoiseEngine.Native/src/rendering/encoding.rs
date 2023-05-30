use crate::rendering::cpu::{CpuTextureData, TextureFileFormat};

use anyhow::{anyhow, Result};
use ash::vk;

pub fn decode(
    file_data: &[u8],
    format: TextureFileFormat,
) -> Result<CpuTextureData> {
    let img = image::load_from_memory_with_format(file_data, format.into())?;
    let width = img.width();
    let height = img.height();

    let (data, format) = match img.color() {
        image::ColorType::L8 => {
            (img.into_luma8().into_raw(), vk::Format::R8_UINT)
        },
        image::ColorType::La8 => {
            (img.into_luma_alpha8().into_raw(), vk::Format::R8G8_UINT)
        },
        image::ColorType::Rgb8 => {
            (img.into_rgb8().into_raw(), vk::Format::R8G8B8_UINT)
        },
        image::ColorType::Rgba8 => {
            (img.into_rgba8().into_raw(), vk::Format::R8G8B8A8_UINT)
        },
        image::ColorType::L16 => {
            let raw = img.into_luma16().into_raw();
            (reinterpret_vec(raw), vk::Format::R16_UINT)
        },
        image::ColorType::La16 => {
            let raw = img.into_luma_alpha16().into_raw();
            (reinterpret_vec(raw), vk::Format::R16G16_UINT)
        },
        image::ColorType::Rgb16 => {
            let raw = img.into_rgb16().into_raw();
            (reinterpret_vec(raw), vk::Format::R16G16B16_UINT)
        },
        image::ColorType::Rgba16 => {
            let raw = img.into_rgba16().into_raw();
            (reinterpret_vec(raw), vk::Format::R16G16B16A16_UINT)
        },
        image::ColorType::Rgb32F => {
            let raw = img.into_rgb32f().into_raw();
            (reinterpret_vec(raw), vk::Format::R32G32B32_SFLOAT)
        },
        image::ColorType::Rgba32F => {
            let raw = img.into_rgba32f().into_raw();
            (reinterpret_vec(raw), vk::Format::R32G32B32A32_SFLOAT)
        },
        _ => return Err(anyhow!("Unknown color type: {:?}", img.color())),
    };

    Ok(CpuTextureData::new(
        width,
        height,
        1,
        format,
        data.into(),
    ))
}

fn reinterpret_vec<T>(vec: Vec<T>) -> Vec<u8> {
    let len = vec.len() * std::mem::size_of::<T>();
    let cap = vec.capacity() * std::mem::size_of::<T>();
    let ptr = vec.as_ptr() as *const u8;
    std::mem::forget(vec);
    unsafe { Vec::from_raw_parts(ptr as *mut u8, len, cap) }
}
