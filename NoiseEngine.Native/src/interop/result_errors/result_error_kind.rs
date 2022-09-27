use std::{collections::HashMap, any::TypeId, error::Error, str::ParseBoolError};

lazy_static! {
    static ref MAP: HashMap<TypeId, ResultErrorKind> = {
        let mut map = HashMap::new();

        map.insert(TypeId::of::<ParseBoolError>(), ResultErrorKind::Format);

        map
    };
}

#[repr(C)]
#[derive(Clone)]
pub enum ResultErrorKind {
    Universal = 0,
    Format = 1
}

impl ResultErrorKind {
    pub fn from_err<T: Error + 'static>() -> ResultErrorKind {
        match MAP.get(&TypeId::of::<T>()) {
            Some(s) => *s,
            None => ResultErrorKind::Universal
        }
    }
}

impl Copy for ResultErrorKind {
}