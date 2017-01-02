﻿using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteoCharts.Render
{
    public class ImageRender
    {
        public static void Render(SKSurface surface)
        {
            Stream imageStream = File.OpenWrite("C:/Users/mjasiak/Desktop");
            SKImage image = surface.Snapshot();
            SKData data = image.Encode();
            data.SaveTo(imageStream);
        }
    }
}
