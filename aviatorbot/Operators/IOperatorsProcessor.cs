using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.Operators
{
    public interface IOperatorsProcessor
    {
        Task Add(string geotag, long tg_id);
        List<long> GetAll(string geotag);
        
    }
}
