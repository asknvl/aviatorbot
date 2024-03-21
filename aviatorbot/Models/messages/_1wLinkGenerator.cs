using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botservice.Models.messages
{
    public class _1wLinkGenerator
    {
        public static string getRegUrl(string link, string uuid)
        {
            return $"{link}/casino/play/aviator/list?open=register&sub1={uuid}";
        }

        public static string getFDUrl(string link, string uuid)
        {
            return $"{link}/casino/play/aviator/list?open=deposit&sub1={uuid}";
        }

        public static string getGameUrl(string link)
        {
            return $"{link}/casino/play/aviator/list?open=deposit";
        }

        public static string getFriendUrl(string link)
        {
            return $"{link}/casino/play/aviator/list?open=register&sub1=friend";
        }
    }
}
