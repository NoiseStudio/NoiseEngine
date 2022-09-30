#![crate_type = "dylib"]

use interop::interop_allocator::InteropAllocator;

#[global_allocator]
static GLOBAL_ALLOCATOR: InteropAllocator = InteropAllocator;

pub mod interop;
pub mod logging;
