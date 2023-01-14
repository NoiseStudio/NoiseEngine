use enumflags2::{bitflags, BitFlags};

#[repr(C)]
#[derive(Copy, Clone)]
pub struct WindowSettings {
    pub mode: WindowMode,
    pub controls: BitFlags<WindowControls>,
    pub position: WindowPosition,
    pub resizable: bool
}

#[repr(C)]
#[derive(Copy, Clone)]
pub enum WindowMode {
    Windowed = 0,
    Borderless = 1
}

#[repr(u32)]
#[bitflags]
#[derive(Copy, Clone, PartialEq)]
pub enum WindowControls {
    MinimizeButton = 1 << 0,
    MaximizeButton = 1 << 1
}

#[repr(C)]
#[derive(Copy, Clone)]
pub struct WindowPosition {
    pub x: WindowCoordinate,
    pub y: WindowCoordinate
}

#[repr(C)]
#[derive(Copy, Clone)]
pub struct WindowCoordinate {
    pub value: i32,
    pub mode: WindowCoordinateMode
}

#[repr(u8)]
#[derive(Copy, Clone)]
pub enum WindowCoordinateMode {
    Default = 0,
    Value = 1,
    Center = 2
}
