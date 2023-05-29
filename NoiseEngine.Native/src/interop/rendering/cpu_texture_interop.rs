use crate::{
    interop::prelude::{
        InteropReadOnlySpan,
        InteropResult,
        ResultError,
        ResultErrorKind,
    },
    rendering::{cpu::CpuTextureData, encoding}
};

#[no_mangle]
extern "C" fn rendering_cpu_texture_interop_decode_png(
    file_data: InteropReadOnlySpan<u8>,
) -> InteropResult<CpuTextureData> {
    let result = encoding::png::decode(file_data.into());

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
