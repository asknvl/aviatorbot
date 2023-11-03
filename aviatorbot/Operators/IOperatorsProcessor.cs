using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.Operators
{
    public interface IOperatorsProcessor
    {
        Task Add(string geotag, long tg_id);
        List<Operator> GetAll(string geotag);
        
    }
    
    public class Operator
    {
        public long tg_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }   
        public string letters { get; set; }
        public List<OperatorPermissions> permissions { get; set; } = new();
    }

    public enum OperatorPermissions
    {
        set_messages,
        give_user_status,
        request_user_status
    }

}
