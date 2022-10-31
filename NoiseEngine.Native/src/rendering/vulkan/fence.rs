use std::mem;

use ash::vk;

use crate::{rendering::fence::GraphicsFence, interop::prelude::{InteropResult, ResultError}};

use super::{device_pool::VulkanDevicePool, errors::universal::VulkanUniversalError};

pub struct VulkanFence<'a> {
    pool: &'a VulkanDevicePool,
    inner: vk::Fence
}

impl<'a> VulkanFence<'a>{
    pub fn new(pool: &'a VulkanDevicePool, inner: vk::Fence) -> Self {
        Self { pool, inner }
    }

    pub fn inner(&self) -> vk::Fence {
        self.inner
    }

    /// # Safety
    /// All fences must be from the same device.
    unsafe fn wait(&self, fences: &[vk::Fence], wait_all: bool, timeout: u64) -> Result<(), VulkanUniversalError> {
        let initialized = self.pool.device().initialized()?;

        Ok(initialized.vulkan_device().wait_for_fences(fences, wait_all, timeout)?)
    }
}

impl Drop for VulkanFence<'_> {
    fn drop(&mut self) {
        let initialized = self.pool.device().initialized().unwrap();

        unsafe {
            initialized.vulkan_device().destroy_fence(self.inner, None);
        }
    }
}

impl GraphicsFence for VulkanFence<'_> {
    fn wait(&self, timeout: u64) -> InteropResult<()> {
        match unsafe {
            self.wait(&[self.inner], false, timeout)
        } {
            Ok(()) => InteropResult::with_ok(()),
            Err(err) => InteropResult::with_err(err.into()),
        }
    }

    fn is_signaled(&self) -> InteropResult<bool> {
        let initialized = match self.pool.device().initialized() {
            Ok(i) => i,
            Err(err) => return InteropResult::with_err(err.into()),
        };

        match unsafe {
            initialized.vulkan_device().get_fence_status(self.inner)
        } {
            Ok(i) => InteropResult::with_ok(i),
            Err(err) => return InteropResult::with_err(err.into()),
        }
    }

    unsafe fn wait_multiple(
        &self, fences: &[&&dyn GraphicsFence], wait_all: bool, timeout: u64
    ) -> Result<(), ResultError> {
        let f: &[&&VulkanFence] = mem::transmute(fences);
        let mut vec = Vec::with_capacity(f.len());

        for fence in f {
            vec.push(fence.inner);
        }

        Ok(self.wait(&vec, wait_all, timeout)?)
    }
}
