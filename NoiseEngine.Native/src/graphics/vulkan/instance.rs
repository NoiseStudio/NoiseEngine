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
    interop::graphics::vulkan::instance_create_info::VulkanInstanceCreateInfo,
    logging::{logger, log_level::LogLevel, log}
};

use super::{log_severity::VulkanLogSeverity, log_type::VulkanLogType};

pub(crate) fn create(
    library: Arc<VulkanLibrary>, create_info: VulkanInstanceCreateInfo,
    log_severity: BitFlags<VulkanLogSeverity>, log_type: BitFlags<VulkanLogType>
) -> Result<Arc<Instance>, InstanceCreationError> {
    let create_info_final = InstanceCreateInfo {
        enabled_extensions: *library.supported_extensions(),
        ..create_info.into()
    };
    
    if log_severity.is_empty() || log_type.is_empty() {
        return Instance::new(library, create_info_final)
    }

    let severity_final = DebugUtilsMessageSeverity {
        verbose: log_severity.contains(VulkanLogSeverity::Verbose),
        information: log_severity.contains(VulkanLogSeverity::Info),
        warning: log_severity.contains(VulkanLogSeverity::Warning),
        error: log_severity.contains(VulkanLogSeverity::Error),
        ..Default::default()
    };

    let type_final = DebugUtilsMessageType {
        general: log_type.contains(VulkanLogType::General),
        validation: log_type.contains(VulkanLogType::Validation),
        performance: log_type.contains(VulkanLogType::Performance),
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
            logger::log(level, format!("{}: {}", prefix, msg.description).as_str())
        },
        None => logger::log(level, msg.description)
    }
}
