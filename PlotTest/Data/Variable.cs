using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlotTest
{
    internal class Variable
    {
        private string _name;
        private string[] _levels;
        private bool _isNum;
        private bool _isLog;
        private string _unit;

        public string Name
        {
            get { return _name; }
            private set { _name = value; }
        }

        public string[] Levels
        {
            get { return _levels; }
            private set { _levels = value; }
        }

        public bool IsNum
        {
            get { return _isNum; }
            private set { _isNum = value; }
        }

        public bool IsLog
        {
            get { return _isLog; }
            private set { _isLog = value; }
        }

        public string Unit
        {
            get { return _unit; }
            private set { _unit = value; }
        }


        public Variable (string name, string[] levels, bool isNum)
        {
            Name = name;
            Levels = levels;
            IsNum = isNum;
            IsLog = false;
            Unit = string.Empty;
        }

        public Variable(string name, string[] levels, bool isNum, bool isLog, string unit)
        {
            Name = name;
            Levels = levels;
            IsNum = isNum;
            IsLog = isLog;
            Unit = unit;
        }
    }
}
