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

            Data testData = new Data(@"C:\Users\User\Desktop\Distance.csv");

            //PlotSimpleEffect(testData, "Salle");

            PlotInteraction(testData, "Salle", "Distance");
        }

        private void PlotSimpleEffect(Data data, string variable)
        {
            // MEAN
            chart1.Series.Add($"{variable} mean");
            chart1.Series[$"{variable} mean"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            Appearance($"{variable} mean", Color.Black, ChartDashStyle.Solid);
            foreach (var point in data.SimpleEffectMeanLine(variable))
            {
                chart1.Series[$"{variable} mean"].Points.AddXY(point.x, point.y);
            }

            // CONFIDENCE INTERVAL
            chart1.Series.Add($"{variable} sd");
            chart1.Series[$"{variable} sd"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.ErrorBar;
            Appearance($"{variable} sd", Color.Black, ChartDashStyle.Solid);
            foreach (var point in data.SimpleEffectStd(variable))
            {
                chart1.Series[$"{variable} sd"].Points.AddXY(point.x, 0, point.y.l, point.y.h);
            }
        }

        private void PlotInteraction(Data data, string variableY, string variableX)
        {
            for (int i = 0; i < data.GetLevels(variableY).Count; i++)
            {
                string lineName = data.GetLevels(variableY)[i];

                // MEAN
                var meanLine = data.InteractionMeanLine(variableY, variableX);
                chart1.Series.Add($"{lineName} mean");
                chart1.Series[$"{lineName} mean"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                Appearance($"{lineName} mean", Color.Black, _styles[i]);
                foreach (var point in meanLine[i])
                {
                    chart1.Series[$"{lineName} mean"].Points.AddXY(point.x, point.y);
                }

                // CONFIDENCE INTERVAL
                var sdLine = data.InteractionStd(variableY, variableX);
                chart1.Series.Add($"{lineName} sd");
                chart1.Series[$"{lineName} sd"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.ErrorBar;
                Appearance($"{lineName} sd", Color.Black, ChartDashStyle.Solid);
                foreach (var point in sdLine[i])
                {
                    chart1.Series[$"{lineName} sd"].Points.AddXY(point.x, 0, point.y.l, point.y.h);
                }
            }
        }

        private void Appearance(string chart, Color color, ChartDashStyle style)
        {
            chart1.Series[chart].BorderWidth = 2;
            chart1.Series[chart].Color = color;
            chart1.Series[chart].BorderDashStyle = style;
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }
    }
}
