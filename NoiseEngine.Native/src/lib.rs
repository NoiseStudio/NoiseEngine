#[macro_use]
extern crate lazy_static;

use interop::interop_allocator::InteropAllocator;

#[global_allocator]
static GLOBAL_ALLOCATOR: InteropAllocator = InteropAllocator;

pub mod interop;
