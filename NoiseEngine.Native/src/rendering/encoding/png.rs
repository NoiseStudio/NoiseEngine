use crate::{
    interop::interop_array::InteropArray,
    rendering::cpu::{CpuTextureFormat, CpuTextureData},
};

use anyhow::{anyhow, Result};

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
            CpuTextureFormat::R8
        },
        (png::ColorType::Grayscale, png::BitDepth::Sixteen) => {
            CpuTextureFormat::R16
        },
        (png::ColorType::GrayscaleAlpha, _) => {
            return Err(anyhow!("GrayscaleAlpha is not supported"));
        },
        (png::ColorType::Rgb, png::BitDepth::Eight) => {
            CpuTextureFormat::R8G8B8
        },
        (png::ColorType::Rgb, png::BitDepth::Sixteen) => {
            CpuTextureFormat::R16G16B16
        },
        (png::ColorType::Rgba, png::BitDepth::Eight) => {
            CpuTextureFormat::R8G8B8A8
        },
        (png::ColorType::Rgba, png::BitDepth::Sixteen) => {
            CpuTextureFormat::R16G16B16A16
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
