use enumflags2::bitflags;

#[repr(u32)]
#[bitflags]
#[derive(Copy, Clone, PartialEq)]
pub(crate) enum VulkanLogType {
    General = 0x00000001,
    Validation = 0x00000002,
    Performance = 0x00000004,
    DeviceAddressBinding = 0x00000008
}
