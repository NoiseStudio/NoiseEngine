use enumflags2::bitflags;

#[repr(u32)]
#[bitflags]
#[derive(Copy, Clone, PartialEq)]
pub(crate) enum VulkanLogSeverity {
    Verbose = 0x00000001,
    Info = 0x00000010,
    Warning = 0x00000100,
    Error = 0x00001000
}
