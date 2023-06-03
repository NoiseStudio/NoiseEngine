use std::cmp::Ordering;

#[repr(C)]
pub struct VulkanDeviceSupport {
    pub graphics: bool,
    pub computing: bool,
    pub transfer: bool,
}

impl VulkanDeviceSupport {
    pub fn is_suitable_to(&self, main: &Self) -> bool {
        (!self.graphics || main.graphics)
            && (!self.computing || main.computing)
            && (!self.transfer || main.transfer)
    }

    pub(crate) fn family_cmp(&self, other: &Self) -> Ordering {
        match self.family_cmp_count().cmp(&other.family_cmp_count()) {
            Ordering::Less => Ordering::Less,
            Ordering::Greater => Ordering::Greater,
            Ordering::Equal => self.family_cmp_binary().cmp(&other.family_cmp_binary()),
        }
    }

    fn family_cmp_count(&self) -> u32 {
        self.graphics as u32 + self.computing as u32 + self.transfer as u32
    }

    fn family_cmp_binary(&self) -> u32 {
        ((self.graphics as u32) << 2) + ((self.computing as u32) << 1) + (self.transfer as u32)
    }
}
