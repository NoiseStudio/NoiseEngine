use enumflags2::{bitflags, BitFlags};

#[repr(C)]
pub struct WindowSettings {
    pub mode: WindowMode,
    pub controls: BitFlags<WindowControls>,
    pub position: WindowPosition,
    pub resizable: bool
}

#[repr(C)]
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
pub struct WindowPosition {
    pub x: WindowCoordinate,
    pub y: WindowCoordinate
}

#[repr(C)]
pub struct WindowCoordinate {
    pub value: i32,
    pub mode: WindowCoordinateMode
}

impl WindowCoordinate {
    pub fn compute<DF, CF>(&self, default_factory: DF, center_factory: CF) -> i32
    where
        DF: FnOnce() -> i32,
        CF: FnOnce() -> i32
    {
        match self.mode {
            WindowCoordinateMode::Default => default_factory(),
            WindowCoordinateMode::Value => self.value,
            WindowCoordinateMode::Center => center_factory()
        }
    }
}

#[repr(u8)]
pub enum WindowCoordinateMode {
    Default = 0,
    Value = 1,
    Center = 2
}
