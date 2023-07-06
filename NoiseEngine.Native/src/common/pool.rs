use std::{
    mem::ManuallyDrop,
    ops::{Deref, DerefMut},
    ptr,
    sync::atomic::{AtomicUsize, Ordering},
};

use lockfree::stack::Stack;

pub struct Pool<T> {
    objects: Stack<T>,
    length: AtomicUsize,
}

impl<T> Pool<T> {
    pub fn try_get(&self) -> Option<PoolItem<T>> {
        self.objects.pop().map(|inner| {
            self.length.fetch_sub(1, Ordering::Relaxed);
            PoolItem {
                pool: self,
                inner: ManuallyDrop::new(inner),
            }
        })
    }

    pub fn wrap(&self, obj: T) -> PoolItem<T> {
        PoolItem {
            pool: self,
            inner: ManuallyDrop::new(obj),
        }
    }

    pub fn get_or_create<E, F>(&self, factory: F) -> Result<PoolItem<T>, E>
    where
        F: FnOnce() -> Result<T, E>,
    {
        match self.try_get() {
            Some(s) => Ok(s),
            None => {
                let obj = factory()?;
                Ok(self.wrap(obj))
            }
        }
    }

    pub fn get_or_create_where<E, W, F>(&self, predicate: W, factory: F) -> Result<PoolItem<T>, E>
    where
        W: Fn(&T) -> bool,
        F: FnOnce() -> Result<T, E>,
    {
        let length = self.length.load(Ordering::Relaxed);

        // TODO: Remove this Vec to not occupy the entire pool.
        let mut vec = Vec::with_capacity(length);

        for _ in 0..length {
            let obj = match self.try_get() {
                Some(s) => s,
                None => continue,
            };

            if predicate(&obj) {
                return Ok(obj);
            }

            vec.push(obj)
        }

        let obj = factory()?;
        Ok(self.wrap(obj))
    }
}

impl<T> Default for Pool<T> {
    fn default() -> Self {
        Self {
            objects: Stack::new(),
            length: AtomicUsize::new(0),
        }
    }
}

pub struct PoolItem<'a, T> {
    pool: &'a Pool<T>,
    inner: ManuallyDrop<T>,
}

impl<T> Drop for PoolItem<'_, T> {
    fn drop(&mut self) {
        let t = unsafe { ptr::read(&self.inner as &T) };

        self.pool.objects.push(t);
        self.pool.length.fetch_add(1, Ordering::Relaxed);
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
