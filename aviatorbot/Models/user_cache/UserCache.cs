using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace aviatorbot.Models.user_cache
{
    public class UserCache : IUserCache
    {
        #region vars
        List<UserCacheData> cache = new List<UserCacheData>();
        readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        #endregion

        public async Task Add(UserCacheData userData)
        {

            await semaphore.WaitAsync();
            try
            {
                var found = cache.FirstOrDefault(d => d.tg_id == userData.tg_id);
                if (found == null)
                    cache.Add(userData);
                else
                {
                    if (!string.IsNullOrEmpty(userData.uuid))
                        found.uuid = userData.uuid;
                }
                    
            } finally
            {
                semaphore.Release();
            }
        }

        public async Task<UserCacheData?> Get(long tg_id)
        {

            await semaphore.WaitAsync();

            try
            {
                var found = cache.FirstOrDefault(d => d.tg_id == tg_id);
                if (found != null)
                {
                    return found.Copy();
                }
                else
                {
                    return null;
                }
            } finally
            {
                semaphore.Release();
            }
        }

        public async Task Remove(long tg_id)
        {
            await semaphore.WaitAsync();

            try
            {
                var found = cache.FirstOrDefault(d => d.tg_id == tg_id);
                if (found != null)
                    cache.Remove(found);    

            } finally
            {
                semaphore.Release();
            }
        }
    }
}
