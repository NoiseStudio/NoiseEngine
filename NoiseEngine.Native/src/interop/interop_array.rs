use std::{mem, slice};

#[repr(C)]
pub struct InteropArray<T> {
    ptr: *mut T,
    length: i32,
}

impl<T> InteropArray<T> {
    pub fn new(length: i32) -> InteropArray<T> {
        let mut vec = Vec::<T>::with_capacity(length as usize);
        let ptr = vec.as_mut_ptr();
        mem::forget(vec);

        InteropArray {
            ptr,
            length,
        }
    }
}

impl<T> Drop for InteropArray<T> {
    fn drop(&mut self) {
        unsafe {
            if !self.ptr.is_null() {
                Vec::from_raw_parts(self.ptr, self.length as usize, self.length as usize);
            }
        }
    }
}

impl<T> From<Vec<T>> for InteropArray<T> {
    fn from(mut vec: Vec<T>) -> InteropArray<T> {
        vec.shrink_to_fit();
        let ptr = vec.as_mut_ptr();
        let length = vec.len() as i32;
        mem::forget(vec);

        InteropArray {
            ptr: ptr as *mut T,
            length,
        }
    }
}

impl<T: Clone> From<&[T]> for InteropArray<T> {
    fn from(slice: &[T]) -> InteropArray<T> {
        slice.to_vec().into()
    }
}

impl<T> From<InteropArray<T>> for Vec<T> {
    fn from(array: InteropArray<T>) -> Vec<T> {
        let vec;

        unsafe {
            vec = Vec::from_raw_parts(array.ptr, array.length as usize, array.length as usize);
        }

        mem::forget(array);
        vec
    }
}

impl<'a, T> From<&'a InteropArray<T>> for &'a [T] {
    fn from(array: &'a InteropArray<T>) -> &'a [T] {
        unsafe {
            slice::from_raw_parts(array.ptr, array.length as usize)
        }
    }
}

impl<'a, T> From<&'a mut InteropArray<T>> for &'a mut [T] {
    fn from(array: &'a mut InteropArray<T>) -> &'a mut [T] {
        unsafe {
            slice::from_raw_parts_mut(array.ptr, array.length as usize)
        }
    }
}
