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
        private static readonly Dictionary<string, string[]> _variables = new Dictionary<string, string[]>()
        {
            {"Salle", new string[] {"Anechoique", "Suaps"} },
            { "Distance", new string[] { "1m", "2m", "4m", "8m" } }
        };

        private static readonly string[] _variablesOrdering = new string[] { "Salle", "Distance" };

        private List<(List<string> var, List<float> val)> _data; // Stores data as a tuple: list of the levels of the variables as Item1 and list of subjects values as Item2

        /// <summary>
        /// Parse a .csv file.
        /// </summary>
        /// <param name="path">Absolute path of the .csv file</param>
        public Data(string path)
        {
            var lines = File.ReadAllLines(path);

            _data = new List<(List<string> var, List<float> val)>();

            int varCombinations = _variables.Select(v => v.Value.Length).Aggregate((a, b) => a * b);

            for (int varComb = 0; varComb < varCombinations; varComb++)
            {
                List<string> variableLevels = _variablesOrdering
                    .Select(v => _variables[v][ (varComb / LevelsAfter(v)) % _variables[v].Length ])
                    .ToList();

                _data.Add((variableLevels, lines.Select(l => float.Parse(l.Split(',').ToArray()[varComb])).ToList()));
            }
        }

        /// <summary>
        /// Outputs the number of variable levels combinations for the variable following the one passed as input.
        /// </summary>
        /// <param name="variable"> Name of the variable as stored in _variablesOrdering.</param>
        /// <returns></returns>
        private static int LevelsAfter(string variable)
        {
            return _variablesOrdering.Last() == variable ? 1 : _variablesOrdering.Skip(Array.IndexOf(_variablesOrdering, variable) + 1).Select(nextV => _variables[nextV].Length).Aggregate((a, b) => a * b);
        }

        /// <summary>
        /// Debug method for checking if the data storing is efficient.
        /// </summary>
        /// <param name="variables">A list of strings representing the variables' required levels</param>
        /// <returns>The raw data (a list of same size as the subjects sample) matching the specified variable levels</returns>
        public List<float> GetData(List<string> variables) => _data.Where(dat => variables.All(v => dat.var.Contains(v))).SelectMany(d => d.val).ToList();

        /// <summary>
        /// Outputs the levels of a given variable.
        /// </summary>
        /// <param name="variable">Name of the variable.</param>
        /// <returns>List of the variable's levels.</returns>
        public List<string> GetLevels(string variable) => _variables[variable].ToList();

        /// <summary>
        /// Outputs the coordinates of the mean points accross subjects for a given level of a given variable (y-axis) on a given variable x-axis.
        /// </summary>
        /// <param name="variable">Name of the variable that is plotted on the line.</param>
        /// <param name="level">Name of the level to plot on the line.</param>
        /// <param name="xAxis">Name of the x-Axis variable.</param>
        /// <returns></returns>
        public List<(string x, float y)> GetMeanLine (string variable, string level, string xAxis)
        {
            return new List<(string x, float y)>();
        }

        private static float Mean(List<float> points) => points.Average();
    }
}
