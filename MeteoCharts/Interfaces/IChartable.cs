﻿using MeteoCharts.Charts.ChartObjects;
using MeteoCharts.Data;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteoCharts.Interfaces
{
    public interface IChartable
    {
        void DrawChart(int canvasHeight, int spaceBetweenValues);
    }
}
