﻿using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using LibPolarizationCamera.PixelTypes;

namespace PolarizationPlanes
{
    public partial class MainForm : Form
    {
        private DisplayFrameFilter displayFrameFilter = new DisplayFrameFilter();

        public MainForm()
        {
            InitializeComponent();
        }

        private static TIS.Imaging.Device FindPolarizationCameraByName(TIS.Imaging.Device[] devices)
        {
            // We simply select the first polarization camera
            foreach (var dev in devices)
            {
                // Monochrome polarization cameras are DZK 33UX250 and DZK 33GX250
                if (dev.Name.StartsWith("DZK"))
                    return dev;
                // Color polarization cameras are DYK 33UX250 and DYK 33GX250
                if (dev.Name.StartsWith("DYK"))
                    return dev;
            }

            return null;
        }

        private static TIS.Imaging.VideoFormat FindLargestVideoFormat(TIS.Imaging.VideoFormat[] formats, params Guid[] subtypes)
        {
            // Select the largest video format that matches one of the supplied media subtypes
            return formats
                .Where(fmt => subtypes.Contains(fmt.FrameType.Subtype))
                .OrderByDescending(fmt => fmt.Width * fmt.Height)
                .First();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Select polarization (DZK or DYK) camera
            icImagingControl1.Device = FindPolarizationCameraByName(icImagingControl1.Devices);

            // Select PPM1 or PPB1 format, set maximum frame rate
            icImagingControl1.VideoFormat = FindLargestVideoFormat(icImagingControl1.VideoFormats, PPM1.MediaSubtype, PPB1.MediaSubtype);
            icImagingControl1.DeviceFrameRate = icImagingControl1.DeviceFrameRates.Max();

            // Set sink type to accept anything, so that the graph can connect
            icImagingControl1.Sink = new TIS.Imaging.FrameHandlerSink();

            // Overlay over is not supported for PPM1 or PPB1
            icImagingControl1.OverlayBitmapPosition = TIS.Imaging.PathPositions.None;

            // Insert DisplayFrameFilter to convert PPM1/PPB1 -> Y800 for display
            icImagingControl1.DisplayFrameFilters.Add(new TIS.Imaging.FrameFilter(displayFrameFilter));

            // Initialize combo box with available display modes
            cboPolarizationPlanes.DataSource = Enum.GetValues(typeof(DisplayFrameFilter.DisplayMode));

            // Start live video
            icImagingControl1.LiveStart();            
        }

        private void cboPolarizationPlanes_SelectedIndexChanged(object sender, EventArgs e)
        {
            displayFrameFilter.OutputMode = (DisplayFrameFilter.DisplayMode)cboPolarizationPlanes.SelectedItem;
        }
    }
}
