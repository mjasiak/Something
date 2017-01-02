﻿using MeteoCharts.Data;
using MeteoCharts.Enums;
using MeteoCharts.Interfaces;
using MeteoCharts.Render;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteoCharts.Charts
{
    public class TemperatureChart : IChartable
    {
        private TemperatureChartData _tempChartData;
        private MinMaxValues _minmax = new MinMaxValues();

        public TemperatureChart(TemperatureChartData tempChartData)
        {
            _tempChartData = tempChartData;
        }

        public void GenerateChart(int canvasWidth, int canvasHeight)
        {
            _minmax.SetToDrawLeftColumn();
            using (var surface = SKSurface.Create(canvasWidth, canvasHeight, SKColorType.Rgb565, SKAlphaType.Premul))
            {
                SKCanvas canvas = surface.Canvas;
                canvas.Clear(SKColors.White);

                using (var paint = new SKPaint())
                {
                    paint.IsAntialias = true;
                    paint.Color = new SKColor(208, 208, 208);
                    paint.StrokeCap = SKStrokeCap.Round;

                    canvas = DrawChartAxis( canvas, paint, canvasHeight, canvasWidth);
                }
                ImageRender.Render(surface);
            }
        }

        public SKCanvas DrawChartAxis(SKCanvas canvas, SKPaint paint, int canvasHeight, int canvasWidth)
        {
            SKPaint paint2 = new SKPaint();
            paint2.TextSize = 18.0f;
            paint2.IsAntialias = true;
            paint2.Color = new SKColor(62, 60, 63);
            paint2.IsStroke = false;

            float height = SetChartHeight(canvasHeight);
            float oneInScale = height / _minmax.valuesCollectionCount;
            int row = _minmax.valuesCollectionCount;
            float value = _minmax.max;
            while (row >= 0)
            {
                float rowHeight = height - (row * oneInScale);
                canvas.DrawLine(0, rowHeight, canvasWidth, rowHeight, paint);
                canvas.DrawText(value.ToString(), 0, rowHeight + 5, paint2);
                value -= 10;
                row -= 10;
            }
            return canvas;
        }
        public void DrawChartHours()
        {

        }

        private float SetChartHeight(float height)
        {
            return height * 0.8f;
        }
        private float SetIconsHeight(float height)
        {
            return height * 0.2f;
        }

        private void SetMinMax()
        {
            foreach (var value in _tempChartData.TemperatureChartDataItems)
            {
                _minmax.SetMinMax(value.Value);
            }
        }
    }
}
    


