use std::borrow::BorrowMut;

use noise_engine_native::interop::interop_array::InteropArray;

#[no_mangle]
extern "C" fn interop_interop_array_test_unmanaged_create(length: i32) -> InteropArray<i32> {
    let mut array = InteropArray::new(length);
    let slice: &mut [i32] = array.borrow_mut().into();

    for (index, value) in slice.into_iter().enumerate() {
        *value = index as i32;
    }

    array
}

#[no_mangle]
extern "C" fn interop_interop_array_test_unmanaged_create_from_vec(length: i32) -> InteropArray<i32> {
    let mut vec = Vec::with_capacity(length as usize);

    for i in 0..length {
        vec.push(i);
    }

    vec.into()
}

#[no_mangle]
extern "C" fn interop_interop_array_test_unmanaged_destroy(_array: InteropArray<i32>) {
}

#[no_mangle]
extern "C" fn interop_interop_array_test_unmanaged_destroy_vec(array: InteropArray<i32>) {
    let _vec: Vec::<i32> = array.into();
}

#[no_mangle]
extern "C" fn interop_interop_array_test_unmanaged_read(array: &InteropArray<i32>, index: i32) -> i32 {
    let slice: &[i32] = array.into();
    slice[index as usize]
}

#[no_mangle]
extern "C" fn interop_interop_array_test_unmanaged_write(array: &mut InteropArray<i32>, index: i32, value: i32) {
    let slice: &mut [i32] = array.into();
    slice[index as usize] = value;
}

#[no_mangle]
extern "C" fn interop_interop_array_test_unmanaged_as_slice(array: InteropArray<i32>) -> InteropArray<i32> {
    Vec::from(array.as_slice()).into()
}

#[no_mangle]
extern "C" fn interop_interop_array_test_unmanaged_as_mut_slice(mut array: InteropArray<i32>) -> InteropArray<i32> {
    Vec::from(array.as_mut_slice()).into()
}
