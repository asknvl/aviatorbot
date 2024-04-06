using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.Models.bot
{
    public class errorCollector
    {
        List<string> errors;
        private readonly object lockObject = new object();

        public void Add(string error)
        {
            lock (lockObject)
            {
                errors.Add(error);
            }
        }

        public string[] Get()
        {
            string[] res;

            lock (lockObject)
            {
                res = errors.ToArray();
                errors.Clear();
            }
            return res;
        }
    }
}
