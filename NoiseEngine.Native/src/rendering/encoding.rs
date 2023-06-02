use std::io::Cursor;

use crate::rendering::cpu::CpuTextureData;

use anyhow::{Result, Context};
use ash::vk;

use super::cpu::{self, TextureFileFormat};

pub fn decode(
    file_data: &[u8],
    format: Option<vk::Format>,
) -> Result<CpuTextureData> {
    let img =
        image::load_from_memory(file_data)?;

    let mut img_color = img.color();

    if let Some(format) = format {
        img_color = match cpu::vk_format_to_color_type(format) {
            Some(color_type) => color_type,
            None => anyhow::bail!("Unsupported format: {:?}", format),
        };
    }

    let width = img.width();
    let height = img.height();

    let (data, format) = match img_color {
        image::ColorType::L8 => {
            (
                img.into_luma8().into_raw(),
                format.unwrap_or(vk::Format::R8_SRGB),
            )
        },
        image::ColorType::La8 => {
            (
                img.into_luma_alpha8().into_raw(),
                format.unwrap_or(vk::Format::R8G8_SRGB),
            )
        },
        image::ColorType::Rgb8 => {
            (
                img.into_rgb8().into_raw(),
                format.unwrap_or(vk::Format::R8G8B8_SRGB),
            )
        },
        image::ColorType::Rgba8 => {
            (
                img.into_rgba8().into_raw(),
                format.unwrap_or(vk::Format::R8G8B8A8_SRGB),
            )
        },
        image::ColorType::L16 => {
            let raw = img.into_luma16().into_raw();
            (
                reinterpret_vec(raw),
                format.unwrap_or(vk::Format::R16_UNORM),
            )
        },
        image::ColorType::La16 => {
            let raw = img.into_luma_alpha16().into_raw();
            (
                reinterpret_vec(raw),
                format.unwrap_or(vk::Format::R16G16_UNORM),
            )
        },
        image::ColorType::Rgb16 => {
            let raw = img.into_rgb16().into_raw();
            (
                reinterpret_vec(raw),
                format.unwrap_or(vk::Format::R16G16B16_UNORM),
            )
        },
        image::ColorType::Rgba16 => {
            let raw = img.into_rgba16().into_raw();
            (
                reinterpret_vec(raw),
                format.unwrap_or(vk::Format::R16G16B16A16_UNORM),
            )
        },
        // Note that the following formats are not supported by Vulkan
        // so we downgrade them to the closest supported format
        image::ColorType::Rgb32F => {
            let raw = img.into_rgb16().into_raw();
            (
                reinterpret_vec(raw),
                format.unwrap_or(vk::Format::R16G16B16_UNORM),
            )
        },
        image::ColorType::Rgba32F => {
            let raw = img.into_rgba16().into_raw();
            (
                reinterpret_vec(raw),
                format.unwrap_or(vk::Format::R16G16B16A16_UNORM),
            )
        },
        _ => anyhow::bail!("Unknown color type: {:?}", img_color),
    };

    Ok(CpuTextureData::new(
        width,
        height,
        1,
        format,
        data.into(),
    ))
}

pub fn encode(
    texture: &CpuTextureData,
    file_format: TextureFileFormat,
    quality: Option<u8>,
) -> Result<Vec<u8>> {
    anyhow::ensure!(
        texture.extent_z() == 1,
        "3D textures are not supported"
    );

    let color_type = cpu::vk_format_to_color_type(*texture.format())
        .context("Invalid format")?;

    let data = texture.data().into();
    let mut result = Cursor::new(Vec::new());

    image::write_buffer_with_format(
        &mut result,
        data,
        texture.extent_x(),
        texture.extent_y(),
        color_type,
        match file_format {
            TextureFileFormat::Png => image::ImageOutputFormat::Png,
            TextureFileFormat::Jpeg => image::ImageOutputFormat::Jpeg(
                quality.unwrap_or(75),
            ),
            TextureFileFormat::Webp => todo!(),
        },
    )?;

    Ok(result.into_inner())
}

fn reinterpret_vec<T>(vec: Vec<T>) -> Vec<u8> {
    let len = vec.len() * std::mem::size_of::<T>();
    let cap = vec.capacity() * std::mem::size_of::<T>();
    let ptr = vec.as_ptr() as *mut u8;
    std::mem::forget(vec);
    unsafe { Vec::from_raw_parts(ptr, len, cap) }
}
