using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSON
{
    class JSON
    {
        string jsonString = "";

        public void Add(string name, string value)
        {

        }
        public void Add(string name, int value)
        {

        }
        public void Add(string name, float value)
        {

        }
        public void Add(string name, double value)
        {

        }
        public void Add(string name, bool toggle)
        {

        }
        public void Add(string name, Array array)
        {
            
        }

        public void AddInto(string target, string name, string value)
        {

        }
        public void AddInto(string target, string name, int value)
        {

        }
        public void AddInto(string target, string name, float value)
        {

        }
        public void AddInto(string target, string name, double value)
        {

        }
        public void AddInto(string target, string name, bool toggle)
        {

        }
        public void AddInto(string target, string name, Array array)
        {

        }

        public void ClearJSONString()
        {
            jsonString = "";
        }

        public string GetJSONString() => jsonString;
        public string SetJSONString(string value) => jsonString = value;
    }
}
