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
        private static readonly Dictionary<string, string[]> _variables = new Dictionary<string, string[]>();
        private static string[] _variablesOrdering;

        private List<(List<string> var, List<float> val)> _data; // Stores data as a tuple: list of the levels of the variables as Item1 and list of subjects values as Item2

        /// <summary>
        /// Parse a .csv file.
        /// </summary>
        /// <param name="path">Absolute path of the .csv file</param>
        public Data(string path, string variablesPath)
        {
            SortFactors(variablesPath);
            SortData(path);
        }

        private void SortFactors(string path)
        {
            var lines = File.ReadAllLines(path);
            _variablesOrdering = lines.Select(l => l.Split(':').First().Trim()).ToArray();
            foreach (string line in lines)
            {
                _variables.Add(line.Split(':').First().Trim(), line.Split(':').Last().Split(',').Select(f => f.Trim()).ToArray());
            }
        }

        private void SortData(string path)
        {
            var lines = File.ReadAllLines(path);

            _data = new List<(List<string> var, List<float> val)>();

            int varCombinations = _variables.Select(v => v.Value.Length).Aggregate((a, b) => a * b);

            for (int varComb = 0; varComb < varCombinations; varComb++)
            {
                List<string> variableLevels = _variablesOrdering
                    .Select(v => _variables[v][(varComb / LevelsAfter(v)) % _variables[v].Length])
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
            return _variablesOrdering.Last() == variable ? 1 : _variablesOrdering.Skip(Array.IndexOf(_variablesOrdering, variable) + 1).Select(nextV => _variables[nextV].Length).Aggregate((a, b) => a * b);
        }

        /// <summary>
        /// Returns all the values matching a given set of levels.
        /// </summary>
        /// <param name="levels">The varaibles' levels at which the values are needed.</param>
        /// <returns>The raw data (a list of same size as the subjects sample) matching the specified variable levels</returns>
        private List<float> GetData(List<string> levels) => _data.Where(dat => levels.All(v => dat.var.Contains(v))).SelectMany(d => d.val).ToList();

        /// <summary>
        /// Returns the levels of a given variable.
        /// </summary>
        /// <param name="variable">Name of the variable.</param>
        /// <returns>List of the variable's levels.</returns>
        public List<string> GetLevels(string variable) => _variables[variable].ToList();

        /// <summary>
        /// Compute the mean points along levels of the given variable.
        /// </summary>
        /// <param name="variable">Variable from which the single effect is plotted.</param>
        /// <param name="logY">True is the Y-axis is logarithmic, false if it's linear.</param>
        /// <returns>The coordinates of the mean points accross subjects for a given variable (Simple effect)</returns>
        public List<(string x, float y)> SimpleEffectMeanLine (string variable, bool logY)
        {
            if (logY) return GetLevels(variable).Select(x => (x, (float) Math.Pow(10,GetData(new List<string>() { x }).Select(y => Math.Log10(y)).Average()))).ToList();
            return GetLevels(variable).Select(x => (x,GetData(new List<string>() { x }).Average())).ToList();
        }

        /// <summary>
        /// Compute the confidence intervals along levels of the given variable.
        /// </summary>
        /// <param name="variable">Variable from which the single effect is plotted.</param>
        /// <returns>A list of coordinates for the confidence intervals corresponding to a simple effect.</returns>
        public List<(string x, (float l, float h) y)> SimpleEffectStd (string variable, bool logY)
        { 
            return GetLevels(variable).Select(x => (x, ConfidenceInterval(GetData(new List<string>() { x }), logY))).ToList();
        }

        /// <summary>
        /// Compute the mean points along levels of the given variable for a given 2-factors interaction.
        /// </summary>
        /// <param name="variableY">The variable which levels are plotted as separate lines.</param>
        /// <param name="variableX">The variable to be plotted on the x axis.</param>
        /// <param name="logY">True is the Y-axis is logarithmic, false if it's linear.</param>
        /// <returns>A list of lines, which contains the coordinates of the mean points accross subjects for a given interaction.</returns>
        public List<List<(string x, float y)>> InteractionMeanLine (string variableY, string variableX, bool logY)
        {
            if (logY) return GetLevels(variableY).Select(level => GetLevels(variableX).Select(x => (x, (float)Math.Pow(10,GetData(new List<string>() { x, level }).Select(y => (float)Math.Log10(y)).Average()))).ToList()).ToList();
            return GetLevels(variableY).Select(level => GetLevels(variableX).Select(x => (x, GetData(new List<string>() { x, level }).Average())).ToList()).ToList();
        }

        /// <summary>
        /// Compute the confidence intervals along levels of the given variable for a given 2-factors interaction.
        /// </summary>
        /// <param name="variableY">The variable which levels are plotted as separate lines.</param>
        /// <param name="variableX">The variable to be plotted on the x axis.</param>
        /// <returns>A list of lists of error bars, which contains the coordinates of the low and high point of the intervals.</returns>
        public List<List<(string x, (float l, float h) y)>> InteractionStd (string variableY, string variableX, bool logY)
        {
            return GetLevels(variableY).Select(level => GetLevels(variableX).Select(x => (x, ConfidenceInterval(GetData(new List<string>() { x, level }), logY))).ToList()).ToList();
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

            Debug.WriteLine($"Mean : {mean}");
            Debug.WriteLine($"Standard Deviation : {Math.Sqrt(dat.Select(d => Math.Pow(d-mean,2)/dat.Count).Sum())}");
            Debug.WriteLine($"Standard Error : {standardError}");
            Debug.WriteLine($"Confidence Interval : ({mean - standardError},{mean + standardError})");

            if (logY) return ((float)Math.Pow(10,mean - standardError), (float)Math.Pow(10, mean + standardError));
            return (mean - standardError, mean + standardError);
        }
    }
}
