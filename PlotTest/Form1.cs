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

namespace PlotTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            //chart1.Series["s1"].Points.AddXY(0,0,0,0);

            Data testData = new Data(@"C:\Users\User\Desktop\Distance.csv");

            PlotSimpleEffect(testData, "Distance");

            PlotInteraction(testData, "Salle", "Distance");
        }

        private void PlotSimpleEffect(Data data, string variable)
        {
            // MEAN
            chart1.Series.Add($"{variable} mean");
            chart1.Series[$"{variable} mean"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            foreach (var point in data.SimpleEffectMeanLine(variable))
            {
                chart1.Series[$"{variable} mean"].Points.AddXY(point.x, point.y);
            }

            // CONFIDENCE INTERVAL
            chart1.Series.Add($"{variable} sd");
            chart1.Series[$"{variable} sd"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.ErrorBar;
            foreach (var point in data.SimpleEffectStd(variable))
            {
                chart1.Series[$"{variable} sd"].Points.AddXY(point.x, 0, point.y.l, point.y.h);
            }
        }

        private void PlotInteraction(Data data, string variableY, string variableX)
        {
            int i = 0;
            foreach (var line in data.InteractionMeanLine(variableY, variableX))
            {
                string lineName = data.GetLevels(variableY)[i];
                chart1.Series.Add(lineName);
                chart1.Series[lineName].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                i++;
                foreach (var point in line)
                {
                    chart1.Series[lineName].Points.AddXY(point.x, point.y);
                }
            }
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }
    }
}
