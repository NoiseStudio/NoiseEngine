use std::{marker::PhantomData, slice};

#[repr(C)]
pub struct InteropReadOnlySpan<T> {
    phantom: PhantomData<T>,
    reference: *const T,
    length: i32,
}

impl<'a, T> Into<&'a [T]> for InteropReadOnlySpan<T> {
    fn into(self) -> &'a [T] {
        unsafe {
            slice::from_raw_parts(self.reference, self.length as usize)
        }
    }
}

impl<'a, T> From<&'a [T]> for InteropReadOnlySpan<T> {
    fn from(src: &'a [T]) -> InteropReadOnlySpan<T> {
        InteropReadOnlySpan {
            phantom: PhantomData,
            reference: src.as_ptr(),
            length: src.len() as i32
        }
    }
}
