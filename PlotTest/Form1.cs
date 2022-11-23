using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace PlotTest
{
    public partial class Form1 : Form
    {
        private static readonly ChartDashStyle[] _styles = { ChartDashStyle.Solid, ChartDashStyle.Dash, ChartDashStyle.Dot };
        private static readonly MarkerStyle[] _markers = { MarkerStyle.Circle, MarkerStyle.Cross, MarkerStyle.Diamond };
        public Form1()
        {
            InitializeComponent();

            string dataPath = @"C:\Users\User\Documents\Gaut\Manips Thèse\Distance\Résultats\Bruit\LoudnessProximalLin.csv";
            string factorsPath = @"C:\Users\User\Documents\Gaut\Manips Thèse\Distance\Résultats\Bruit\Exp1Factors.txt";
            //string dataPath = @"C:\Users\User\Desktop\Distance.csv";
            //string factorsPath = @"C:\Users\User\Desktop\Factors.txt";

            Data data = new Data(dataPath, factorsPath);

            string variableX = "Room";
            bool logY = true;
            string variableY = "Visibility";
            List<string> restrictionLevels = new List<string>() {};

            Plot(data, variableX, logY, variableY, restrictionLevels);
            ChartLook(false, true, true, true, "Source distance (m)", "Source distance (m)");

            // Exports an emf file for external svg conversion
            chart1.SaveImage(@"C:\Users\User\Desktop\Figure.emf", ChartImageFormat.Emf);
        }

        /// <summary>
        /// Plots an interaction between two factors.
        /// </summary>
        /// <param name="data">Collection of data on which the statistical analysis has been done.</param>
        /// <param name="variableY">Factors from which the levels are plotted on different lines (y-axis).</param>
        /// <param name="variableX">Factor that is used as x-axis.</param>
        private void Plot(Data data, string variableX, bool logY, string variableY = null, List<string> restrictionLevels = null)
        {
            for (int i = 0; i < data.GetLevels(variableY).Count; i++)
            {
                // Computes the offset to apply to each line depending on if there are 2 or 3 lines.
                float xOffset = 0.05f ;
                switch (data.GetLevels(variableY).Count)
                {
                    case 1: xOffset *= 0; break;
                    case 2: xOffset *= (i == 0 ? -1 : 1); break;
                    default: xOffset *= (i - 1); break;
                }

                string lineName = data.GetLevels(variableY)[i] ?? variableX;

                // MEAN
                var meanLine = data.MeanLine(variableX, logY, variableY, restrictionLevels);
                chart1.Series.Add(lineName);
                chart1.Series[lineName].ChartType = SeriesChartType.Line;

                LineLook(lineName, Color.Black, _styles[i], _markers[i], true);

                int x = 0; // For the X-offset of non-numerical x values

                foreach (var point in meanLine[i])
                {
                    if (int.TryParse(point.x, out int xVal)) chart1.Series[lineName].Points.AddXY(xVal * (1 + xOffset), point.y);
                    else
                    {
                        chart1.Series[lineName].Points.AddXY(x + xOffset, point.y);
                        CustomLabel cL = new CustomLabel(x - 0.5, x + 0.5, point.x, 0, LabelMarkStyle.None, GridTickTypes.Gridline);
                        chart1.ChartAreas[0].AxisX.CustomLabels.Add(cL);
                        x++;
                    }
                }

                // CONFIDENCE INTERVAL
                var sdLine = data.Std(variableX, logY, variableY, restrictionLevels);
                chart1.Series.Add($"{lineName} sd");
                chart1.Series[$"{lineName} sd"].ChartType = SeriesChartType.ErrorBar;
                LineLook($"{lineName} sd", Color.Black, ChartDashStyle.Solid, MarkerStyle.None , false);

                x = 0;
                
                foreach (var point in sdLine[i])
                {
                    if (int.TryParse(point.x, out int xVal)) chart1.Series[$"{lineName} sd"].Points.AddXY(xVal * (1 + xOffset), 0, point.y.l, point.y.h);
                    else
                    {
                        chart1.Series[$"{lineName} sd"].Points.AddXY(x + xOffset, 0, point.y.l, point.y.h);
                        x++;
                    }
                }
            }
        }

        /// <summary>
        /// Changes the aspect of the plot.
        /// </summary>
        /// <param name="chart">Name of the line.</param>
        /// <param name="color">Color of the line.</param>
        /// <param name="lineStyle">Style (solid, dash or dot) of the line.</param>
        /// <param name="markerStyle">Style (circle, cross or diamond) of the markers.</param>
        /// <param name="isVisibleInLegend">Specify if the chart is visible in the legend.</param>
        private void LineLook(string line, Color color, ChartDashStyle lineStyle, MarkerStyle markerStyle, bool isVisibleInLegend)
        {
            chart1.Series[line].BorderWidth = 2;
            chart1.Series[line].Color = color;
            chart1.Series[line].BorderDashStyle = lineStyle;
            chart1.Series[line].MarkerStyle = markerStyle;
            chart1.Series[line].MarkerSize = 8;
            chart1.Series[line].IsVisibleInLegend = isVisibleInLegend;

            if (line.Contains("sd")) chart1.Series[line].CustomProperties = "PixelPointWidth = 10"; ;
        }

        private void ChartLook(bool numX, bool logX, bool numY, bool logY, string xTitle, string yTitle)
        {
            ChartArea cA = chart1.ChartAreas[0];

            cA.AxisX.MajorTickMark.Enabled = false;
            cA.AxisX2.MajorTickMark.Enabled = false;
            cA.AxisY.MajorTickMark.Enabled = false;
            cA.AxisY2.MajorTickMark.Enabled = false;
            
            float offset = 0.2f;            
            int[] xMajorTicks = { 1, 2, 4, 8, 16 };
            //int[] yMajorTicks = xMajorTicks;
            int[] yMajorTicks = { 5, 10, 15, 20 };

            if (numX)
            {
                int minX = 1;
                int maxX = 16;
                float margin = 1.1f;

                cA.AxisX.Minimum = logX ? minX / margin : minX - margin;
                cA.AxisX.Maximum = logY ? maxX * margin : maxX + margin;

                cA.AxisX2.Enabled = AxisEnabled.True;
                cA.AxisX2.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
                cA.AxisX2.MajorGrid.LineColor = Color.LightGray;
                cA.AxisX2.LabelStyle.Enabled = false;
                cA.AxisX2.LineWidth = 0;

                cA.AxisX2.Minimum = cA.AxisX.Minimum;
                cA.AxisX2.Maximum = cA.AxisX.Maximum;

                cA.AxisX.IsLogarithmic = logX;
                cA.AxisX2.IsLogarithmic = logX;
                cA.AxisX.LogarithmBase = 2;
                cA.AxisX2.LogarithmBase = 2;

                if (logX)
                {
                    var ticks = GetLogLabels(minX, maxX, xMajorTicks, offset);
                    foreach (var tick in ticks.major) cA.AxisX.CustomLabels.Add(tick);
                    foreach (var tick in ticks.minor) cA.AxisX2.CustomLabels.Add(tick);
                }
            }
            if (numY)
            {
                int minY = 4;
                int maxY = 20;
                float margin = 1.1f;

                cA.AxisY.Minimum = logX ? minY / margin : minY - margin;
                cA.AxisY.Maximum = logY ? maxY * margin : maxY + margin;

                cA.AxisY2.Enabled = AxisEnabled.True;
                cA.AxisY2.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
                cA.AxisY2.MajorGrid.LineColor = Color.LightGray;
                cA.AxisY2.LabelStyle.Enabled = false;
                cA.AxisY2.LineWidth = 0;

                cA.AxisY2.Minimum = cA.AxisY.Minimum;
                cA.AxisY2.Maximum = cA.AxisY.Maximum;

                cA.AxisY.IsLogarithmic = logY;
                cA.AxisY2.IsLogarithmic = logY;
                cA.AxisY.LogarithmBase = 2;
                cA.AxisY2.LogarithmBase = 2;

                if (logY)
                {
                    var ticks = GetLogLabels(minY, maxY, yMajorTicks, offset);
                    foreach (var tick in ticks.major) cA.AxisY.CustomLabels.Add(tick);
                    foreach (var tick in ticks.minor) cA.AxisY2.CustomLabels.Add(tick);
                }
            }

            cA.AxisX.LineWidth = 0;
            cA.AxisX.MajorGrid.LineColor = Color.Gray;
            cA.AxisX.Title = xTitle;

            cA.AxisY.LineWidth = 0;
            cA.AxisY.MajorGrid.LineColor = Color.Gray;
            cA.AxisY.Title = yTitle;
        }

        private (List<CustomLabel> major, List<CustomLabel> minor) GetLogLabels(int min, int max, int[] majorTicks , float offset)
        {
            List<CustomLabel> majorCL = new List<CustomLabel>();
            List<CustomLabel> minorCL = new List<CustomLabel>();

            for (int tick = min; tick <= max; tick++)
            {
                double linPos = Math.Log(tick, 2); // Log values on linear axis

                CustomLabel cL = new CustomLabel();

                cL.FromPosition = linPos - offset;
                cL.ToPosition = linPos + offset;

                cL.Text = tick.ToString();

                cL.LabelMark = LabelMarkStyle.Box;
                cL.GridTicks = GridTickTypes.Gridline;

                if (majorTicks.Contains(tick)) majorCL.Add(cL);
                else minorCL.Add(cL);
            }
            return (majorCL, minorCL);
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }
    }
}
