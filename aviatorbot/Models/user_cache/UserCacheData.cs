using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.Models.user_cache
{
    public class UserCacheData
    {
        public long tg_id { get; set; }
        public string uuid { get; set; }

        public UserCacheData Copy()
        {
            return new UserCacheData()
            {
                tg_id = tg_id,
                uuid = uuid
            };
        }
    }
}
