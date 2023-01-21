use std::{mem, sync::Arc};

use ash::vk;

use crate::{rendering::fence::GraphicsFence, interop::prelude::{InteropResult, ResultError}};

use super::{errors::universal::VulkanUniversalError, device::VulkanDevice};

pub struct VulkanFence<'init> {
    inner: vk::Fence,
    device: Arc<VulkanDevice<'init>>
}

impl<'init> VulkanFence<'init>{
    pub fn new(device: &Arc<VulkanDevice<'init>>, inner: vk::Fence) -> Self {
        Self { inner, device: device.clone() }
    }

    pub fn inner(&self) -> vk::Fence {
        self.inner
    }

    pub fn wait(&self, timeout: u64) -> Result<bool, VulkanUniversalError> {
        unsafe {
            self.wait_inner(&[self.inner], false, timeout)
        }
    }

    /// # Safety
    /// All fences must be from the same device.
    unsafe fn wait_inner(
        &self, fences: &[vk::Fence], wait_all: bool, timeout: u64
    ) -> Result<bool, VulkanUniversalError> {
        match self.device.initialized().unwrap().vulkan_device().wait_for_fences(fences, wait_all, timeout) {
            Ok(()) => Ok(true),
            Err(err) => match err {
                vk::Result::TIMEOUT => Ok(false),
                _ => Err(err.into())
            }
        }
    }
}

impl Drop for VulkanFence<'_> {
    fn drop(&mut self) {
        unsafe {
            self.device.initialized().unwrap().vulkan_device().destroy_fence(
                self.inner, None
            );
        }
    }
}

impl GraphicsFence for VulkanFence<'_> {
    fn wait(&self, timeout: u64) -> InteropResult<bool> {
        match self.wait(timeout) {
            Ok(is_signaled) => InteropResult::with_ok(is_signaled),
            Err(err) => InteropResult::with_err(err.into()),
        }
    }

    fn is_signaled(&self) -> InteropResult<bool> {
        match unsafe {
            self.device.initialized().unwrap().vulkan_device().get_fence_status(self.inner)
        } {
            Ok(i) => InteropResult::with_ok(i),
            Err(err) => return InteropResult::with_err(err.into()),
        }
    }

    unsafe fn wait_multiple(
        &self, fences: &[&Arc<dyn GraphicsFence>], wait_all: bool, timeout: u64
    ) -> Result<bool, ResultError> {
        let f: &[&Arc<VulkanFence>] = mem::transmute(fences);
        let mut vec = Vec::with_capacity(f.len());

        for fence in f {
            vec.push(fence.inner);
        }

        Ok(self.wait_inner(&vec, wait_all, timeout)?)
    }
}
