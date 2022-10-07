use std::sync::Arc;

use vulkano::{VulkanLibrary, LoadingError};

pub(crate) fn create() -> Result<Arc<VulkanLibrary>, LoadingError> {
    VulkanLibrary::new()
}
