use std::{marker::PhantomData, slice};

#[repr(C)]
pub struct InteropSpan<T> {
    phantom: PhantomData<T>,
    reference: *mut T,
    length: i32,
}

impl<'a, T> Into<&'a mut [T]> for InteropSpan<T> {
    fn into(self) -> &'a mut[T] {
        unsafe {
            slice::from_raw_parts_mut(self.reference, self.length as usize)
        }
    }
}

impl<'a, T> From<&'a mut [T]> for InteropSpan<T> {
    fn from(src: &'a mut [T]) -> InteropSpan<T> {
        InteropSpan {
            phantom: PhantomData,
            reference: src.as_mut_ptr(),
            length: src.len() as i32
        }
    }
}
