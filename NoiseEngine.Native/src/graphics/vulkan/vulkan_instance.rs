use std::sync::Arc;

use enumflags2::BitFlags;
use vulkano::{
    instance::{
        Instance, InstanceCreationError,
        debug::{DebugUtilsMessageSeverity, DebugUtilsMessageType, DebugUtilsMessengerCreateInfo, Message},
        InstanceCreateInfo
    },
    VulkanLibrary
};

use crate::{
    interop::graphics::vulkan::vulkan_instance_create_info::VulkanInstanceCreateInfo,
    logging::{logger, log_level::LogLevel, log}
};

use super::{vulkan_log_severity::VulkanLogSeverity, vulkan_log_type::VulkanLogType};

pub(crate) fn create(
    library: Arc<VulkanLibrary>, create_info: VulkanInstanceCreateInfo,
    log_severity: VulkanLogSeverity, log_type: VulkanLogType
) -> Result<Arc<Instance>, InstanceCreationError> {
    let create_info_final = InstanceCreateInfo {
        enabled_extensions: *library.supported_extensions(),
        ..create_info.into()
    };

    let severity_flags = BitFlags::from_flag(log_severity);
    let type_flags = BitFlags::from_flag(log_type);

    if severity_flags.is_empty() || type_flags.is_empty() {
        return Instance::new(library, create_info_final)
    }

    let severity_final = DebugUtilsMessageSeverity {
        verbose: severity_flags.contains(VulkanLogSeverity::Verbose),
        information: severity_flags.contains(VulkanLogSeverity::Info),
        warning: severity_flags.contains(VulkanLogSeverity::Warning),
        error: severity_flags.contains(VulkanLogSeverity::Error),
        ..Default::default()
    };

    let type_final = DebugUtilsMessageType {
        general: type_flags.contains(VulkanLogType::General),
        validation: type_flags.contains(VulkanLogType::Validation),
        performance: type_flags.contains(VulkanLogType::Performance),
        ..Default::default()
    };

    unsafe {
        Instance::with_debug_utils_messengers(library, create_info_final, [
            DebugUtilsMessengerCreateInfo {
                message_severity: severity_final,
                message_type: type_final,
                ..DebugUtilsMessengerCreateInfo::user_callback(Arc::new(log_callback))
            }
        ])
    }
}

fn log_callback(msg: &Message) {
    let level;
    if msg.severity.warning {
        level = LogLevel::Warning;
    } else if msg.severity.error {
        level = LogLevel::Error;
    } else if msg.severity.information {
        level = LogLevel::Info;
    } else if msg.severity.verbose {
        level = LogLevel::Debug;
    } else {
        level = LogLevel::Info;
        log::warning("Vulkan log severity was not handled.");
    }

    match msg.layer_prefix {
        Some(prefix) => {
            logger::log_owned(level, prefix.to_owned() + ": " + msg.description)
        },
        None => logger::log(level, msg.description)
    }
}
