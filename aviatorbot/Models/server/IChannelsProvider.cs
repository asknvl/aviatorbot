using asknvl.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asknvl.server
{
    public interface IChannelsProvider
    {
        Task<List<ChannelModel>> GetChannels();
    }
}
