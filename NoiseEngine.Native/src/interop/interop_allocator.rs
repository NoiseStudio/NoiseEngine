use std::alloc::{GlobalAlloc, Layout};

use libc::c_void;

pub struct InteropAllocator;

/// # Safety
/// This unsafy calls the malloc function of the libc.
pub unsafe fn alloc(size: usize, aligment: usize) -> *mut u8 {
    libc::aligned_malloc(size, aligment) as *mut u8
}

/// # Safety
/// This unsafy calls the free function of the libc.
pub unsafe fn dealloc(ptr: *mut u8) {
    libc::aligned_free(ptr as *mut c_void);
}

unsafe impl GlobalAlloc for InteropAllocator {
    unsafe fn alloc(&self, layout: Layout) -> *mut u8 {
        alloc(layout.size(), layout.align())
    }

    unsafe fn dealloc(&self, ptr: *mut u8, _layout: Layout) {
        dealloc(ptr);
    }
}

// INFO: Future Allocator implementation when Allocator will be stable.
/*unsafe impl Allocator for InteropAllocator {
    fn allocate(&self, layout: Layout) -> Result<NonNull<[u8]>, AllocError> {
        let size = layout.size();
        let ptr;

        unsafe {
            ptr = NonNull::new(alloc(size)).ok_or(AllocError)?;
        }

        Ok(NonNull::slice_from_raw_parts(ptr, size))
    }

    unsafe fn deallocate(&self, ptr: NonNull<u8>, _layout: Layout) {
        dealloc(ptr.as_ptr());
    }
}*/
