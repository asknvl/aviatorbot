﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.Models.bot
{
    public interface INotifyObserver
    {
        Task Notify(Object notify);
    }
    
}
