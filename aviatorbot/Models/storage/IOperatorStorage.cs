using aviatorbot.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.Models.storage
{
    public interface IOperatorStorage
    {
        void Add(string geotag);
        void Add(string geotag, Operator op);
        List<Operator> GetAll(string geotag);
        List<BotOperators> GetAll();

        public event Action UpdatedEvent;
    }
}
