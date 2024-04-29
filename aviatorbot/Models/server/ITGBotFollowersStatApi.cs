﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static asknvl.server.TGBotFollowersStatApi;

namespace asknvl.server
{
    public interface ITGBotFollowersStatApi
    {        
        Task UpdateFollowers(List<Follower> followers);

        Task<List<subscriptionDto>> GetFollowerSubscriprion(string geotag, long tg_id);
        Task<bool> IsSubscriptionAvailable(string geotag, long id);
        Task MarkFollowerWasDeclined(string geotag, long id);
        Task<(string, string)> GetFollowerState(string geotag, long id);
        Task<tgFollowerStatusResponse> GetFollowerStateResponse(string geotag, long id);
        Task SetManualFollowerStatus(string uuid, string status);
        Task SlipPush(int notification_id, bool isok);
        Task SetFollowerMadeDeposit(string uuid, long player_id, uint sum);
        Task SetFollowerRegistered(string player_id, string uuid);
        Task<List<getIdUserInfoDto>> GetUserInfoByTGid(long tg_id);
        Task<List<getIdUserInfoDto>> GetUserInfoByPlayerId(string player_id);
    }

    public enum DailyPushState
    {
        sent,
        delivered,
        disable
    }
    public class NotFoundException : Exception
    {
        public NotFoundException(string msg) : base(msg) { }
    }

    public class TGFollowersStatException : Exception
    {
        public TGFollowersStatException(string msg) : base(msg) { }
    }
}
