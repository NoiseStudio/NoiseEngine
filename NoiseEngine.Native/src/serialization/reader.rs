use std::{mem, ptr, slice};

pub struct SerializationReader<'a> {
    pub index: usize,
    pub data: &'a [u8],
}

impl<'a> SerializationReader<'a> {
    pub fn new(data: &'a [u8]) -> SerializationReader {
        SerializationReader { index: 0, data }
    }

    pub fn read<T>(&mut self) -> Option<T> {
        let size = mem::size_of::<T>();
        if self.index + size > self.data.len() {
            return None;
        }

        let mut ptr = self.data.as_ptr() as *const u8;
        ptr = unsafe { ptr.add(self.index) };

        self.index += size;

        Some(unsafe { ptr::read_unaligned::<T>(ptr as *const T) })
    }

    pub fn read_unchecked<T>(&mut self) -> T {
        let mut ptr = self.data.as_ptr() as *const u8;
        ptr = unsafe { ptr.add(self.index) };

        self.index += mem::size_of::<T>();

        unsafe { ptr::read_unaligned::<T>(ptr as *const T) }
    }

    pub fn read_bytes_unchecked(&mut self, length: usize) -> &[u8] {
        let mut ptr = self.data.as_ptr() as *const u8;
        ptr = unsafe { ptr.add(self.index) };

        self.index += length;

        unsafe { slice::from_raw_parts(ptr, length) }
    }
}
