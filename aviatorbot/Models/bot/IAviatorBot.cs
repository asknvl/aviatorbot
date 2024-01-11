using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.Model.bot
{
    public interface IAviatorBot
    { 
        string Geotag { get; set; }
        string Name { get; set; }
        string Token { get; set; }
        ObservableCollection<long> Operators { get; }
        bool IsActive { get; set; }
    }

    public enum State
    {
        free,
        waiting_new_message,
        waiting_bot_selection_reg,
        waiting_player_id_input,
        waiting_fd_access,
        waiting_vip_access,
        waiting_check_status_by_tg_id,
        waiting_check_status_by_player_id

    }
}
