#[repr(C)]
pub enum CameraClearFlags {
    Undefined = 0,
    Nothing = 1,
    SolidColor = 2
}

#[repr(C)]
pub struct CameraClear {
    pub clear_flags: CameraClearFlags,
    pub background_color_x: f32,
    pub background_color_y: f32,
    pub background_color_z: f32
}
