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

            List<string> dataComb = new List<string>() { "Suaps", "2m" };
            Debug.WriteLine(string.Join("\n", testData.GetData(dataComb)));
        }
    }
}
