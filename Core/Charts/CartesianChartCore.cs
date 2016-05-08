﻿//The MIT License(MIT)

//copyright(c) 2016 Alberto Rodriguez

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.Linq;

namespace LiveCharts.Charts
{
    public class CartesianChartCore : ChartCore
    {
        public CartesianChartCore(IChartView view, IChartUpdater updater) : base(view, updater)
        {
            updater.Chart = this;
            updater.UpdateFrequency();
        }

        public override void PrepareAxes()
        {
            base.PrepareAxes();

            if (View.Series.Any(x => !(x.Model is ICartesianSeries)))
                throw new Exception(
                    "There is a invalid series in the series collection, " +
                    "verify that all the series are desiged to be plotted in a cartesian chart.");

            var xMode = AxisLimitsMode.Stretch;
            var yMode = AxisLimitsMode.Stretch;

            foreach (var cartesianSeries in View.Series.Select(x => x.Model).Cast<ICartesianSeries>())
            {
                xMode = cartesianSeries.XAxisMode < xMode ? xMode : cartesianSeries.XAxisMode;
                yMode = cartesianSeries.YAxisMode < yMode ? yMode : cartesianSeries.YAxisMode;
            }

            if (View.Series.Any(x => x is IBubbleSeries))
            {
                var vs = View.Series.Select(x => x.Values.Value3Limit).ToArray();
                Value3Limit = new Limit(vs.Select(x => x.Min).DefaultIfEmpty(0).Min(),
                    vs.Select(x => x.Max).DefaultIfEmpty(0).Max());
            }

            for (int index = 0; index < AxisX.Count; index++)
            {
                var xi = AxisX[index];
                xi.CalculateSeparator(this, AxisTags.X);
                switch (xMode)
                {
                    case AxisLimitsMode.Stretch:
                        xi.MaxLimit = Math.Ceiling(xi.MaxLimit/xi.Magnitude)*xi.Magnitude;
                        xi.MinLimit = Math.Floor(xi.MinLimit/xi.Magnitude)*xi.Magnitude;
                        if (Math.Abs(xi.MaxLimit - xi.MinLimit) < xi.S*0.01)
                        {
                            xi.MinLimit = (Math.Round(xi.MaxLimit/xi.S) - 1)*xi.S;
                            xi.MaxLimit = (Math.Round(xi.MaxLimit/xi.S) + 1)*xi.S;
                        }
                        break;
                    case AxisLimitsMode.Separator:
                        if (xi.MaxValue == null) xi.MaxLimit = (Math.Round(xi.MaxLimit/xi.S) + 1)*xi.S;
                        if (xi.MinValue == null) xi.MinLimit = (Math.Truncate(xi.MinLimit/xi.S) - 1)*xi.S;
                        break;
                    case AxisLimitsMode.Bubble:
                        var maxRadius = View.Series.Where(x => x.ScalesXAt == index)
                            .OfType<IBubbleSeries>()
                            .Select(x => x.MaxBubbleDiameter)
                            .DefaultIfEmpty(0).Max()/2;
                        var pxRadius = Math.Round(ChartFunctions.FromDrawMargin(maxRadius, AxisTags.X, this, index));
                        if (xi.MaxValue == null) xi.MaxLimit = (Math.Round(xi.MaxLimit/pxRadius) + 1)*pxRadius;
                        if (xi.MinValue == null) xi.MinLimit = (Math.Truncate(xi.MinLimit/pxRadius) - 1)*pxRadius;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            for (int index = 0; index < AxisY.Count; index++)
            {
                var yi = AxisY[index];
                yi.CalculateSeparator(this, AxisTags.Y);
                switch (yMode)
                {
                    case AxisLimitsMode.Stretch:
                        yi.MaxLimit = Math.Ceiling(yi.MaxLimit/yi.Magnitude)*yi.Magnitude;
                        yi.MinLimit = Math.Floor((yi.MinLimit/yi.Magnitude))*yi.Magnitude;
                        if (Math.Abs(yi.MaxLimit - yi.MinLimit) < yi.S*0.01)
                        {
                            yi.MinLimit = (Math.Round(yi.MaxLimit/yi.S) - 1)*yi.S;
                            yi.MaxLimit = (Math.Round(yi.MaxLimit/yi.S) + 1)*yi.S;
                        }
                        break;
                    case AxisLimitsMode.Separator:
                        if (yi.MaxValue == null) yi.MaxLimit = (Math.Round(yi.MaxLimit/yi.S) + 1)*yi.S;
                        if (yi.MinValue == null) yi.MinLimit = (Math.Truncate(yi.MinLimit/yi.S) - 1)*yi.S;
                        break;
                    case AxisLimitsMode.Bubble:
                        var maxRadius = View.Series.Where(x => x.ScalesYAt == index)
                            .OfType<IBubbleSeries>()
                            .Select(x => x.MaxBubbleDiameter)
                            .DefaultIfEmpty(0).Max()/2;
                        var pxRadius = Math.Round(ChartFunctions.FromDrawMargin(maxRadius, AxisTags.Y, this, index));
                        if (yi.MaxValue == null) yi.MaxLimit = (Math.Round(yi.MaxLimit/pxRadius) + 1)*pxRadius;
                        if (yi.MinValue == null) yi.MinLimit = (Math.Truncate(yi.MinLimit/pxRadius) - 1)*pxRadius;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            CalculateComponentsAndMargin();
        }
    }
}
