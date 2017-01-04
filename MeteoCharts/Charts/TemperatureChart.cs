﻿using MeteoCharts.Charts.ChartObjects;
using MeteoCharts.Data;
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
        private List<TemperatureObject> _tempObjects = new List<TemperatureObject>();
        private ChartRangeSetting _chartSetting = new ChartRangeSetting();
        private List<ChartAxis> _chartAxis = new List<ChartAxis>();
        private int canvasWidth;

        public TemperatureChart(TemperatureChartData tempChartData)
        {
            _tempChartData = tempChartData;                
        }

        public void DrawChart(int canvasHeight, int spaceBetweenValues)
        {
            MathChart(canvasHeight, spaceBetweenValues);
            using (var surface = SKSurface.Create(canvasWidth, canvasHeight, SKColorType.Rgb565, SKAlphaType.Premul))
            {
                SKCanvas canvas = surface.Canvas;
                canvas.Clear(SKColors.White);  
                                  
                canvas = DrawChartAxis(canvas);
                canvas = DrawChartBezier(canvas,spaceBetweenValues);
                canvas = DrawChartValues(canvas);
                
                ImageRender.Render(surface);
            }
        }
        private void MathChart(int canvasHeight, int spaceBetween)
        {
            canvasWidth = _tempChartData.TemperatureChartDataItems.Count() * spaceBetween;
            _chartSetting = SetChartRange(_chartSetting, GetValues(), canvasHeight);
            MathChartAxis(_chartSetting, canvasHeight);
            GetObjects();
            MathChartValues(spaceBetween);         
        }

        private ChartRangeSetting SetChartRange(ChartRangeSetting chartSetting, IEnumerable<int> values, int canvasHeight)
        {
            int min = values.First();
            int max = values.First();
            foreach(var value in values)
            {
                if (min > value) min = value;
                if (max < value) max = value;
            }

            chartSetting.min = min;
            chartSetting.max = max;

            chartSetting.setInScale(min, max);
            chartSetting.heightOfAxis = SetChartHeight(canvasHeight);

            chartSetting.oneInScale = chartSetting.heightOfAxis / chartSetting.valuesRangeInScale;
            return chartSetting;
        }

        private void MathChartAxis(ChartRangeSetting chartSetting, int canvasHeight)
        {
            float height = SetChartHeight(canvasHeight);
            int row = chartSetting.valuesRangeInScale;
            int value = chartSetting.maxInScale;
            while (row >= 0)
            {
                float rowHeight = height - (row * chartSetting.oneInScale);
                ChartAxis axis = new ChartAxis(0, rowHeight, canvasWidth, rowHeight, value);               
                value -= 10;
                row -= 10;
                _chartAxis.Add(axis);
            }            
        }
        private void MathChartValue(ChartObject chartObject, int spaceBetween)
        {          
                chartObject.x = spaceBetween;
                chartObject.y = GetHeightOfValueInPixels(_chartSetting, chartObject);
        }
        #region MathHelper
        private void MathChartValues(int spaceBetween)
        {
            int space = 0;
            foreach (var chartObj in _tempObjects)
            {
                MathChartValue(chartObj, space);
                space += spaceBetween;
            }
        }
        #endregion

        private SKCanvas DrawChartAxis(SKCanvas canvas)
        {
            #region Painting
            SKPaint paint = new SKPaint();
            paint.IsAntialias = true;
            paint.Color = new SKColor(208, 208, 208);
            paint.StrokeCap = SKStrokeCap.Round;

            SKPaint paint2 = new SKPaint();
            paint2.TextSize = 18.0f;
            paint2.IsAntialias = true;
            paint2.Color = new SKColor(62, 60, 63);
            paint2.IsStroke = false;
            paint2.TextAlign = SKTextAlign.Right;
            #endregion
            foreach (var axis in _chartAxis)
            {
                if (axis.value != 0)
                {
                    paint.Color = new SKColor(172, 172, 172);
                    paint.StrokeWidth = 1;
                }
                else {
                    paint.Color = new SKColor(125, 127, 126);
                    paint.StrokeWidth = 2;
                }
                canvas.DrawLine(axis.x0 + 40, axis.y0, axis.x1 + 40, axis.y1, paint);
                canvas.DrawText(axis.value.ToString(), axis.x0 + 30, axis.y0 + 5, paint2);
            }
            return canvas;
        }
        private SKCanvas DrawChartValues(SKCanvas canvas)
        {
            SKPaint paint = new SKPaint();
            paint.Color = new SKColor(0, 0, 0);

            foreach (var obj in _tempObjects)
            {
                //obj.x = obj.x + 75;
                paint.Color = new SKColor(0, 0, 0);
                canvas.DrawCircle(obj.x, obj.y, 8,paint);                
                paint.TextSize = 32.0f;
                paint.TextAlign = SKTextAlign.Center;
                canvas.DrawText(obj.value.ToString() + "°", obj.x, obj.y - 30, paint);
                paint.StrokeCap = SKStrokeCap.Round;
                canvas.DrawLine(obj.x, obj.y, obj.x, _chartSetting.heightOfAxis,paint);
                paint.Color = new SKColor(254, 254, 254);
                canvas.DrawCircle(obj.x, obj.y, 5, paint);
                paint.Color = new SKColor(0, 0, 0);
                paint.TextSize = 18.0f;
                paint.TextAlign = SKTextAlign.Center;
                if(_tempObjects[0] == obj) canvas.DrawText("TERAZ", obj.x, _chartSetting.heightOfAxis * 1.05f, paint);
                else canvas.DrawText(obj.hour.ToString(@"hh\:mm"), obj.x, _chartSetting.heightOfAxis * 1.05f, paint);
            }
            return canvas;
        }
        private SKCanvas DrawChartBezier(SKCanvas canvas, int spaceBetweenValues)
        {
            SKPath path = new SKPath();
            SKPaint paint = new SKPaint();
            paint.Color = new SKColor(0, 0, 0);
            paint.IsAntialias = true;
            paint.StrokeWidth = 4;
            paint.Style = SKPaintStyle.Stroke;

            spaceBetweenValues /= 2;

            TemperatureObject previousObj= _tempObjects[0];           
            path.MoveTo(previousObj.x, previousObj.y);

            for (int i = 1; i <= _tempObjects.Count()-1; i++)
            {               
                TemperatureObject nextObj = _tempObjects[i];
                float halfRoad = nextObj.x - ((nextObj.x - previousObj.x) / 2);
                path.CubicTo(previousObj.x+spaceBetweenValues,previousObj.y,nextObj.x-spaceBetweenValues,nextObj.y,nextObj.x,nextObj.y);
                canvas.DrawPath(path, paint);
                previousObj = _tempObjects[i];
            } 
            return canvas;
        }

        private float SetChartHeight(float height)
        {
            return height * 0.8f;
        }

        private IEnumerable<int> GetValues()
        {
            List<int> values = new List<int>();
            foreach(var value in _tempChartData.TemperatureChartDataItems)
            {
                values.Add(value.Value);
            }
            return values;
        }
        private void GetObjects()
        {
            foreach(var item in _tempChartData.TemperatureChartDataItems)
            {
                TemperatureObject tempObj = new TemperatureObject();
                tempObj.hour = item.Time;
                tempObj.icon = item.IconType;
                tempObj.value = item.Value;
                _tempObjects.Add(tempObj);
            }
        }
        private float GetHeightOfValueInPixels(ChartRangeSetting chartSett, ChartObject chartObj)
        {
            int minus = chartSett.maxInScale - chartObj.value;
            int minusInScale = chartSett.valuesRangeInScale - minus;
            return chartSett.heightOfAxis - (minusInScale * chartSett.oneInScale);
        }
    }
}
    


