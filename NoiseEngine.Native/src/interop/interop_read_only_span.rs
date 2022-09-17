use std::slice;

#[repr(C)]
pub struct InteropReadOnlySpan<T> {
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

impl<'a, T> Into<InteropReadOnlySpan<T>> for &'a [T] {
    fn into(self) -> InteropReadOnlySpan<T> {
        InteropReadOnlySpan {
            reference: self.as_ptr(),
            length: self.len() as i32
        }
    }
}