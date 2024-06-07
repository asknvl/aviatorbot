using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.Models.user_storage
{
    public class User
    {
        [Key]
        public int id { get; set; }
        public string geotag { get; set; }
        public long tg_id { get; set; }
        public string? bcId { get; set; }
        public string? fn { get; set; }
        public string? ln { get; set; }
        public string? un { get; set; }
        public DateTime created_date { get; set; }
        public int? first_msg_id { get; set; }
        public DateTime first_msg_rcvd_date { get; set; }        
        public bool is_first_msg_rep { get; set; }
        public DateTime first_msg_rep_date { get; set; }
        public bool is_chat_deleted { get; set; }
        public DateTime chat_delete_date { get; set; }
        public bool was_autoreply { get; set; }
        public DateTime autoreply_date { get; set; }

        public User(string geotag, long tg_id, string bcId, string? fn = null, string? ln = null, string? un = null)
        {
            this.geotag = geotag;
            this.tg_id = tg_id;
            this.bcId = bcId;

            created_date = DateTime.UtcNow;
            first_msg_id = null;
            is_first_msg_rep = false;
            is_chat_deleted = false;

            this.fn = fn;
            this.ln = ln;
            this.un = un;
            
        }
    }       
}
