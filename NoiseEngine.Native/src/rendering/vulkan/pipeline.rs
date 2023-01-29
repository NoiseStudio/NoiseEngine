use std::{ptr, ffi::CString, sync::Arc};

use ash::vk;

use crate::errors::invalid_operation::InvalidOperationError;

use super::{
    errors::universal::VulkanUniversalError, pipeline_layout::PipelineLayout,
    pipeline_shader_stage::PipelineShaderStage, graphics_pipeline_create_info::GraphicsPipelineCreateInfo,
    render_pass::RenderPass
};

pub struct Pipeline<'init> {
    inner: vk::Pipeline,
    _render_pass: Option<Arc<RenderPass<'init>>>,
    layout: Arc<PipelineLayout<'init>>
}

impl<'init> Pipeline<'init> {
    pub fn with_graphics(
        render_pass: &Arc<RenderPass<'init>>, layout: &Arc<PipelineLayout<'init>>, stages: &[PipelineShaderStage],
        flags: vk::PipelineCreateFlags, create_info: GraphicsPipelineCreateInfo
    ) -> Result<Self, VulkanUniversalError> {
        // Stages.
        let mut stage_names = Vec::with_capacity(stages.len());
        let mut final_stages = Vec::with_capacity(stages.len());

        for stage in stages {
            let stage_name = match CString::new(&stage.name) {
                Ok(s) => s,
                Err(_) => return Err(InvalidOperationError::with_str(
                    "Pipeline shader stage name contains null character."
                ).into())
            };

            final_stages.push(vk::PipelineShaderStageCreateInfo {
                s_type: vk::StructureType::PIPELINE_SHADER_STAGE_CREATE_INFO,
                p_next: ptr::null(),
                flags: vk::PipelineShaderStageCreateFlags::empty(),
                stage: stage.stage,
                module: stage.module.inner(),
                p_name: stage_name.as_ptr(),
                p_specialization_info: ptr::null(),
            });
            stage_names.push(stage_name);
        }

        // Vertex input.
        let vertex_input = vk::PipelineVertexInputStateCreateInfo {
            s_type: vk::StructureType::PIPELINE_VERTEX_INPUT_STATE_CREATE_INFO,
            p_next: ptr::null(),
            flags: vk::PipelineVertexInputStateCreateFlags::empty(),
            vertex_binding_description_count: create_info.vertex_input_binding_descriptions.len() as u32,
            p_vertex_binding_descriptions: create_info.vertex_input_binding_descriptions.as_ptr(),
            vertex_attribute_description_count: create_info.vertex_input_attribute_descriptions.len() as u32,
            p_vertex_attribute_descriptions: create_info.vertex_input_attribute_descriptions.as_ptr(),
        };

        // Input assembly.
        let input_assembly = vk::PipelineInputAssemblyStateCreateInfo {
            s_type: vk::StructureType::PIPELINE_INPUT_ASSEMBLY_STATE_CREATE_INFO,
            p_next: ptr::null(),
            flags: vk::PipelineInputAssemblyStateCreateFlags::empty(),
            topology: create_info.primitive_topology,
            primitive_restart_enable: vk::FALSE,
        };

        // Viewport.
        let viewport = vk::PipelineViewportStateCreateInfo {
            s_type: vk::StructureType::PIPELINE_VIEWPORT_STATE_CREATE_INFO,
            p_next: ptr::null(),
            flags: vk::PipelineViewportStateCreateFlags::empty(),
            viewport_count: 1,
            p_viewports: ptr::null(),
            scissor_count: 1,
            p_scissors: ptr::null(),
        };

        // Rasterization.
        let rasterization = vk::PipelineRasterizationStateCreateInfo {
            s_type: vk::StructureType::PIPELINE_RASTERIZATION_STATE_CREATE_INFO,
            p_next: ptr::null(),
            flags: vk::PipelineRasterizationStateCreateFlags::empty(),
            depth_clamp_enable: vk::FALSE,
            rasterizer_discard_enable: vk::FALSE,
            polygon_mode: vk::PolygonMode::FILL,
            cull_mode: vk::CullModeFlags::BACK,
            front_face: vk::FrontFace::CLOCKWISE,
            depth_bias_enable: vk::FALSE,
            depth_bias_constant_factor: 0.0,
            depth_bias_clamp: 0.0,
            depth_bias_slope_factor: 0.0,
            line_width: 1.0,
        };

        // Multisample.
        let multisample = vk::PipelineMultisampleStateCreateInfo {
            s_type: vk::StructureType::PIPELINE_MULTISAMPLE_STATE_CREATE_INFO,
            p_next: ptr::null(),
            flags: vk::PipelineMultisampleStateCreateFlags::empty(),
            rasterization_samples: vk::SampleCountFlags::TYPE_1,
            sample_shading_enable: vk::FALSE,
            min_sample_shading: 1.0,
            p_sample_mask: ptr::null(),
            alpha_to_coverage_enable: vk::FALSE,
            alpha_to_one_enable: vk::FALSE,
        };

        // Depth stencil.
        let depth_stencil;
        if render_pass.depth_testing() {
            depth_stencil = vk::PipelineDepthStencilStateCreateInfo {
                s_type: vk::StructureType::PIPELINE_DEPTH_STENCIL_STATE_CREATE_INFO,
                p_next: ptr::null(),
                flags: vk::PipelineDepthStencilStateCreateFlags::empty(),
                depth_test_enable: vk::TRUE,
                depth_write_enable: vk::TRUE,
                depth_compare_op: vk::CompareOp::LESS,
                depth_bounds_test_enable: vk::FALSE,
                stencil_test_enable: vk::FALSE,
                front: vk::StencilOpState::default(),
                back: vk::StencilOpState::default(),
                min_depth_bounds: 0.0,
                max_depth_bounds: 1.0,
            };
        } else {
            depth_stencil = vk::PipelineDepthStencilStateCreateInfo::default();
        }

        // Color blend.
        let color_blend_attachment = vk::PipelineColorBlendAttachmentState {
            blend_enable: vk::FALSE,
            src_color_blend_factor: vk::BlendFactor::ONE,
            dst_color_blend_factor: vk::BlendFactor::ZERO,
            color_blend_op: vk::BlendOp::ADD,
            src_alpha_blend_factor: vk::BlendFactor::ONE,
            dst_alpha_blend_factor: vk::BlendFactor::ZERO,
            alpha_blend_op: vk::BlendOp::ADD,
            color_write_mask: vk::ColorComponentFlags::RGBA,
        };

        let color_blend = vk::PipelineColorBlendStateCreateInfo {
            s_type: vk::StructureType::PIPELINE_COLOR_BLEND_STATE_CREATE_INFO,
            p_next: ptr::null(),
            flags: vk::PipelineColorBlendStateCreateFlags::empty(),
            logic_op_enable: vk::FALSE,
            logic_op: vk::LogicOp::COPY,
            attachment_count: 1,
            p_attachments: &color_blend_attachment,
            blend_constants: [0.0, 0.0, 0.0, 0.0],
        };

        // Dynamic.
        let dynamic_states = [
            vk::DynamicState::VIEWPORT,
            vk::DynamicState::SCISSOR
        ];

        let dynamic = vk::PipelineDynamicStateCreateInfo {
            s_type: vk::StructureType::PIPELINE_DYNAMIC_STATE_CREATE_INFO,
            p_next: ptr::null(),
            flags: vk::PipelineDynamicStateCreateFlags::empty(),
            dynamic_state_count: dynamic_states.len() as u32,
            p_dynamic_states: dynamic_states.as_ptr(),
        };

        // Construct.
        let create_info_final = vk::GraphicsPipelineCreateInfo {
            s_type: vk::StructureType::GRAPHICS_PIPELINE_CREATE_INFO,
            p_next: ptr::null(),
            flags,
            stage_count: final_stages.len() as u32,
            p_stages: final_stages.as_ptr(),
            p_vertex_input_state: &vertex_input,
            p_input_assembly_state: &input_assembly,
            p_tessellation_state: ptr::null(),
            p_viewport_state: &viewport,
            p_rasterization_state: &rasterization,
            p_multisample_state: &multisample,
            p_depth_stencil_state: &depth_stencil,
            p_color_blend_state: &color_blend,
            p_dynamic_state: &dynamic,
            layout: layout.inner(),
            render_pass: render_pass.inner(),
            subpass: 0,
            base_pipeline_handle: vk::Pipeline::null(),
            base_pipeline_index: 0,
        };

        let initialized = layout.device().initialized()?;
        let inner = match unsafe {
            initialized.vulkan_device().create_graphics_pipelines(
                vk::PipelineCache::null(), &[create_info_final], None
            )
        } {
            Ok(i) => i[0],
            Err((_, err)) => return Err(err.into())
        };

        Ok(Self { inner, _render_pass: Some(render_pass.clone()), layout: layout.clone() })
    }

    pub fn with_compute(
        layout: &Arc<PipelineLayout<'init>>, stage: PipelineShaderStage, flags: vk::PipelineCreateFlags
    ) -> Result<Self, VulkanUniversalError> {
        let stage_name = match CString::new(String::from(stage.name)) {
            Ok(s) => s,
            Err(_) => return Err(
                InvalidOperationError::with_str("Pipeline shader stage name contains null character.").into()
            )
        };

        let final_stage = vk::PipelineShaderStageCreateInfo {
            s_type: vk::StructureType::PIPELINE_SHADER_STAGE_CREATE_INFO,
            p_next: ptr::null(),
            flags: vk::PipelineShaderStageCreateFlags::empty(),
            stage: stage.stage,
            module: stage.module.inner(),
            p_name: stage_name.as_ptr(),
            p_specialization_info: ptr::null(),
        };

        let create_info = vk::ComputePipelineCreateInfo {
            s_type: vk::StructureType::COMPUTE_PIPELINE_CREATE_INFO,
            p_next: ptr::null(),
            flags,
            stage: final_stage,
            layout: layout.inner(),
            base_pipeline_handle: vk::Pipeline::null(),
            base_pipeline_index: 0,
        };

        let initialized = layout.device().initialized()?;
        let inner = match unsafe {
            initialized.vulkan_device().create_compute_pipelines(
                vk::PipelineCache::null(), &[create_info], None
            )
        } {
            Ok(i) => i[0],
            Err((_, err)) => return Err(err.into())
        };

        Ok(Self { inner, _render_pass: None, layout: layout.clone() })
    }

    pub fn inner(&self) -> vk::Pipeline {
        self.inner
    }

    pub fn layout(&self) -> &Arc<PipelineLayout<'init>> {
        &self.layout
    }
}

impl Drop for Pipeline<'_> {
    fn drop(&mut self) {
        unsafe {
            self.layout.device().initialized().unwrap().vulkan_device().destroy_pipeline(
                self.inner, None
            );
        }
    }
}
