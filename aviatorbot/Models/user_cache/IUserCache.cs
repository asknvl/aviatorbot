using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.Models.user_cache
{
    public interface IUserCache
    {
        Task Add(UserCacheData userData);
        Task<UserCacheData?> Get(long tg_id);
        Task Remove(long tg_id);
    }
}
