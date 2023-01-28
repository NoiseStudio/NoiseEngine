
pub const CAPSLOCK_MODIFIER: u16 = 1 << 0;
pub const SHIFT_MODIFIER: u16 = 1 << 1;
pub const CONTROL_MODIFIER: u16 = 1 << 2;
pub const ALT_MODIFIER: u16 = 1 << 3;
pub const SUPER_MODIFIER: u16 = 1 << 4;
pub const LEFT_SHIFT_MODIFIER: u16 = 1 << 5;
pub const LEFT_CONTROL_MODIFIER: u16 = 1 << 6;
pub const LEFT_ALT_MODIFIER: u16 = 1 << 7;
pub const LEFT_SUPER_MODIFIER: u16 = 1 << 8;
pub const RIGHT_SHIFT_MODIFIER: u16 = 1 << 9;
pub const RIGHT_CONTROL_MODIFIER: u16 = 1 << 10;
pub const RIGHT_ALT_MODIFIER: u16 = 1 << 11;
pub const RIGHT_SUPER_MODIFIER: u16 = 1 << 12;

pub struct InputData {
    pub current_modifier: u16,
    pub key_values: &'static mut [KeyValue],
}

impl InputData {
    pub fn new(key_values: &'static mut [KeyValue]) -> Self {
        Self { current_modifier: 0, key_values }
    }
}

#[repr(C)]
#[derive(Debug, Copy, Clone)]
pub struct KeyValue {
    pub modifier: u16,
    pub state: KeyState
}

#[repr(u16)]
#[derive(Debug, Copy, Clone, PartialEq)]
pub enum KeyState {
    Released = 0,
    JustReleased = 1,
    JustPressed = 2,
    Pressed = 3
}
