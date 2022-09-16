use std::slice;

#[repr(C)]
pub struct InteropSpan<T> {
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

impl<'a, T> Into<&'a [T]> for InteropSpan<T> {
    fn into(self) -> &'a [T] {
        unsafe {
            slice::from_raw_parts(self.reference, self.length as usize)
        }
    }
}

impl<'a, T> Into<InteropSpan<T>> for &'a mut [T] {
    fn into(self) -> InteropSpan<T> {
        InteropSpan {
            reference: self.as_mut_ptr(),
            length: self.len() as i32
        }
    }
}
