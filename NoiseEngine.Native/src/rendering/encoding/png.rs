use crate::{
    interop::interop_array::InteropArray,
    rendering::cpu::CpuTextureData,
};

use anyhow::Result;
use ash::vk;

pub fn decode(
    file_data: &[u8],
) -> Result<CpuTextureData> {
    let decoder = png::Decoder::new(file_data);

    let mut info = decoder.read_info()?;
    let (mut color_type, mut bit_depth) = info.output_color_type();

    match bit_depth {
        png::BitDepth::Eight | png::BitDepth::Sixteen => {
        },
        _ => {
            let mut decoder = png::Decoder::new(file_data);
            decoder.set_transformations(
                png::Transformations::normalize_to_color8()
            );

            info = decoder.read_info()?;
            (color_type, bit_depth) = info.output_color_type();
        },
    };

    let format = match (color_type, bit_depth) {
        (png::ColorType::Grayscale, png::BitDepth::Eight) => {
            vk::Format::R8_UINT
        },
        (png::ColorType::Grayscale, png::BitDepth::Sixteen) => {
            vk::Format::R16_UINT
        },
        (png::ColorType::GrayscaleAlpha, png::BitDepth::Eight) => {
            vk::Format::R8G8_UINT
        },
        (png::ColorType::GrayscaleAlpha, png::BitDepth::Sixteen) => {
            vk::Format::R16G16_UINT
        },
        (png::ColorType::Rgb, png::BitDepth::Eight) => {
            vk::Format::R8G8B8_UINT
        },
        (png::ColorType::Rgb, png::BitDepth::Sixteen) => {
            vk::Format::R16G16B16_UINT
        },
        (png::ColorType::Rgba, png::BitDepth::Eight) => {
            vk::Format::R8G8B8A8_UINT
        },
        (png::ColorType::Rgba, png::BitDepth::Sixteen) => {
            vk::Format::R16G16B16A16_UINT
        },
        _ => unreachable!("set_transformations should disallow other state"),
    };

    let mut data = InteropArray::new(info.output_buffer_size() as i32);

    let frame = info.next_frame(data.as_mut_slice())?;

    Ok(CpuTextureData::new(
        frame.width,
        frame.height,
        1,
        format,
        data,
    ))
}
