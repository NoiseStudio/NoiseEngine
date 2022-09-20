use noise_engine_native::interop::interop_string::InteropString;

static TEST_STRING: &str = "Hello, world!";
static TEST_STRING_NON_ASCII: &str = "Hello, 世界!";

#[no_mangle]
extern "C" fn interop_interop_string_test_unmanaged_create() -> InteropString {
    TEST_STRING.into()
}

#[no_mangle]
extern "C" fn interop_interop_string_test_unmanaged_destroy(_string: InteropString) {
}

#[no_mangle]
extern "C" fn interop_interop_string_test_unmanaged_compare(string: InteropString) -> bool {
    String::from(string) == TEST_STRING
}

#[no_mangle]
extern "C" fn interop_interop_string_test_unmanaged_create_non_ascii() -> InteropString {
    TEST_STRING_NON_ASCII.into()
}

#[no_mangle]
extern "C" fn interop_interop_string_test_unmanaged_compare_non_ascii(string: InteropString) -> bool {
    String::from(string) == TEST_STRING_NON_ASCII
}
