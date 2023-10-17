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
        Task<(string, string)> GetFollowerState(string geotag, long id);
        Task SlipPush(int notification_id, bool isok);
        Task SetFollowerMadeDeposit(string uuid, int dep_number);
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
