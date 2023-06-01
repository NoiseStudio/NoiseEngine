use ash::vk;

use crate::{
    interop::prelude::{
        InteropReadOnlySpan,
        InteropResult,
        ResultError,
        ResultErrorKind, InteropOption,
    },
    rendering::{cpu::CpuTextureData, encoding}
};

#[no_mangle]
extern "C" fn rendering_cpu_texture_interop_decode(
    file_data: InteropReadOnlySpan<u8>,
    format: InteropOption<vk::Format>,

) -> InteropResult<CpuTextureData> {
    let result =
        encoding::decode(file_data.into(), format.into());

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
