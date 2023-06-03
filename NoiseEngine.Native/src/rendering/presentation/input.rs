use cgmath::Vector2;

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

#[repr(C)]
pub struct InputData {
    pub key_values: [KeyValue; 133],
    pub cursor_position: Vector2<f64>,
    pub scroll_delta: Vector2<f64>,
}

#[repr(C)]
#[derive(Debug, Copy, Clone)]
pub struct KeyValue {
    pub modifier: u16,
    pub state: KeyState,
}

#[repr(u16)]
#[derive(Debug, Copy, Clone, PartialEq)]
pub enum KeyState {
    Released = 0,
    JustReleased = 1,
    JustPressed = 2,
    Pressed = 3,
}
