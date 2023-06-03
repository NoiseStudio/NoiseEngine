use std::{marker::PhantomData, slice};

#[repr(C)]
pub struct InteropSpan<'a, T> {
    reference: *mut T,
    length: i32,
    phantom_data: PhantomData<&'a mut T>,
}

impl<'a, T> From<InteropSpan<'a, T>> for &'a mut [T] {
    fn from(span: InteropSpan<'a, T>) -> Self {
        unsafe { slice::from_raw_parts_mut(span.reference, span.length as usize) }
    }
}

impl<'a, T> From<InteropSpan<'a, T>> for &'a [T] {
    fn from(span: InteropSpan<'a, T>) -> Self {
        unsafe { slice::from_raw_parts(span.reference, span.length as usize) }
    }
}

impl<'a, T> From<&'a mut [T]> for InteropSpan<'a, T> {
    fn from(slice: &'a mut [T]) -> Self {
        InteropSpan {
            reference: slice.as_mut_ptr(),
            length: slice.len() as i32,
            phantom_data: PhantomData,
        }
    }
}
