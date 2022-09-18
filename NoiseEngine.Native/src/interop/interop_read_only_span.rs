use std::{slice, marker::PhantomData};

#[repr(C)]
pub struct InteropReadOnlySpan<'a, T> {
    reference: *const T,
    length: i32,
    phantom_data: PhantomData<&'a T>
}

impl<'a, T> Into<&'a [T]> for InteropReadOnlySpan<'a, T> {
    fn into(self) -> &'a [T] {
        unsafe {
            slice::from_raw_parts(self.reference, self.length as usize)
        }
    }
}

impl<'a, T> Into<InteropReadOnlySpan<'a, T>> for &'a [T] {
    fn into(self) -> InteropReadOnlySpan<'a, T> {
        InteropReadOnlySpan {
            reference: self.as_ptr(),
            length: self.len() as i32,
            phantom_data: PhantomData,
        }
    }
}
