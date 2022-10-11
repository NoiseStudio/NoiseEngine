use interop::interop_allocator::InteropAllocator;

#[global_allocator]
static GLOBAL_ALLOCATOR: InteropAllocator = InteropAllocator;

pub mod graphics;
pub mod interop;
pub mod logging;
