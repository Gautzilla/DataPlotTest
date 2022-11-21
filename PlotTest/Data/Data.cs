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
        /// Returns all the values matching a given set of levels.
        /// </summary>
        /// <param name="levels">The varaibles' levels at which the values are needed.</param>
        /// <returns>The raw data (a list of same size as the subjects sample) matching the specified variable levels</returns>
        private List<float> GetData(List<string> levels) => _data.Where(dat => levels.All(v => dat.var.Contains(v))).SelectMany(d => d.val).ToList();

        /// <summary>
        /// Outputs the levels of a given variable.
        /// </summary>
        /// <param name="variable">Name of the variable.</param>
        /// <returns>List of the variable's levels.</returns>
        public List<string> GetLevels(string variable) => _variables[variable].ToList();

        /// <summary>
        /// Outputs the coordinates of the mean points accross subjects for a given variable (Simple effect).
        /// </summary>
        /// <param name="variable">Variable from which to plot.</param>
        /// <returns></returns>
        public List<(string x, float y)> SimpleEffectMeanLine (string variable)
        {
            return GetLevels(variable).Select(x => (x,GetData(new List<string>() { x }).Average())).ToList();
        }

        public List<(string x, (float l, float h) y)> SimpleEffectStd (string variable)
        {
            return GetLevels(variable).Select(x => (x, ConfidenceInterval(GetData(new List<string>() { x })))).ToList();
        }

        /// <summary>
        /// Outputs a list of lines, which contains the coordinates of the maen points accross subjects for a given interaction.
        /// </summary>
        /// <param name="variableY">The variable which levels are plotted as separate lines.</param>
        /// <param name="variableX">the variable to be plotted on the x axis.</param>
        /// <returns></returns>
        public List<List<(string x, float y)>> InteractionMeanLine (string variableY, string variableX)
        {
            return GetLevels(variableY).Select(level => GetLevels(variableX).Select(x => (x, GetData(new List<string>() { x, level }).Average())).ToList()).ToList();
        }

        private (float l, float h) ConfidenceInterval (List<float> dat)
        {
            float mean = dat.Average();
            float standardError = (float)(1.96 * Math.Sqrt(dat.Select(d => Math.Pow(d - mean, 2)).Sum() / dat.Count));
            return (mean - standardError, mean + standardError);
        }
    }
}
