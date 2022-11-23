using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlotTest
{
    internal class Data
    {
        private static List<Variable> _variables;
        private static string[] _variablesOrdering;

        public List<Variable> Variables
        {
            get { return _variables; }
            private set { _variables = value; }
        }


        private List<(List<string> var, List<float> val)> _data; // Stores data as a tuple: list of the levels of the variables as Item1 and list of subjects values as Item2

        /// <summary>
        /// Parse a .csv file.
        /// </summary>
        /// <param name="path">Absolute path of the .csv file</param>
        public Data(string path, string variablesPath)
        {
            Variables = new List<Variable>();

            SortFactors(variablesPath);
            SortData(path);
        }

        private void SortFactors(string path)
        {
            var lines = File.ReadAllLines(path);
            _variablesOrdering = lines.Select(l => l.Split(';').First().Trim()).ToArray();
            foreach (string line in lines)
            {
                string[] fields = line.Split(';').Select(s => s.Trim()).ToArray();
                string name = fields[0];
                bool isNum = fields[1] != "qualitative";
                bool isLog = fields[1] == "log";
                string[] levels = fields[2].Split(',').Select(l => l.Trim()).ToArray();
                string unit = isNum ? fields[3] : string.Empty;

                if (!isNum) Variables.Add(new Variable(name, levels, isNum));
                else Variables.Add(new Variable(name, levels, isNum, isLog, unit));
            }
        }

        private void SortData(string path)
        {
            var lines = File.ReadAllLines(path);

            _data = new List<(List<string> var, List<float> val)>();

            int varCombinations = _variables.Select(v => v.Levels.Length).Aggregate((a, b) => a * b);

            for (int varComb = 0; varComb < varCombinations; varComb++)
            {
                List<string> variableLevels = _variablesOrdering
                    .Select(v => _variables.FirstOrDefault(var => var.Name == v).Levels[varComb / LevelsAfter(v) % _variables.FirstOrDefault(var => var.Name == v).Levels.Length])
                    .ToList();

                _data.Add((variableLevels, lines.Select(l => float.Parse(l.Split(',').ToArray()[varComb])).ToList()));
            }
        }

        /// <summary>
        /// Access the amount of combinations of factors that follow a given variable.
        /// </summary>
        /// <param name="variable"> Name of the variable as stored in _variablesOrdering.</param>
        /// <returns>The number of variable levels combinations for the variable following the one passed as input.</returns>
        private static int LevelsAfter(string variable)
        {
            return _variablesOrdering.Last() == variable ? 1 : _variablesOrdering.Skip(Array.IndexOf(_variablesOrdering, variable) + 1).Select(nextV => _variables.FirstOrDefault(v => v.Name == nextV).Levels.Length).Aggregate((a, b) => a * b);
        }

        /// <summary>
        /// Returns all the values matching a given set of levels.
        /// </summary>
        /// <param name="levels">The varaibles' levels at which the values are needed.</param>
        /// <returns>The raw data (a list of same size as the subjects sample) matching the specified variable levels</returns>
        private List<float> GetData(List<string> levels) => _data.Where(dat => levels.Where(l => l != null).All(v => dat.var.Contains(v))).SelectMany(d => d.val).ToList();

        /// <summary>
        /// Returns the levels of a given variable.
        /// </summary>
        /// <param name="variable">Name of the variable.</param>
        /// <returns>List of the variable's levels.</returns>
        public List<string> GetLevels(string variable) => variable == null ? new List<string>() { null } :_variables.FirstOrDefault(v => v.Name == variable).Levels.ToList();


        /// <summary>
        /// Compute the mean points along levels of a given effect or interaction.
        /// </summary>
        /// <param name="variableY">The variable which levels are plotted as separate lines (null if simple effect).</param>
        /// <param name="variableX">The variable to be plotted on the x axis.</param>
        /// <param name="logY">True is the Y-axis is logarithmic, false if it's linear.</param>
        /// <param name="restrictionLevels">Additional levels that have to be taken into account, for plotting higher-than-2-factors interactions.</param>
        /// <returns>A list of lines, which contains the coordinates of the mean points accross subjects for a given interaction.</returns>
        public List<List<(string x, float y)>> MeanLine (string variableX, bool logY, string variableY = null, List<string> restrictionLevels = null)
        {
            if (logY) return GetLevels(variableY).Select(level => GetLevels(variableX).Select(x => (x, (float)Math.Pow(10,GetData(new List<string>() { x, level }.Concat(restrictionLevels ?? new List<string>()).ToList()).Select(y => (float)Math.Log10(y)).Average()))).ToList()).ToList();
            return GetLevels(variableY).Select(level => GetLevels(variableX).Select(x => (x, GetData(new List<string>() { x, level }.Concat(restrictionLevels ?? new List<string>()).ToList()).Average())).ToList()).ToList();
        }

        /// <summary>
        /// Compute the confidence intervals along levels of a given effect or interaction.
        /// </summary>
        /// <param name="variableY">The variable which levels are plotted as separate lines (null if simple effect).</param>
        /// <param name="variableX">The variable to be plotted on the x axis.</param>
        ///  <param name="restrictionLevels">Additional levels that have to be taken into account, for plotting higher-than-2-factors interactions.</param>
        /// <returns>A list of lists of error bars, which contains the coordinates of the low and high point of the intervals.</returns>
        public List<List<(string x, (float l, float h) y)>> Std (string variableX, bool logY, string variableY = null, List<string> restrictionLevels = null)
        {
            return GetLevels(variableY).Select(level => GetLevels(variableX).Select(x => (x, ConfidenceInterval(GetData(new List<string>() { x, level }.Concat(restrictionLevels ?? new List<string>()).ToList()), logY))).ToList()).ToList();
        }

        /// <summary>
        /// Computes the 95% confidence interval of a given series.
        /// </summary>
        /// <param name="dat">Lis of floats that form the series.</param>
        /// <returns>The low and high ends of the 95% confidence interval.</returns>
        private (float l, float h) ConfidenceInterval (List<float> dat, bool logY)
        {
            if (logY) dat = dat.Select(d => (float)Math.Log10(d)).ToList();

            float mean = dat.Average();
            double sd = Math.Sqrt(dat.Select(d => Math.Pow(d - mean, 2) / dat.Count).Sum());
            float standardError = (float)(1.96 * sd/Math.Sqrt(dat.Count));

            if (logY) return ((float)Math.Pow(10,mean - standardError), (float)Math.Pow(10, mean + standardError));
            return (mean - standardError, mean + standardError);
        }
    }
}
