using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.Models.user_storage
{
    public interface IDBStorage
    {
        (User, bool) createUserIfNeeded(string geotag, long tg_ids, string? fn, string? ln, string? un, string bcId);
        User getUser(string geotag, long tg_id);
        void updateUserData(string geotag, long tg_id, int? first_msg_id = null, bool? is_reply = null, bool? deleted = null);
    }    
}
