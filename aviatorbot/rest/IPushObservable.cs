﻿using botservice.Models.bot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace botservice.rest
{
    public interface IPushObservable
    {
        void Add(IPushObserver observer);
        void Remove(IPushObserver observer);        
    }
}
