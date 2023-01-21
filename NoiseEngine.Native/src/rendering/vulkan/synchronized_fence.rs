use std::sync::{Arc, atomic::{Ordering, AtomicBool}};

use rsevents::{ManualResetEvent, EventState, Awaitable};

use super::{fence::VulkanFence, errors::universal::VulkanUniversalError};

pub struct VulkanSynchronizedFence<'init> {
    reset_event: ManualResetEvent,
    is_retired: AtomicBool,
    fence: Arc<VulkanFence<'init>>
}

impl<'init> VulkanSynchronizedFence<'init> {
    pub fn new(fence: Arc<VulkanFence<'init>>) -> Self {
        Self {
            reset_event: ManualResetEvent::new(EventState::Unset),
            is_retired: AtomicBool::new(false),
            fence
        }
    }

    pub fn wait(&self) -> Result<bool, VulkanUniversalError> {
        self.reset_event.wait();

        if !self.is_retired.load(Ordering::Relaxed) {
            return self.fence.wait(u64::MAX)
        }

        Ok(true)
    }

    pub fn set_reset_event(&self) {
        self.reset_event.set();
    }

    pub fn retire(&self) {
        self.is_retired.store(true, Ordering::Relaxed);
        self.set_reset_event();
    }

    pub fn fence(&self) -> &Arc<VulkanFence<'init>> {
        &self.fence
    }
}
