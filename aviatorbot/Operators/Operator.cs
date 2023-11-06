using aviatorbot.ViewModels;
using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.Operators
{
    public class Operator : ViewModelBase
    {
        long _tg_id;
        [JsonProperty]
        public long tg_id {
            get => _tg_id;
            set => this.RaiseAndSetIfChanged(ref  _tg_id, value);   
        }

        string _first_name;
        [JsonProperty]
        public string first_name { 
            get => _first_name;
            set => this.RaiseAndSetIfChanged(ref _first_name, value);
        }

        string _last_name;
        [JsonProperty]
        public string last_name {
            get => _last_name;  
            set => this.RaiseAndSetIfChanged(ref _last_name, value);
        }

        string _letters;
        [JsonProperty]
        public string letters
        {
            get => _letters;
            set => this.RaiseAndSetIfChanged(ref _letters, value);
        }

        string _uniqcode;
        public string uniqcode
        {
            get => _uniqcode;
            set => this.RaiseAndSetIfChanged(ref _uniqcode, value);
        }

        List<OperatorPermission> _permissions = new();

        [JsonProperty]
        public List<OperatorPermission> permissions
        {
            get => _permissions;
            set => this.RaiseAndSetIfChanged(ref _permissions, value);
        }
    }

    public class OperatorPermission
    {
        public string name { get; set; }
        public OperatorPermissionType type { get; set; }    
    }

    public enum OperatorPermissionType
    {
        all,
        get_user_status,
        set_user_status
    }
}
