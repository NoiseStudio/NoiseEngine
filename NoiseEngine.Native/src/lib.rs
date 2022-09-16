#[no_mangle]
pub extern "C" fn add(left: i32, right: i32) -> i32 {
    left + right
}

pub fn return_42() -> i32 {
    42
}
