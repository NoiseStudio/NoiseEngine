using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoiseEngine {
    internal readonly struct SaveCameraData {

        private readonly Entity entity;
        private readonly UInt2 windowSize;
        private readonly string windowTitle;
        private readonly float cameraNearClipPlane;
        private readonly float cameraFarClipPlane;
        private readonly AngleFloat cameraFieldOfView;
        private readonly float cameraOrthographicSize;

        public SaveCameraData(CameraData cameraData) {
            entity = cameraData.Entity;
        }

    }
}
