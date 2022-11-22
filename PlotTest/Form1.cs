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
        public Form1()
        {
            InitializeComponent();

            string dataPath = @"C:\Users\User\Documents\Gaut\Manips Thèse\Distance\Résultats\Bruit\Distance.csv";
            string factorsPath = @"C:\Users\User\Documents\Gaut\Manips Thèse\Distance\Résultats\Bruit\Exp1Factors.txt";
            //string dataPath = @"C:\Users\User\Desktop\Distance.csv";
            //string factorsPath = @"C:\Users\User\Desktop\Factors.txt";

            Data data = new Data(dataPath, factorsPath);

            //PlotSimpleEffect(data, "Visibility", false);

            bool logY = true;
            List<string> restrictionLevels = new List<string>() {};
            PlotInteraction(data, "Room", "Distance", logY, restrictionLevels);

            ChartLook(true, true, true, true, "Source distance (m)", "Source distance (m)");
        }

        /// <summary>
        /// Plots a single effect of the specified factor.
        /// </summary>
        /// <param name="data">Collection of data on which the statistical analysis has been done.</param>
        /// <param name="variable">Factor from which to plot the simple effect.</param>
        private void PlotSimpleEffect(Data data, string variable, bool logY)
        {
            // MEAN
            chart1.Series.Add(variable);
            chart1.Series[variable].ChartType = SeriesChartType.Line;
            LineLook(variable, Color.Black, ChartDashStyle.Solid, true);
            foreach (var point in data.SimpleEffectMeanLine(variable, logY))
            {
                chart1.Series[variable].Points.AddXY(point.x, point.y);
            }

            // CONFIDENCE INTERVAL
            chart1.Series.Add($"{variable} sd");
            chart1.Series[$"{variable} sd"].IsVisibleInLegend = false;
            chart1.Series[$"{variable} sd"].ChartType = SeriesChartType.ErrorBar;
            LineLook($"{variable} sd", Color.Black, ChartDashStyle.Solid, true);
            foreach (var point in data.SimpleEffectStd(variable, logY))
            {
                chart1.Series[$"{variable} sd"].Points.AddXY(point.x, 0, point.y.l, point.y.h);
            }
        }

        /// <summary>
        /// Plots an interaction between two factors.
        /// </summary>
        /// <param name="data">Collection of data on which the statistical analysis has been done.</param>
        /// <param name="variableY">Factors from which the levels are plotted on different lines (y-axis).</param>
        /// <param name="variableX">Factor that is used as x-axis.</param>
        private void PlotInteraction(Data data, string variableY, string variableX, bool logY, List<string> restrictionLevels = null)
        {
            for (int i = 0; i < data.GetLevels(variableY).Count; i++)
            {
                // Computes the offset to apply to each line depending on if there are 2 or 3 lines.
                float xOffset = 0.05f * (data.GetLevels(variableY).Count == 2 ? (i == 0 ? -1 : 1) : i - 1);

                string lineName = data.GetLevels(variableY)[i];

                // MEAN
                var meanLine = data.InteractionMeanLine(variableY, variableX, logY, restrictionLevels);
                chart1.Series.Add(lineName);
                chart1.Series[lineName].ChartType = SeriesChartType.Line;
                LineLook(lineName, Color.Black, _styles[i], true);
                foreach (var point in meanLine[i])
                {
                    if (int.TryParse(point.x, out int xVal)) chart1.Series[lineName].Points.AddXY(xVal * (1 + xOffset), point.y);
                    else chart1.Series[lineName].Points.AddXY(point.x, point.y);
                }

                // CONFIDENCE INTERVAL
                var sdLine = data.InteractionStd(variableY, variableX, logY, restrictionLevels);
                chart1.Series.Add($"{lineName} sd");
                chart1.Series[$"{lineName} sd"].ChartType = SeriesChartType.ErrorBar;
                LineLook($"{lineName} sd", Color.Black, ChartDashStyle.Solid, false);
                foreach (var point in sdLine[i])
                {
                    if (int.TryParse(point.x, out int xVal)) chart1.Series[$"{lineName} sd"].Points.AddXY(xVal * (1 + xOffset), 0, point.y.l, point.y.h);
                    else chart1.Series[$"{lineName} sd"].Points.AddXY(point.x, 0, point.y.l, point.y.h);
                }
            }
        }

        /// <summary>
        /// Changes the aspect of the plot.
        /// </summary>
        /// <param name="chart">Name of the line.</param>
        /// <param name="color">Color of the line.</param>
        /// <param name="style">Style (solid, dash or dot) of the line.</param>
        /// <param name="isVisible">Specify if the chart is visible in the legend.</param>
        private void LineLook(string chart, Color color, ChartDashStyle style, bool isVisible)
        {
            chart1.Series[chart].BorderWidth = 2;
            chart1.Series[chart].Color = color;
            chart1.Series[chart].BorderDashStyle = style;
            chart1.Series[chart].IsVisibleInLegend = isVisible;

            if (chart.Contains("sd")) chart1.Series[chart].CustomProperties = "PixelPointWidth = 10"; ;
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
            int[] yMajorTicks = xMajorTicks;
            //int[] yMajorTicks = { 4, 6, 8, 10, 12, 14, 16, 18, 20 };

            if (numX)
            {
                int minX = 1;
                int maxX = 16;
                float margin = 1.1f;

                cA.AxisX.Minimum = logX ? minX / margin : minX - margin;
                cA.AxisX.Maximum = logY ? maxX * margin : maxX + margin;

                cA.AxisX2.Minimum = cA.AxisX.Minimum;
                cA.AxisX2.Maximum = cA.AxisX.Maximum;

                cA.AxisX.IsLogarithmic = logX;
                cA.AxisX2.IsLogarithmic = logX;
                cA.AxisX.LogarithmBase = 2;
                cA.AxisX2.LogarithmBase = 2;

                if (logX)
                {
                    var ticks = GetLogLabels(xMajorTicks, offset);
                    foreach (var tick in ticks.major) cA.AxisX.CustomLabels.Add(tick);
                    foreach (var tick in ticks.minor) cA.AxisX2.CustomLabels.Add(tick);
                }
            }
            if (numY)
            {
                int minY = 1;
                int maxY = 16;
                float margin = 1.1f;

                cA.AxisY.Minimum = logX ? minY / margin : minY - margin;
                cA.AxisY.Maximum = logY ? maxY * margin : maxY + margin;

                cA.AxisY2.Minimum = cA.AxisY.Minimum;
                cA.AxisY2.Maximum = cA.AxisY.Maximum;

                cA.AxisY.IsLogarithmic = logY;
                cA.AxisY2.IsLogarithmic = logY;
                cA.AxisY.LogarithmBase = 2;
                cA.AxisY2.LogarithmBase = 2;

                if (logY)
                {
                    var ticks = GetLogLabels(yMajorTicks, offset);
                    foreach (var tick in ticks.major) cA.AxisY.CustomLabels.Add(tick);
                    foreach (var tick in ticks.minor) cA.AxisY2.CustomLabels.Add(tick);
                }
            }

            cA.AxisX.LineWidth = 0;
            cA.AxisX.MajorGrid.LineColor = Color.Gray;
            cA.AxisX.Title = xTitle;

            cA.AxisX2.Enabled = AxisEnabled.True;
            cA.AxisX2.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
            cA.AxisX2.MajorGrid.LineColor = Color.LightGray;
            cA.AxisX2.LabelStyle.Enabled = false;
            cA.AxisX2.LineWidth = 0;

            cA.AxisY.LineWidth = 0;
            cA.AxisY.MajorGrid.LineColor = Color.Gray;
            cA.AxisY.Title = yTitle;

            cA.AxisY2.Enabled = AxisEnabled.True;            
            cA.AxisY2.MajorGrid.LineDashStyle = ChartDashStyle.Dot;            
            cA.AxisY2.MajorGrid.LineColor = Color.LightGray;            
            cA.AxisY2.LabelStyle.Enabled = false;            
            cA.AxisY2.LineWidth = 0;

        }

        private (List<CustomLabel> major, List<CustomLabel> minor) GetLogLabels( int[] majorTicks , float offset)
        {
            List<CustomLabel> majorCL = new List<CustomLabel>();
            List<CustomLabel> minorCL = new List<CustomLabel>();

            for (int tick = majorTicks.Min(); tick <= majorTicks.Max(); tick++)
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
