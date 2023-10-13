using aviatorbot.Model.bot;
using motivebot.Model.storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.Operators
{
    public class LocalOperatorProcessor : IOperatorsProcessor
    {

        #region vars
        IBotStorage botStorage;        
        #endregion

        public LocalOperatorProcessor(IBotStorage botStorage)
        {            
            this.botStorage = botStorage;
        }

        #region public
        public Task Add(string geotag, long tg_id)
        {
            throw new NotImplementedException();
        }

        public List<long> GetAll(string geotag)
        {

            var res = new List<long>();

            var found = botStorage.GetAll().FirstOrDefault(m => m.geotag.Equals(geotag));
            if (found != null)
                res = found.operators_id;

            return res;
        }
        #endregion
    }
}
