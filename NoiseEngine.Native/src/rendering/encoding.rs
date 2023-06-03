use std::io::Cursor;

use crate::rendering::cpu::CpuTextureData;

use anyhow::{Result, Context};
use ash::vk;
use image::{ImageBuffer, ColorType, DynamicImage};

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
        ColorType::L8 => {
            (
                img.into_luma8().into_raw(),
                format.unwrap_or(vk::Format::R8_SRGB),
            )
        },
        ColorType::La8 => {
            (
                img.into_luma_alpha8().into_raw(),
                format.unwrap_or(vk::Format::R8G8_SRGB),
            )
        },
        ColorType::Rgb8 => {
            (
                img.into_rgb8().into_raw(),
                format.unwrap_or(vk::Format::R8G8B8_SRGB),
            )
        },
        ColorType::Rgba8 => {
            (
                img.into_rgba8().into_raw(),
                format.unwrap_or(vk::Format::R8G8B8A8_SRGB),
            )
        },
        ColorType::L16 => {
            let raw = img.into_luma16().into_raw();
            (
                reinterpret_vec(raw),
                format.unwrap_or(vk::Format::R16_UNORM),
            )
        },
        ColorType::La16 => {
            let raw = img.into_luma_alpha16().into_raw();
            (
                reinterpret_vec(raw),
                format.unwrap_or(vk::Format::R16G16_UNORM),
            )
        },
        ColorType::Rgb16 => {
            let raw = img.into_rgb16().into_raw();
            (
                reinterpret_vec(raw),
                format.unwrap_or(vk::Format::R16G16B16_UNORM),
            )
        },
        ColorType::Rgba16 => {
            let raw = img.into_rgba16().into_raw();
            (
                reinterpret_vec(raw),
                format.unwrap_or(vk::Format::R16G16B16A16_UNORM),
            )
        },
        // Note that the following formats are not supported by Vulkan
        // so we downgrade them to the closest supported format
        ColorType::Rgb32F => {
            let raw = img.into_rgb16().into_raw();
            (
                reinterpret_vec(raw),
                format.unwrap_or(vk::Format::R16G16B16_UNORM),
            )
        },
        ColorType::Rgba32F => {
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

    match file_format {
        TextureFileFormat::Webp => {
            return encode_webp(
                data,
                texture.extent_x(),
                texture.extent_y(),
                color_type,
                quality,
            );
        },
        _ => {},
    };

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
            TextureFileFormat::Webp => unreachable!(),
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

fn interpret_vec<T>(vec: Vec<u8>) -> Vec<T> {
    if vec.len() % std::mem::size_of::<T>() != 0 {
        panic!("Invalid size");
    }

    let len = vec.len() / std::mem::size_of::<T>();
    let cap = vec.capacity() / std::mem::size_of::<T>();
    let ptr = vec.as_ptr() as *mut T;
    std::mem::forget(vec);
    unsafe { Vec::from_raw_parts(ptr, len, cap) }
}

fn encode_webp(
    data: &[u8],
    width: u32,
    height: u32,
    color_type: ColorType,
    quality: Option<u8>,
) -> Result<Vec<u8>> {
    use webp::Encoder;


    let data = data.to_vec();

    match color_type {
        ColorType::L8 => {
            let img = ImageBuffer::<image::Luma<u8>, _>::from_vec(
                width,
                height,
                data,
            ).context(FAIL_MSG)?;

            let img = img.into();
            let encoder = Encoder::from_image(&img);

            let encoder = match encoder {
                Ok(encoder) => encoder,
                Err(_) => anyhow::bail!(FAIL_MSG)
            };

            let result = match quality {
                Some(quality) => encoder.encode(quality as f32),
                None => encoder.encode_lossless(),
            };

            return Ok(result.to_vec());
        },
        _ => anyhow::bail!("Unsupported color type {:?}", color_type),
    };
}

fn encode_webp_helper<T, P: image::Pixel<Subpixel = T>>(
    data: Vec<u8>,
    width: u32,
    height: u32,
    quality: Option<u8>,
) -> Result<Vec<u8>> where DynamicImage: From<ImageBuffer<P, Vec<T>>> {
    let img = ImageBuffer::<P, T>::from_vec(
        width,
        height,
        interpret_vec::<T>(data),
    ).context("Failed to encode image")?;

    let img = img.into();
    let encoder = webp::Encoder::from_image(&img);

    let encoder = match encoder {
        Ok(encoder) => encoder,
        Err(_) => anyhow::bail!("Failed to encode image")
    };

    let result = match quality {
        Some(quality) => encoder.encode(quality as f32),
        None => encoder.encode_lossless(),
    };

    Ok(result.to_vec())
}
