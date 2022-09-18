use std::{slice, marker::PhantomData};

#[repr(C)]
pub struct InteropSpan<'a, T> {
    reference: *mut T,
    length: i32,
    phantom_data: PhantomData<&'a mut T>,
}

impl<'a, T> Into<&'a mut [T]> for InteropSpan<'a, T> {
    fn into(self) -> &'a mut[T] {
        unsafe {
            slice::from_raw_parts_mut(self.reference, self.length as usize)
        }
    }
}

impl<'a, T> Into<&'a [T]> for InteropSpan<'a, T> {
    fn into(self) -> &'a [T] {
        unsafe {
            slice::from_raw_parts(self.reference, self.length as usize)
        }
    }
}

impl<'a, T> Into<InteropSpan<'a, T>> for &'a mut [T] {
    fn into(self) -> InteropSpan<'a, T> {
        InteropSpan {
            reference: self.as_mut_ptr(),
            length: self.len() as i32,
            phantom_data: PhantomData,
        }
    }
}
