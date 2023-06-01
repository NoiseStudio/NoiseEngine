use crate::rendering::cpu::{CpuTextureData, TextureFileFormat};

use anyhow::Result;
use ash::vk;

pub fn decode(
    file_data: &[u8],
    file_format: TextureFileFormat,
    format: Option<vk::Format>,
) -> Result<CpuTextureData> {
    let img =
        image::load_from_memory_with_format(file_data, file_format.into())?;

    if let Some(format) = format {
        return decode_with_format(img, format);
    }

    let width = img.width();
    let height = img.height();

    let (data, format) = match img.color() {
        image::ColorType::L8 => {
            (img.into_luma8().into_raw(), vk::Format::R8_SRGB)
        },
        image::ColorType::La8 => {
            (img.into_luma_alpha8().into_raw(), vk::Format::R8G8_SRGB)
        },
        image::ColorType::Rgb8 => {
            (img.into_rgb8().into_raw(), vk::Format::R8G8B8_SRGB)
        },
        image::ColorType::Rgba8 => {
            (img.into_rgba8().into_raw(), vk::Format::R8G8B8A8_SRGB)
        },
        image::ColorType::L16 => {
            let raw = img.into_luma16().into_raw();
            (reinterpret_vec(raw), vk::Format::R16_UNORM)
        },
        image::ColorType::La16 => {
            let raw = img.into_luma_alpha16().into_raw();
            (reinterpret_vec(raw), vk::Format::R16G16_UNORM)
        },
        image::ColorType::Rgb16 => {
            let raw = img.into_rgb16().into_raw();
            (reinterpret_vec(raw), vk::Format::R16G16B16_UNORM)
        },
        image::ColorType::Rgba16 => {
            let raw = img.into_rgba16().into_raw();
            (reinterpret_vec(raw), vk::Format::R16G16B16A16_UNORM)
        },
        // Note that the following formats are not supported by Vulkan
        // so we downgrade them to the closest supported format
        image::ColorType::Rgb32F => {
            let raw = img.into_rgb16().into_raw();
            (reinterpret_vec(raw), vk::Format::R16G16B16_UNORM)
        },
        image::ColorType::Rgba32F => {
            let raw = img.into_rgba16().into_raw();
            (reinterpret_vec(raw), vk::Format::R16G16B16A16_UNORM)
        },
        _ => anyhow::bail!("Unknown color type: {:?}", img.color()),
    };

    Ok(CpuTextureData::new(
        width,
        height,
        1,
        format,
        data.into(),
    ))
}

fn decode_with_format(
    img: image::DynamicImage,
    format: vk::Format,
) -> Result<CpuTextureData> {
    type F = vk::Format;

    let width = img.width();
    let height = img.height();

    #[rustfmt::skip]
    let data = match format {
        F::R8_UNORM | F::R8_SNORM | F::R8_USCALED | F::R8_SSCALED | F::R8_UINT | F::R8_SINT | F::R8_SRGB => {
            img.into_luma8().into_raw()
        },
        F::R8G8_UNORM | F::R8G8_SNORM | F::R8G8_USCALED | F::R8G8_SSCALED | F::R8G8_UINT | F::R8G8_SINT | F::R8G8_SRGB => {
            img.into_luma_alpha8().into_raw()
        },
        F::R8G8B8_UNORM | F::R8G8B8_SNORM | F::R8G8B8_USCALED | F::R8G8B8_SSCALED | F::R8G8B8_UINT | F::R8G8B8_SINT | F::R8G8B8_SRGB => {
            img.into_rgb8().into_raw()
        },
        F::R8G8B8A8_UNORM | F::R8G8B8A8_SNORM | F::R8G8B8A8_USCALED | F::R8G8B8A8_SSCALED | F::R8G8B8A8_UINT | F::R8G8B8A8_SINT | F::R8G8B8A8_SRGB => {
            img.into_rgba8().into_raw()
        },
        F::R16_UNORM | F::R16_SNORM | F::R16_USCALED | F::R16_SSCALED | F::R16_UINT | F::R16_SINT | F::R16_SFLOAT => {
            let raw = img.into_luma16().into_raw();
            reinterpret_vec(raw)
        },
        F::R16G16_UNORM | F::R16G16_SNORM | F::R16G16_USCALED | F::R16G16_SSCALED | F::R16G16_UINT | F::R16G16_SINT | F::R16G16_SFLOAT => {
            let raw = img.into_luma_alpha16().into_raw();
            reinterpret_vec(raw)
        },
        F::R16G16B16_UNORM | F::R16G16B16_SNORM | F::R16G16B16_USCALED | F::R16G16B16_SSCALED | F::R16G16B16_UINT | F::R16G16B16_SINT | F::R16G16B16_SFLOAT => {
            let raw = img.into_rgb16().into_raw();
            reinterpret_vec(raw)
        },
        F::R16G16B16A16_UNORM | F::R16G16B16A16_SNORM | F::R16G16B16A16_USCALED | F::R16G16B16A16_SSCALED | F::R16G16B16A16_UINT | F::R16G16B16A16_SINT | F::R16G16B16A16_SFLOAT => {
            let raw = img.into_rgba16().into_raw();
            reinterpret_vec(raw)
        },
        _ => anyhow::bail!("Unsupported format: {:?}", format)
    };

    Ok(CpuTextureData::new(
        width,
        height,
        1,
        format,
        data.into()
    ))
}

fn reinterpret_vec<T>(vec: Vec<T>) -> Vec<u8> {
    let len = vec.len() * std::mem::size_of::<T>();
    let cap = vec.capacity() * std::mem::size_of::<T>();
    let ptr = vec.as_ptr() as *const u8;
    std::mem::forget(vec);
    unsafe { Vec::from_raw_parts(ptr as *mut u8, len, cap) }
}
