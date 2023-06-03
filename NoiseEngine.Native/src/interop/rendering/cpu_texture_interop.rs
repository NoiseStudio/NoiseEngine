use ash::vk;

use crate::{
    interop::prelude::{
        InteropReadOnlySpan,
        InteropResult,
        ResultError,
        ResultErrorKind, InteropOption, InteropArray,
    },
    rendering::{cpu_texture_2d::{CpuTextureData, self}, cpu_texture_2d_encoding}
};

#[no_mangle]
extern "C" fn rendering_cpu_texture_interop_decode(
    file_data: InteropReadOnlySpan<u8>,
    format: InteropOption<vk::Format>,
) -> InteropResult<CpuTextureData> {
    let result =
        cpu_texture_2d_encoding::decode(file_data.into(), format.into());

    match result {
        Ok(data) => {
            InteropResult::with_ok(data)
        },
        Err(error) => {
            InteropResult::with_err(ResultError::with_kind(
                &*Box::<dyn std::error::Error>::from(error),
                ResultErrorKind::Argument,
            ))
        },
    }
}

#[no_mangle]
extern "C" fn rendering_cpu_texture_interop_encode(
    data: InteropReadOnlySpan<u8>,
    width: u32,
    height: u32,
    format: vk::Format,
    file_format: cpu_texture_2d::TextureFileFormat,
    quality: InteropOption<u8>,
) -> InteropResult<InteropArray<u8>> {
    let result = cpu_texture_2d_encoding::encode(
        data.into(),
        width,
        height,
        format,
        file_format,
        quality.into(),
    );

    match result {
        Ok(data) => {
            InteropResult::with_ok(data.into())
        },
        Err(error) => {
            InteropResult::with_err(ResultError::with_kind(
                &*Box::<dyn std::error::Error>::from(error),
                ResultErrorKind::Argument,
            ))
        },
    }
}
