using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BuildingPlus.Camera
{
    internal class CameraToggle : MonoBehaviour
    {
        private ZoomCamera _zoomCamera;
        private CameraController2D _controller;

        internal void SetCameras(ZoomCamera zoomCamera, CameraController2D controller) 
        { 
            _zoomCamera = zoomCamera;
            _controller = controller;
        }

        void Update() 
        {
            if (Input.GetKeyDown(BuildingPlusConfig.ToggleCameraKey.Value))
            {
                ToggleCameras();
            }
        }

        private void ToggleCameras()
        {
            if (_controller != null) _controller.enabled = _controller.enabled;
            if (_zoomCamera != null) _zoomCamera.enabled = !_zoomCamera.enabled;
        }

    }
}
