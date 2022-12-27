use std::{mem, ptr, sync::Arc};

use ash::vk;

use crate::errors::invalid_operation::InvalidOperationError;

use super::{errors::universal::VulkanUniversalError, device::{VulkanDevice, VulkanDeviceInitialized}};

pub struct ShaderModule<'init> {
    initialized: &'init VulkanDeviceInitialized<'init>,
    inner: vk::ShaderModule,
    _device: Arc<VulkanDevice<'init>>,
}

impl<'dev: 'init, 'init> ShaderModule<'init> {
    pub fn new(device: &'dev Arc<VulkanDevice<'init>>, code: &[u8]) -> Result<Self, VulkanUniversalError> {
        let size = mem::size_of::<u32>();
        if code.len() % size != 0 {
            return Err(InvalidOperationError::new(
                format!("Given code length must be a multiple of {} bytes.", size)
            ).into())
        }

        let create_info = vk::ShaderModuleCreateInfo {
            s_type: vk::StructureType::SHADER_MODULE_CREATE_INFO,
            p_next: ptr::null(),
            flags: vk::ShaderModuleCreateFlags::empty(),
            code_size: code.len(),
            p_code: code.as_ptr() as *const u32,
        };

        let initialized = device.initialized()?;
        let inner = unsafe {
            initialized.vulkan_device().create_shader_module(&create_info, None)
        }?;

        Ok(Self { initialized, inner, _device: device.clone() })
    }

    pub fn inner(&self) -> vk::ShaderModule {
        self.inner
    }
}

impl Drop for ShaderModule<'_> {
    fn drop(&mut self) {
        unsafe {
            self.initialized.vulkan_device().destroy_shader_module(self.inner, None);
        }
    }
}
