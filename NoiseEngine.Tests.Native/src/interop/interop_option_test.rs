use noise_engine_native::interop::interop_option::InteropOption;

#[no_mangle]
extern "C" fn interop_interop_option_test_managed_read(value: i32) -> InteropOption<i32> {
    Some(value).into()
}

#[no_mangle]
extern "C" fn interop_interop_option_test_managed_read_none() -> InteropOption<i32> {
    None.into()
}

#[no_mangle]
extern "C" fn interop_interop_option_test_unmanaged_read(option: InteropOption<i32>) -> i32 {
    Option::from(option).unwrap()
}

#[no_mangle]
extern "C" fn interop_interop_option_test_unmanaged_read_none(option: InteropOption<i32>) -> bool {
    Option::<i32>::from(option).is_none()
}