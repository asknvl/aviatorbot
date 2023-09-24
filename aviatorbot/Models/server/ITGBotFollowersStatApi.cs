using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asknvl.server
{
    public interface ITGBotFollowersStatApi
    {        
        Task UpdateFollowers(List<Follower> followers);
        Task<(string, string)> GetFollowerState(long id);
    }

    public enum DailyPushState
    {
        sent,
        delivered,
        disable
    }

    public class TGFollowersStatException : Exception
    {
        public TGFollowersStatException(string msg) : base(msg) { }
    }
}
