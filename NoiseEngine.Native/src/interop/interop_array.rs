use std::{mem, slice};

use super::interop_allocator::{self, InteropAllocator};

#[repr(C)]
pub struct InteropArray<T> {
    ptr: *mut T,
    length: i32,
}

impl<T> InteropArray<T> {
    pub fn new(length: i32) -> InteropArray<T> {
        let size = (length as usize) * mem::size_of::<T>();
        let ptr;

        unsafe {
            ptr = interop_allocator::alloc(size) as *mut T;
        }

        InteropArray {
            ptr,
            length,
        }
    }
}

impl<T> Drop for InteropArray<T> {
    fn drop(&mut self) {
        if !self.ptr.is_null() {
            unsafe {
                interop_allocator::dealloc(self.ptr as *mut u8);
            }
        }
    }
}

impl<T> From<Vec<T, InteropAllocator>> for InteropArray<T> {
    fn from(mut vec: Vec<T, InteropAllocator>) -> InteropArray<T> {
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

impl<T> From<InteropArray<T>> for Vec<T, InteropAllocator> {
    fn from(array: InteropArray<T>) -> Vec<T, InteropAllocator> {
        let vec;
        
        unsafe {
            vec = Vec::from_raw_parts_in(
                array.ptr, array.length as usize, array.length as usize, InteropAllocator
            );
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
