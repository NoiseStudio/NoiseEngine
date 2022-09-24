use std::{ptr::NonNull, alloc::{Allocator, AllocError, Layout}};
use libc::c_void;

pub struct InteropAllocator;

pub unsafe fn alloc(size: usize) -> *mut u8 {
    libc::malloc(size) as *mut u8
}

pub unsafe fn dealloc(ptr: *mut u8) {
    libc::free(ptr as *mut c_void);
}

unsafe impl Allocator for InteropAllocator {
    fn allocate(&self, layout: Layout) -> Result<NonNull<[u8]>, AllocError> {
        let size = layout.size();
        let ptr;

        unsafe {
            ptr = NonNull::new_unchecked(alloc(size));
        }

        Ok(NonNull::slice_from_raw_parts(ptr, size))
    }

    unsafe fn deallocate(&self, ptr: NonNull<u8>, _layout: Layout) {
        dealloc(ptr.as_ptr());
    }
}
