use noise_engine_native;

#[no_mangle]
pub extern "C" fn return_42() -> i32 {
    noise_engine_native::return_42()
}
