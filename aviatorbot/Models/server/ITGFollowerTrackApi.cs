﻿using Newtonsoft.Json;
using System.Threading.Tasks;

namespace csb.server
{
    public interface ITGFollowerTrackApi
    {
        Task EnqueueInviteLink(string geotag, string link);        
        Task<int> GetInviteLinksAvailable(string geotag);
    }   
}