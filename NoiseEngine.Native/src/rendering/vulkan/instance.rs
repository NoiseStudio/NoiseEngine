use std::{ptr, mem, ffi::{CStr, CString}, sync::Arc};

use ash::vk;
use libc::c_void;
use uuid::Uuid;

use crate::{
    interop::rendering::vulkan::{application_info::VulkanApplicationInfo, device_value::VulkanDeviceValue},
    logging::{logger, log_level::LogLevel, log}, errors::null_reference::NullReferenceError
};

use super::{device::VulkanDevice, errors::universal::VulkanUniversalError};

pub struct VulkanInstance {
    inner: ash::Instance,
    library: Arc<ash::Entry>
}

impl VulkanInstance {
    pub(crate) fn new(
        library: &Arc<ash::Entry>, application_info: VulkanApplicationInfo,
        log_severity: vk::DebugUtilsMessageSeverityFlagsEXT, log_type: vk::DebugUtilsMessageTypeFlagsEXT,
        validation: bool, surface: bool
    ) -> Result<Self, VulkanUniversalError>  {
        let create_info;
        let p_application_info =
            Result::<vk::ApplicationInfo, VulkanUniversalError>::from(application_info)?;

        if (log_severity.is_empty() || log_type.is_empty()) && !validation {
            create_info = vk::InstanceCreateInfo {
                s_type: vk::StructureType::INSTANCE_CREATE_INFO,
                p_next: ptr::null(),
                flags: vk::InstanceCreateFlags::empty(),
                p_application_info: &p_application_info,
                enabled_layer_count: 0,
                pp_enabled_layer_names: ptr::null(),
                enabled_extension_count: 0,
                pp_enabled_extension_names: ptr::null(),
            };

            return match unsafe { library.create_instance(&create_info, None) } {
                Ok(instance) => Ok(Self { inner: instance, library: library.clone() }),
                Err(err) => Err(err.into())
            }
        }

        let mut enabled_extensions: Vec<*const i8> = Vec::new();
        let mut enabled_layers: Vec<*const i8> = Vec::new();

        let validation_layer;
        if validation {
            enabled_extensions.push(ash::extensions::ext::DebugUtils::name().as_ptr());

            validation_layer = match CString::new("VK_LAYER_KHRONOS_validation") {
                Ok(str) => str,
                Err(_) => return Err(NullReferenceError::default().into())
            };
            enabled_layers.push(validation_layer.as_ptr());
        }

        let surface_extension;
        let surface_extension_platform;
        if surface {
            surface_extension = CString::new("VK_KHR_surface").unwrap();
            enabled_extensions.push(surface_extension.as_ptr());

            surface_extension_platform = CString::new("VK_KHR_win32_surface").unwrap();
            enabled_extensions.push(surface_extension_platform.as_ptr());
        }

        let messenger_create_info = vk::DebugUtilsMessengerCreateInfoEXT {
            s_type: vk::StructureType::DEBUG_UTILS_MESSENGER_CREATE_INFO_EXT,
            p_next: ptr::null(),
            flags: vk::DebugUtilsMessengerCreateFlagsEXT::empty(),
            message_severity: log_severity,
            message_type: log_type,
            pfn_user_callback: Some(log_callback),
            p_user_data: ptr::null_mut(),
        };

        create_info = vk::InstanceCreateInfo {
            s_type: vk::StructureType::INSTANCE_CREATE_INFO,
            p_next: &messenger_create_info as *const vk::DebugUtilsMessengerCreateInfoEXT as *const c_void,
            flags: vk::InstanceCreateFlags::empty(),
            p_application_info: &p_application_info,
            enabled_layer_count: enabled_layers.len() as u32,
            pp_enabled_layer_names: enabled_layers.as_ptr(),
            enabled_extension_count: enabled_extensions.len() as u32,
            pp_enabled_extension_names: enabled_extensions.as_ptr(),
        };

        match unsafe { library.create_instance(&create_info, None) } {
            Ok(instance) => Ok(Self { inner: instance, library: library.clone() }),
            Err(err) => Err(err.into())
        }
    }

    pub(crate) fn get_vulkan_physical_devices(
        instance: &Arc<Self>
    ) -> Result<Vec<VulkanDeviceValue>, VulkanUniversalError> {
        let physical_devices = unsafe { instance.inner.enumerate_physical_devices() }?;
        let mut result = Vec::with_capacity(physical_devices.len());

        for physical_device in physical_devices {
            // Properties.
            let mut id_properties = vk::PhysicalDeviceIDProperties::default();
            let mut properties2 = vk::PhysicalDeviceProperties2 {
                s_type: vk::StructureType::PHYSICAL_DEVICE_PROPERTIES_2,
                p_next: &mut id_properties as *mut vk::PhysicalDeviceIDProperties as *mut c_void,
                properties: vk::PhysicalDeviceProperties::default(),
            };

            unsafe {
                instance.inner.get_physical_device_properties2(physical_device, &mut properties2)
            };

            let properties = properties2.properties;

            // Queue families.
            let queue_family_properties = unsafe {
                instance.inner.get_physical_device_queue_family_properties(physical_device)
            };

            let mut supports_graphics = false;
            let mut supports_computing = false;
            for queue_family_properties in queue_family_properties {
                supports_graphics |= queue_family_properties.queue_flags.contains(vk::QueueFlags::GRAPHICS);
                supports_computing |= queue_family_properties.queue_flags.contains(vk::QueueFlags::COMPUTE);
            }

            // Presentation.
            let extensions = unsafe {
                instance.inner.enumerate_device_extension_properties(physical_device)
            }?;

            let mut supports_presentation = false;
            for extension in extensions {
                let name = unsafe { CStr::from_ptr(extension.extension_name.as_ptr()) }.to_str()?;
                if name == "VK_KHR_swapchain" {
                    supports_presentation = true;
                    break;
                }
            }

            // Result.
            result.push(VulkanDeviceValue {
                name: match unsafe { CStr::from_ptr(properties.device_name.as_ptr()) }.to_str() {
                    Ok(name) => name,
                    Err(err) => return Err(err.into())
                }.into(),
                vendor: properties.vendor_id,
                device_type: unsafe { mem::transmute(properties.device_type) },
                api_version: properties.api_version,
                driver_version: properties.driver_version,
                guid: Uuid::from_bytes_le(id_properties.device_uuid),
                supports_graphics,
                supports_computing,
                supports_presentation,
                handle: Box::new(Arc::new(VulkanDevice::new(instance, physical_device)))
            });
        }

        Ok(result)
    }

    pub fn inner(&self) -> &ash::Instance {
        &self.inner
    }

    pub fn library(&self) -> &Arc<ash::Entry> {
        &self.library
    }
}

impl Drop for VulkanInstance {
    fn drop(&mut self) {
        unsafe {
            self.inner.destroy_instance(None);
        }

        log::info(format!("Dropped VulkanInstance {{ InnerHandle = {:p} }}.", self.inner.handle()).as_str());
    }
}

unsafe extern "system" fn log_callback(
    message_severity: vk::DebugUtilsMessageSeverityFlagsEXT,
    message_type: vk::DebugUtilsMessageTypeFlagsEXT,
    p_callback_data: *const vk::DebugUtilsMessengerCallbackDataEXT,
    _p_user_data: *mut c_void,
) -> vk::Bool32 {
    let level = match message_severity {
        vk::DebugUtilsMessageSeverityFlagsEXT::WARNING => LogLevel::Warning,
        vk::DebugUtilsMessageSeverityFlagsEXT::ERROR => LogLevel::Error,
        vk::DebugUtilsMessageSeverityFlagsEXT::INFO => LogLevel::Info,
        vk::DebugUtilsMessageSeverityFlagsEXT::VERBOSE => LogLevel::Debug,
        _ => {
            log::warning("Vulkan log severity was not handled.");
            LogLevel::Info
        }
    };

    let prefix = match message_type {
        vk::DebugUtilsMessageTypeFlagsEXT::GENERAL => "",
        vk::DebugUtilsMessageTypeFlagsEXT::VALIDATION => "Validation",
        vk::DebugUtilsMessageTypeFlagsEXT::PERFORMANCE => "Performance",
        _ => {
            log::warning("Vulkan log type was not handled.");
            "Unknown"
        }
    };

    let message = match CStr::from_ptr((*p_callback_data).p_message).to_str() {
        Ok(m) => m,
        Err(_) => {
            log::error("Vulkan log message throws Utf8Error.");
            return vk::FALSE;
        }
    };

    if prefix.is_empty() {
        logger::log(level, message);
    } else {
        logger::log(level, format!("{}: {}", prefix, message).as_str());
    }

    vk::FALSE
}
