use std::{sync::Arc, ptr};

use ash::vk;

use crate::rendering::texture_sampler::{TextureSamplerCreateInfo, TextureSampler};

use super::{device::VulkanDevice, errors::universal::VulkanUniversalError};

pub struct VulkanSampler<'init> {
    inner: vk::Sampler,
    device: Arc<VulkanDevice<'init>>,
}

impl<'init> VulkanSampler<'init> {
    pub fn new(
        device: &Arc<VulkanDevice<'init>>, create_info: TextureSamplerCreateInfo
    ) -> Result<Self, VulkanUniversalError> {
        let vk_create_info = vk::SamplerCreateInfo {
            s_type: vk::StructureType::SAMPLER_CREATE_INFO,
            p_next: ptr::null(),
            flags: vk::SamplerCreateFlags::default(),
            mag_filter: vk::Filter::LINEAR,
            min_filter: vk::Filter::LINEAR,
            mipmap_mode: vk::SamplerMipmapMode::LINEAR,
            address_mode_u: vk::SamplerAddressMode::REPEAT,
            address_mode_v: vk::SamplerAddressMode::REPEAT,
            address_mode_w: vk::SamplerAddressMode::REPEAT,
            mip_lod_bias: 0.0,
            anisotropy_enable: match create_info.max_anisotropy > 0.0 {
                true => vk::TRUE,
                false => vk::FALSE,
            },
            max_anisotropy: match create_info.max_anisotropy > 0.0 {
                true => create_info.max_anisotropy,
                false => 0.0,
            },
            compare_enable: vk::FALSE,
            compare_op: vk::CompareOp::ALWAYS,
            min_lod: 0.0,
            max_lod: 0.0,
            border_color: vk::BorderColor::INT_OPAQUE_BLACK,
            unnormalized_coordinates: vk::FALSE,
        };

        let initialized = device.initialized().unwrap();
        let inner = unsafe {
            initialized.vulkan_device().create_sampler(&vk_create_info, None)
        }?;

        Ok(Self {
            inner,
            device: device.clone(),
        })
    }

    pub fn inner(&self) -> vk::Sampler {
        self.inner
    }
}

impl Drop for VulkanSampler<'_> {
    fn drop(&mut self) {
        unsafe {
            self.device
                .initialized()
                .unwrap()
                .vulkan_device()
                .destroy_sampler(self.inner, None);
        }
    }
}

impl TextureSampler for VulkanSampler<'_> {}
