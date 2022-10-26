use std::{ptr, mem, ffi::{CStr, CString}};

use ash::vk;
use libc::c_void;
use uuid::Uuid;

use crate::{
    interop::rendering::vulkan::{application_info::VulkanApplicationInfo, device_value::VulkanDeviceValue},
    logging::{logger, log_level::LogLevel, log}, errors::{overflow::OverflowError, null_reference::NullReferenceError}
};

use super::{device::VulkanDevice, errors::universal::VulkanUniversalError};

pub(crate) fn create(
    library: &ash::Entry, application_info: VulkanApplicationInfo,
    log_severity: vk::DebugUtilsMessageSeverityFlagsEXT, log_type: vk::DebugUtilsMessageTypeFlagsEXT
) -> Result<ash::Instance, VulkanUniversalError> {
    let create_info;

    if log_severity.is_empty() || log_type.is_empty() {
        create_info = vk::InstanceCreateInfo {
            s_type: vk::StructureType::INSTANCE_CREATE_INFO,
            p_next: ptr::null(),
            flags: vk::InstanceCreateFlags::empty(),
            p_application_info: &application_info.into(),
            enabled_layer_count: 0,
            pp_enabled_layer_names: ptr::null(),
            enabled_extension_count: 0,
            pp_enabled_extension_names: ptr::null(),
        };

        return match unsafe { library.create_instance(&create_info, None) } {
            Ok(instance) => Ok(instance),
            Err(err) => Err(err.into())
        }
    }

    let mut enabled_extensions: Vec<*const i8> = Vec::new();
    enabled_extensions.push(ash::extensions::ext::DebugUtils::name().as_ptr());

    let mut enabled_layers: Vec<*const i8> = Vec::new();

    let validation_layer = match CString::new("VK_LAYER_KHRONOS_validation") {
        Ok(str) => str,
        Err(_) => return Err(NullReferenceError::default().into())
    };
    enabled_layers.push(validation_layer.as_ptr());

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
        p_application_info: &application_info.into(),
        enabled_layer_count: enabled_layers.len() as u32,
        pp_enabled_layer_names: enabled_layers.as_ptr(),
        enabled_extension_count: enabled_extensions.len() as u32,
        pp_enabled_extension_names: enabled_extensions.as_ptr(),
    };

    match unsafe { library.create_instance(&create_info, None) } {
        Ok(instance) => Ok(instance),
        Err(err) => Err(err.into())
    }
}

pub(crate) fn get_vulkan_physical_devices(
    instance: &ash::Instance
) -> Result<Vec<VulkanDeviceValue>, VulkanUniversalError> {
    match unsafe { instance.enumerate_physical_devices() } {
        Ok(physical_devices) => {
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
                    instance.get_physical_device_properties2(physical_device, &mut properties2)
                };

                let properties = properties2.properties;

                // Queue families.
                let queue_family_properties = unsafe {
                    instance.get_physical_device_queue_family_properties(physical_device)
                };

                let mut supports_graphics = false;
                let mut supports_computing = false;
                for queue_family_properties in queue_family_properties {
                    supports_graphics |= queue_family_properties.queue_flags.contains(vk::QueueFlags::GRAPHICS);
                    supports_computing |= queue_family_properties.queue_flags.contains(vk::QueueFlags::COMPUTE);
                }

                // Result.
                result.push(VulkanDeviceValue {
                    name: match unsafe { CStr::from_ptr(properties.device_name.as_ptr()) }.to_str() {
                        Ok(name) => name,
                        Err(err) => return Err(err.into())
                    }.into(),
                    vendor: properties.vendor_id,
                    device_type: unsafe { mem::transmute(properties.device_type) },
                    api_version: match u32::try_from(properties.api_version) {
                        Ok(v) => v,
                        Err(_) => return Err(OverflowError::with_str(
                            "Vulkan API version overflowed on physical device."
                        ).into())
                    },
                    driver_version: properties.driver_version,
                    guid: Uuid::from_bytes_le(id_properties.device_uuid),
                    supports_graphics,
                    supports_computing,
                    handle: Box::new(VulkanDevice::new(instance, physical_device))
                });
            }

            Ok(result.into())
        },
        Err(err) => Err(err.into())
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
        vk::DebugUtilsMessageTypeFlagsEXT::GENERAL => "General",
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

    logger::log(level, format!("{}: {}", prefix, message).as_str());

    return vk::FALSE;
}
