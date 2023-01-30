use std::{
    ptr, sync::{Arc, Weak, atomic::{AtomicBool, Ordering}}, cell::{UnsafeCell, Cell},
    thread::{self, JoinHandle, ThreadId}, mem::{self, ManuallyDrop}
};

use ash::{vk, extensions::khr};
use cgmath::{Vector2, Zero};
use crossbeam_queue::SegQueue;
use libc::{c_void, wchar_t};
use rsevents::{ManualResetEvent, EventState, Awaitable, AutoResetEvent};
use uuid::Uuid;

use crate::{
    errors::{
        platform::{windows::win32::Win32Error, platform_universal::PlatformUniversalError},
        null_reference::NullReferenceError, invalid_operation::InvalidOperationError
    },
    logging::log,
    rendering::{
        presentation::{
            window::Window, window_settings::{WindowSettings, WindowMode, WindowControls, WindowCoordinateMode},
            window_event_handler::WindowEventHandler, input::{InputData, self, KeyValue, KeyState}
        },
        vulkan::{surface::VulkanSurface, instance::VulkanInstance, errors::universal::VulkanUniversalError}
    }, platform::windows::rect::Rect, common::send_ptr::{SendPtrMut, SendPtr}
};

use super::{wnd_class_w::WndClassW, msg::Msg};

const PROP_NAME: &[u16] = const_utf16::encode!("NEwp\0");

fn wide_null(s: &str) -> Vec<u16> {
    s.encode_utf16().chain(Some(0)).collect()
}

#[inline]
fn low_word(l: isize) -> u16 {
    (l & 0xffff) as u16
}

#[inline]
fn low_word_i(l: isize) -> i16 {
    (l & 0xffff) as i16
}

#[inline]
fn high_word(l: isize) -> u16 {
    ((l >> 16) & 0xffff) as u16
}

#[inline]
fn high_word_i(l: isize) -> i16 {
    ((l >> 16) & 0xffff) as i16
}

#[inline]
fn is_extended_key(l_param: isize) -> bool {
    (high_word(l_param) & 0x0100) == 0x0100
}

#[inline]
fn is_right_shift(l_param: isize) -> bool {
    match unsafe {
        MapVirtualKeyW(((l_param & 0x00ff0000) >> 16) as u32, 3)
    } {
        0xa0 => false,
        0xa1 => true,
        _ => panic!("Given key is not a shift.")
    }
}

pub struct WindowWindows {
    id: u64,
    weak: Weak<Self>,
    h_wnd: *mut c_void,
    class_name: Vec<u16>,
    h_instance: *mut c_void,
    thread_id: ThreadId,
    thread_work: AtomicBool,
    thread_join_handle: Cell<Option<JoinHandle<()>>>,
    thread_task_queue: SegQueue<(WindowWindowsThreadTask, Option<Arc<AutoResetEvent>>, *mut c_void)>,
    thread_reset_event: AutoResetEvent,
    thread_end_reset_event: Arc<AutoResetEvent>,
    thread_last_signaler: Cell<Option<Arc<AutoResetEvent>>>,
    data: UnsafeCell<WindowWindowsData>
}

impl WindowWindows {
    pub fn new(
        id: u64, title: String, width: u32, height: u32, settings: WindowSettings
    ) -> Result<Arc<dyn Window>, PlatformUniversalError> {
        let mut result = unsafe { mem::zeroed() };
        let reset_event = ManualResetEvent::new(EventState::Unset);

        let result_ptr = SendPtrMut(&mut result);
        let reset_event_ptr = SendPtr(&reset_event);
        let join_handle = match thread::Builder::new().name(
            format!("Window {{ Id = {id} }}")
        ).spawn(move || {
            let mut window = {
                let result =
                    Self::new_worker(id, title, width, height, settings);

                let _ = &result_ptr;
                unsafe {
                    ptr::copy(&result, result_ptr.0, 1)
                };

                let _ = &reset_event_ptr;
                unsafe {
                    reset_event_ptr.0.as_ref()
                }.unwrap().set();

                match result {
                    Ok(w) => {
                        mem::forget(w.clone());
                        ManuallyDrop::new(w)
                    },
                    Err(_) => {
                        mem::forget(result);
                        return
                    },
                }
            };

            window.thread_worker();

            let end_reset_event = window.thread_end_reset_event.clone();
            unsafe {
                ManuallyDrop::drop(&mut window);
            }
            end_reset_event.wait();
        }) {
            Ok(j) => j,
            Err(_) => return Err(InvalidOperationError::with_str("Unable to create window thread.").into()),
        };

        reset_event.wait();
        match result {
            Ok(window) => {
                window.thread_join_handle.set(Some(join_handle));
                Ok(window)
            },
            Err(err) => Err(err),
        }
    }

    fn new_worker(
        id: u64, title: String, width: u32, height: u32, settings: WindowSettings
    ) -> Result<Arc<Self>, PlatformUniversalError> {
        let h_instance = unsafe {
            GetModuleHandleW(ptr::null_mut())
        };

        // Create window class.
        let class_name = wide_null(Uuid::new_v4().to_string().as_str());

        let window_class = WndClassW {
            lpfn_wnd_proc: Some(window_procedure),
            h_instance,
            lpsz_class_name: class_name.as_ptr(),
            ..Default::default()
        };

        if unsafe { RegisterClassW(&window_class) } == 0 {
            return Err(Win32Error::get_last().into())
        }

        // Construct.
        #[allow(deref_nullptr)]
        let arc = Arc::new(Self {
            id,
            weak: Weak::default(),
            h_wnd: ptr::null_mut(),
            class_name,
            h_instance,
            thread_id: thread::current().id(),
            thread_work: AtomicBool::new(true),
            thread_join_handle: Cell::new(None),
            thread_task_queue: SegQueue::new(),
            thread_reset_event: AutoResetEvent::new(EventState::Unset),
            thread_end_reset_event: Arc::new(AutoResetEvent::new(EventState::Unset)),
            thread_last_signaler: Cell::new(None),
            data: UnsafeCell::new(WindowWindowsData {
                width, height, settings, input_current_modifier: 0, input_data: unsafe { &mut *ptr::null_mut() }
            }),
        });

        let reference = unsafe {
            &mut *(Arc::as_ptr(&arc) as *mut WindowWindows)
        };
        reference.weak = Arc::downgrade(&arc);

        // Create window.
        let adjust = reference.get_adjust(Rect {
            left: 0,
            top: 0,
            right: reference.get_width() as i32,
            bottom: reference.get_height() as i32,
        })?;
        let (position_x, position_y) = reference.get_winapi_position(&adjust);
        let wide_title = wide_null(title.as_str()).as_ptr();

        reference.h_wnd = unsafe {
            CreateWindowExW(
                0,
                reference.class_name.as_ptr(),
                wide_title,
                reference.get_window_style(),
                position_x,
                position_y,
                adjust.right - adjust.left,
                adjust.bottom - adjust.top,
                ptr::null_mut(),
                ptr::null_mut(),
                h_instance,
                ptr::null_mut()
            )
        };

        let h_data = reference as *const WindowWindows as *const c_void;
        if unsafe { SetPropW(reference.h_wnd, PROP_NAME.as_ptr(), h_data) } == 0 {
            return Err(Win32Error::get_last().into())
        }

        _ = unsafe {
            ShowWindow(reference.h_wnd, 1)
        };

        Ok(arc)
    }

    fn translate_keys(w_param: usize, l_param: isize) -> usize {
        match w_param {
            0x30..=0x39 => w_param - 47, // Alpha 0 - 9
            0x41..=0x5a => w_param - 54, // A - Z
            0x20 => 37, // Space ( )
            0xbd => 38, // Minus (-)
            0xbc => 39, // Comma (,)
            0xbe..=0xbf => w_param - 150, // Period (.), Slash (/)
            0xdc => 42, // BackSlash (\)
            0xba..=0xbb => w_param - 143, // Semicolon (;), Equal (=)
            0xde => 45, // Appostrophe (')
            0xdb => 46, // LeftBracket ([)
            0xdd => 47, // RightBracket (])
            0x1b => 48, // Escape
            0xc0 => 49, // Grave (`),
            0x70..=0x87 => w_param - 62, // F1 - F24
            0x6a..=0x6f => w_param - 32, // NumpadMultiply - NumpadDivide
            0xd => match is_extended_key(l_param) {
                true => 80, // NumpadEnter
                false => 103, // Return
            },
            0x21..=0x28 => w_param + match is_extended_key(l_param) { // NumpadKeys
                true => 71, // Normal
                false => 48, // Numpad
            },
            0x90 => 89, // NumpadLock
            0x60..=0x69 => w_param - 6, // Numpad 0 - 9
            0x8..=0x9 => w_param + 92, // Backspace, Tab
            0xc => 102, // Clear
            0x2c => 112, // PrintScreen
            0x91 => 113, // ScrollLock
            0x13 => 114, // Pause
            0x2d..=0x2f => w_param + 70, // Insert, Delete, Help
            0x5d => 118,
            _ => 0
        }
    }

    fn keydown_worker(data: &mut WindowWindowsData, last_modifier: u16, key_index: usize) {
        let input = &mut data.input_data;

        input.key_values[0] = KeyValue { modifier: last_modifier, state: KeyState::JustPressed };

        let value = input.key_values[key_index];
        if value.state == KeyState::Released {
            input.key_values[key_index] = KeyValue { modifier: last_modifier, state: KeyState::JustPressed };
        }

        if data.input_current_modifier == last_modifier {
            data.input_current_modifier = 0;
        }
    }

    fn keyup_worker(data: &mut WindowWindowsData, key_index: usize) {
        let input = &mut data.input_data;

        let mut value = input.key_values[0];
        if value.state == KeyState::Pressed {
            input.key_values[0] = KeyValue { modifier: value.modifier, state: KeyState::JustReleased };
        }

        value = input.key_values[key_index];
        if value.state == KeyState::Pressed {
            input.key_values[key_index] = KeyValue { modifier: value.modifier, state: KeyState::JustReleased };
        }
    }

    // https://learn.microsoft.com/en-us/windows/win32/winmsg/window-styles
    fn get_window_style(&self) -> u32 {
        let settings = self.data().settings;

        let mut result = match settings.mode {
            WindowMode::Windowed => {
                let mut a = 0x00C00000 | 0x00080000; // WS_CAPTION | WS_SYSMENU

                if settings.controls.contains(WindowControls::MinimizeButton) {
                    a |= 0x00020000; // WS_MINIMIZEBOX
                }
                if settings.controls.contains(WindowControls::MaximizeButton) {
                    a |= 0x00010000; // WS_MAXIMIZEBOX
                }

                a
            },
            WindowMode::Borderless => 0x80000000, // WS_POPUP
        };

        if settings.resizable {
            result |= 0x00040000; // WS_THICKFRAME
        }

        result
    }

    fn get_adjust(&self, rect: Rect) -> Result<Rect, Win32Error> {
        let result = unsafe {
            AdjustWindowRectEx(&rect, self.get_window_style(), 0, 0)
        };

        if result == 0 {
            return Err(Win32Error::get_last())
        }

        Ok(rect)
    }

    fn get_winapi_position(&self, adjust: &Rect) -> (i32, i32) {
        const CW_USEDEFAULT: u32 = 0x80000000;

        let settings = self.data().settings;

        let x = match settings.position.x.mode {
            WindowCoordinateMode::Default => CW_USEDEFAULT as i32,
            WindowCoordinateMode::Value => settings.position.x.value + adjust.left,
            WindowCoordinateMode::Center => unimplemented!(),
        };

        let y = match settings.position.y.mode {
            WindowCoordinateMode::Default => CW_USEDEFAULT as i32,
            WindowCoordinateMode::Value => settings.position.y.value + adjust.top,
            WindowCoordinateMode::Center => unimplemented!(),
        };

        (x, y)
    }

    fn data(&self) -> &WindowWindowsData {
        unsafe { &*self.data.get() }
    }

    fn data_mut(&self) -> &mut WindowWindowsData {
        unsafe { &mut *self.data.get() }
    }

    fn execute_task(&self, task: WindowWindowsThreadTask) {
        self.thread_task_queue.push((task, None, ptr::null_mut()));
        self.thread_reset_event.set();
    }

    fn execute_task_wait(&self, task: WindowWindowsThreadTask) {
        let mut i = 0;
        self.execute_task_wait_with_data(task, &mut i);
    }

    fn execute_task_wait_with_data<T>(&self, task: WindowWindowsThreadTask, data: &mut T) {
        if self.thread_id == thread::current().id() {
            self.thread_worker_task_invoker(task, None, data as *mut T as *mut c_void);
            return;
        }

        if !self.thread_work.load(Ordering::Relaxed) {
            return;
        }

        let reset_event = Arc::new(AutoResetEvent::new(EventState::Unset));
        self.thread_task_queue.push((task, Some(reset_event.clone()), data as *mut T as *mut c_void));
        self.thread_reset_event.set();

        if self.thread_work.load(Ordering::Relaxed) {
            reset_event.wait();
        }
    }

    fn thread_worker(&self) {
        while self.thread_work.load(Ordering::Relaxed) {
            self.thread_reset_event.wait();
            self.thread_worker_iterator();
        }
    }

    fn thread_worker_iterator(&self) {
        while let Some((
            task, signal, data
        )) = self.thread_task_queue.pop() {
            self.thread_worker_task_invoker(task, signal, data);
        }
    }

    fn thread_worker_task_invoker(
        &self, task: WindowWindowsThreadTask, signal: Option<Arc<AutoResetEvent>>, data: *mut c_void
    ) {
        self.thread_last_signaler.set(signal.clone());

        match task {
            WindowWindowsThreadTask::PoolEvents => self.pool_events_thread(),
            WindowWindowsThreadTask::Hide => self.hide_thread(),
            WindowWindowsThreadTask::IsFocused => self.is_focused_thread(data),
            WindowWindowsThreadTask::Dispose => self.dispose_thread(),
        };

        match signal {
            Some(s) => s.set(),
            None => (),
        };
    }

    fn pool_events_thread(&self) {
        let mut msg = Msg::default();
        loop {
            let result = unsafe {
                PeekMessageW(&mut msg, self.h_wnd, 0, 0, 1)
            };

            if result == 0 {
                return;
            }

            unsafe {
                TranslateMessage(&msg);
                DispatchMessageW(&msg);
            }
        }
    }

    fn hide_thread(&self) {
        unsafe {
            ShowWindow(self.h_wnd, 0)
        };
    }

    fn is_focused_thread(&self, data: *mut c_void) {
        unsafe {
            ptr::write(data as *mut bool, GetActiveWindow() == self.h_wnd);
        }
    }

    fn dispose_thread(&self) {
        self.thread_work.store(false, Ordering::Relaxed);
        self.hide_thread();

        while let Some((_, signal, _)) = self.thread_task_queue.pop() {
            match signal {
                Some(s) => s.set(),
                None => (),
            };
        }
    }
}

impl Drop for WindowWindows {
    fn drop(&mut self) {
        self.thread_end_reset_event.set();
        match self.thread_join_handle.take() {
            Some(j) => match j.join() {
                Ok(_) => (),
                Err(_) => log::error("Window thread has panicked."),
            },
            None => (),
        }

        if unsafe { DestroyWindow(self.h_wnd) } != 0 {
            log::error(Win32Error::get_last().to_string().as_str());
        }

        if unsafe { UnregisterClassW(self.class_name.as_ptr(), self.h_instance) } != 0 {
            log::error(Win32Error::get_last().to_string().as_str());
        }

        if unsafe { FreeLibrary(self.h_instance) } != 0 {
            log::error(Win32Error::get_last().to_string().as_str());
        }
    }
}

impl Window for WindowWindows {
    fn get_width(&self) -> u32 {
        self.data().width
    }

    fn get_height(&self) -> u32 {
        self.data().height
    }

    fn pool_events(&self, input_data: &'static mut InputData) {
        input_data.scroll_delta = Vector2::zero();

        for i in 0..input_data.key_values.len() {
            let value = input_data.key_values[i];
            match value.state {
                KeyState::JustReleased => input_data.key_values[i] = KeyValue {
                    modifier: 0, state: KeyState::Released
                },
                KeyState::JustPressed => input_data.key_values[i] = KeyValue {
                    modifier: value.modifier, state: KeyState::Pressed
                },
                _ => ()
            }
        }

        self.data_mut().input_data = input_data;
        self.execute_task_wait(WindowWindowsThreadTask::PoolEvents);
    }

    fn hide(&self) {
        self.execute_task(WindowWindowsThreadTask::Hide);
    }

    fn set_position(
        &self, position: Option<Vector2<i32>>, size: Option<Vector2<u32>>
    ) -> Result<(), PlatformUniversalError> {
        let mut flags = 0x0004 | 0x4000; // SWP_NOZORDER | SWP_ASYNCWINDOWPOS

        let x;
        let y;
        match position {
            Some(p) => {
                x = p.x;
                y = p.y;
            },
            None => {
                flags |= 0x0002; // SWP_NOMOVE
                x = 0;
                y = 0;
            },
        }

        let cx;
        let cy;
        match size {
            Some(s) => {
                let adjust = self.get_adjust(Rect {
                    left: 0,
                    top: 0,
                    right: s.x as i32,
                    bottom: s.y as i32,
                })?;

                cx = adjust.right - adjust.left;
                cy = adjust.bottom - adjust.top;
            },
            None => {
                flags |= 0x0001; // SWP_NOSIZE
                cx = 0;
                cy = 0;
            },
        }

        if unsafe { SetWindowPos(self.h_wnd, ptr::null_mut(), x, y, cx, cy, flags) } == 0 {
            Err(Win32Error::get_last().into())
        } else {
            Ok(())
        }
    }

    fn set_cursor_position(&self, position: Vector2<f64>) -> Result<(), PlatformUniversalError> {
        let mut position_i = Vector2::new(position.x as i32, position.y as i32);

        if unsafe { ClientToScreen(self.h_wnd, &mut position_i) } == 0 {
            return Err(Win32Error::get_last().into())
        }

        if unsafe { SetCursorPos(position_i.x, position_i.y) } == 0 {
            Err(Win32Error::get_last().into())
        } else {
            Ok(())
        }
    }

    fn is_focused(&self) -> bool {
        let mut result = false;
        self.execute_task_wait_with_data(WindowWindowsThreadTask::IsFocused, &mut result);
        result
    }

    fn create_vulkan_surface(&self, instance: &Arc<VulkanInstance>) -> Result<VulkanSurface, VulkanUniversalError> {
        let create_info = vk::Win32SurfaceCreateInfoKHR {
            s_type: vk::StructureType::WIN32_SURFACE_CREATE_INFO_KHR,
            p_next: ptr::null(),
            flags: vk::Win32SurfaceCreateFlagsKHR::empty(),
            hinstance: self.h_instance,
            hwnd: self.h_wnd,
        };

        let creator = khr::Win32Surface::new(instance.library(), instance.inner());
        let inner = unsafe {
            creator.create_win32_surface(&create_info, None)
        }?;

        let window_arc = match Weak::upgrade(&self.weak) {
            Some(a) => a,
            None => return Err(NullReferenceError::with_str("WindowWindows weak is null.").into()),
        };

        Ok(VulkanSurface::new(
            instance.clone(), window_arc, inner,
            khr::Surface::new(instance.library(), instance.inner())
        ))
    }

    fn dispose(&self) -> Result<(), PlatformUniversalError> {
        if self.thread_id == thread::current().id() {
            self.dispose_thread();
            return Ok(());
        }

        self.execute_task_wait(WindowWindowsThreadTask::Dispose);
        Ok(())
    }
}

unsafe extern "system" fn window_procedure(
    h_wnd: *mut c_void, msg: u32, w_param: usize, l_param: isize
) -> isize {
    let event_handler = WindowEventHandler::get();
    let window = &*(GetPropW(h_wnd, PROP_NAME.as_ptr()) as *const WindowWindows);
    let data = window.data_mut();

    match msg {
        // WM_SIZE
        0x0005 => {
            let width = low_word(l_param) as u32;
            let height = high_word(l_param) as u32;

            if window.get_width() != width || window.get_height() != height {
                data.width = width;
                data.height = height;
                (event_handler.size_changed)(window.id, width, height);
            }
        },
        // WM_CLOSE
        0x0010 => {
            match window.thread_last_signaler.take() {
                Some(s) => s.set(),
                None => (),
            };

            (event_handler.user_closed)(window.id);
        },
        // WM_SETFOCUS
        0x0007 => (event_handler.focused)(window.id),
        // WM_KILLFOCUS
        0x0008 => (event_handler.unfocused)(window.id),
        // WM_KEYDOWN
        0x0100 => {
            let last_modifier = data.input_current_modifier;
            let key_index = match w_param {
                0x14 => {
                    data.input_current_modifier |= input::CAPSLOCK_MODIFIER;
                    119
                },
                0x10 => match is_right_shift(l_param) {
                    true => {
                        data.input_current_modifier |= input::SHIFT_MODIFIER | input::RIGHT_SHIFT_MODIFIER;
                        124
                    },
                    false => {
                        data.input_current_modifier |= input::SHIFT_MODIFIER | input::LEFT_SHIFT_MODIFIER;
                        120
                    }
                },
                0x11 => match is_extended_key(l_param) {
                    true => {
                        data.input_current_modifier |= input::CONTROL_MODIFIER | input::RIGHT_CONTROL_MODIFIER;
                        125
                    },
                    false => {
                        data.input_current_modifier |= input::CONTROL_MODIFIER | input::LEFT_CONTROL_MODIFIER;
                        121
                    }
                },
                0x12 => match is_extended_key(l_param) {
                    true => {
                        data.input_current_modifier |= input::ALT_MODIFIER | input::RIGHT_ALT_MODIFIER;
                        126
                    },
                    false => {
                        data.input_current_modifier |= input::ALT_MODIFIER | input::LEFT_ALT_MODIFIER;
                        122
                    }
                },
                0x5b => {
                    data.input_current_modifier |= input::SUPER_MODIFIER | input::LEFT_SUPER_MODIFIER;
                    123
                },
                0x5c => {
                    data.input_current_modifier |= input::SUPER_MODIFIER | input::RIGHT_SUPER_MODIFIER;
                    127
                },
                _ => WindowWindows::translate_keys(w_param, l_param)
            };

            WindowWindows::keydown_worker(data, last_modifier, key_index);
        },
        // WM_SYSKEYDOWN
        0x0104 => {
            WindowWindows::keydown_worker(
                data, data.input_current_modifier,
                WindowWindows::translate_keys(w_param, l_param)
            );
            return DefWindowProcW(h_wnd, msg, w_param, l_param);
        },
        // WM_KEYUP
        0x0101 => {
            let key_index = match w_param {
                0x14 => 119, // CapsLock
                0x10 => match is_right_shift(l_param) {
                    true => 124,
                    false => 120
                },
                0x11..=0x12 => match is_extended_key(l_param) { // Shift, Control, Alt
                    true => w_param + 108, // Right
                    false => w_param + 104 // Left
                },
                0x5b => 123, // LeftSuper
                0x5c => 127, // RightSuper
                _ => WindowWindows::translate_keys(w_param, l_param)
            };

            WindowWindows::keyup_worker(data, key_index);
        },
        // WM_SYSKEYDOWN
        0x105 => {
            WindowWindows::keyup_worker(
                data, WindowWindows::translate_keys(w_param, l_param)
            );
            return DefWindowProcW(h_wnd, msg, w_param, l_param);
        }
        // WM_LBUTTONDOWN
        0x0201 => WindowWindows::keydown_worker(data, data.input_current_modifier, 128),
        // WM_RBUTTONDOWN
        0x0204 => WindowWindows::keydown_worker(data, data.input_current_modifier, 129),
        // WM_RBUTTONDOWN
        0x0207 => WindowWindows::keydown_worker(data, data.input_current_modifier, 130),
        // WM_XBUTTONDOWN
        0x020b => match high_word(w_param as isize) {
            0x0001 => WindowWindows::keydown_worker(data, data.input_current_modifier, 131),
            0x0002 => WindowWindows::keydown_worker(data, data.input_current_modifier, 132),
            _ => unimplemented!()
        },
        0x0202 => WindowWindows::keyup_worker(data, 128), // WM_LBUTTONDOWN
        0x0205 => WindowWindows::keyup_worker(data, 129), // WM_RBUTTONDOWN
        0x0208 => WindowWindows::keyup_worker(data, 130), // WM_MBUTTONDOWN
        0x020c => match high_word(w_param as isize) { // WM_XBUTTONDOWN
            0x0001 => WindowWindows::keyup_worker(data, 131),
            0x0002 => WindowWindows::keyup_worker(data, 132),
            _ => unimplemented!()
        },
        // WM_MOUSEMOVE
        0x0200 => data.input_data.cursor_position = Vector2::new(
            low_word_i(l_param) as f64, high_word_i(l_param) as f64
        ),
        // WM_MOUSEWHEEL
        0x020a => data.input_data.scroll_delta = Vector2::new(
            data.input_data.scroll_delta.x, high_word_i(w_param as isize) as f64 / 120.0
        ),
        // WM_MOUSEHWHEEL
        0x020e => data.input_data.scroll_delta = Vector2::new(
            high_word_i(w_param as isize) as f64 / 120.0, data.input_data.scroll_delta.y
        ),
        _ => return DefWindowProcW(h_wnd, msg, w_param, l_param)
    };
    0
}

struct WindowWindowsData {
    pub width: u32,
    pub height: u32,
    pub settings: WindowSettings,
    pub input_current_modifier: u16,
    pub input_data: &'static mut InputData
}

enum WindowWindowsThreadTask {
    PoolEvents,
    Hide,
    IsFocused,
    Dispose
}

#[link(name = "kernel32")]
extern "system" {
    fn GetModuleHandleW(lp_module_name: *const wchar_t) -> *mut c_void;

    fn FreeLibrary(h_module: *mut c_void) -> u32;

    fn RegisterClassW(lp_wnd_class: *const WndClassW) -> u16;

    fn CreateWindowExW(
        dw_ex_style: u32, lp_class_name: *const wchar_t, lp_window_name: *const wchar_t, dw_style: u32, x: i32, y: i32,
        n_width: i32, n_height: i32, h_wnd_parent: *mut c_void, h_menu: *mut c_void, h_instance: *mut c_void,
        lp_param: *mut c_void
    ) -> *mut c_void;
}

#[link(name = "user32")]
extern "system" {
    fn UnregisterClassW(lp_class_name: *const wchar_t, h_instance: *mut c_void) -> u32;

    fn DestroyWindow(h_wnd: *mut c_void) -> u32;

    fn SetPropW(h_wnd: *mut c_void, lp_string: *const wchar_t, h_data: *const c_void) -> u32;

    fn GetPropW(h_wnd: *mut c_void, lp_string: *const wchar_t) -> *const c_void;

    fn DefWindowProcW(h_wnd: *mut c_void, msg: u32, w_param: usize, l_param: isize) -> isize;

    fn PeekMessageW(
        lp_msg: &mut Msg, h_wnd: *mut c_void, w_msg_filter_min: u32, w_msg_filter_max: u32, w_remove_msg: u32
    ) -> u32;

    fn TranslateMessage(lp_msg: &Msg) -> u32;

    fn DispatchMessageW(lp_msg: &Msg) -> isize;

    fn MapVirtualKeyW(u_code: u32, u_map_type: u32) -> u32;

    fn ShowWindow(h_wnd: *mut c_void, n_cmd_show: u32) -> u32;

    fn AdjustWindowRectEx(lp_rect: &Rect, dw_style: u32, b_menu: u32, dw_ex_style: u32) -> u32;

    fn GetActiveWindow() -> *mut c_void;

    fn ClientToScreen(h_wnd: *mut c_void, lp_point: &mut Vector2<i32>) -> u32;

    fn SetWindowPos(
        h_wnd: *mut c_void, h_wnd_insert_after: *mut c_void, x: i32, y: i32, cx: i32, cy: i32, flags: u32
    ) -> u32;

    fn SetCursorPos(x: i32, y: i32) -> u32;
}
