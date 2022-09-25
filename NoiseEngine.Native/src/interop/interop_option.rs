use std::mem::MaybeUninit;

#[repr(C)]
pub struct InteropOption<T> {
    has_value: bool,
    value: MaybeUninit<T>
}

impl<T> From<Option<T>> for InteropOption<T> {
    fn from(option: Option<T>) -> Self {
        match option {
            Some(value) => InteropOption { has_value: true, value: MaybeUninit::new(value) },
            None => InteropOption { has_value: false, value: MaybeUninit::uninit() }
        }
    }
}

impl<T> From<InteropOption<T>> for Option<T> {
    fn from(option: InteropOption<T>) -> Self {
        if !option.has_value {
            return None
        }

        let value = unsafe {
            option.value.assume_init_read()
        };

        Some(value)
    }
}