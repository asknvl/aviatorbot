using System.Collections.Generic;

namespace aviatorbot.Models.bot
{
    public class errorCollector
    {
        List<string> errors = new();
        private readonly object lockObject = new object();

        public void Add(string error)
        {
            lock (lockObject)
            {
                if (!errors.Contains(error))
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
        public void Clear()
        {
            lock (lockObject)
            {
                errors.Clear();
            }
        }
    }
}
