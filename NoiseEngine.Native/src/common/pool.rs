use std::{mem::ManuallyDrop, ptr, ops::{Deref, DerefMut}};

use lockfree::stack::Stack;

pub struct Pool<T> {
    objects: Stack<T>
}

impl<T> Pool<T> {
    pub fn try_get(&self) -> Option<PoolItem<T>> {
        self.objects.pop().map(|inner| PoolItem {
            pool: self,
            inner: ManuallyDrop::new(inner)
        })
    }

    pub fn wrap(&self, obj: T) -> PoolItem<T> {
        PoolItem {
            pool: self,
            inner: ManuallyDrop::new(obj)
        }
    }

    pub fn get_or_create<E, F>(&self, factory: F) -> Result<PoolItem<T>, E>
    where
        F: Fn() -> Result<T, E>
    {
        match self.try_get() {
            Some(s) => Ok(s),
            None => {
                let obj = factory()?;
                Ok(self.wrap(obj))
            }
        }
    }
}

impl<T> Default for Pool<T> {
    fn default() -> Self {
        Self { objects: Stack::new() }
    }
}

pub struct PoolItem<'a, T> {
    pool: &'a Pool<T>,
    inner: ManuallyDrop<T>
}

impl<T> Drop for PoolItem<'_, T> {
    fn drop(&mut self) {
        let t = unsafe {
            ptr::read(&self.inner as &T)
        };

        self.pool.objects.push(t);
    }
}

impl<T> Deref for PoolItem<'_, T> {
    type Target = T;

    fn deref(&self) -> &Self::Target {
        &self.inner
    }
}

impl<T> DerefMut for PoolItem<'_, T> {
    fn deref_mut(&mut self) -> &mut Self::Target {
        &mut self.inner
    }
}
