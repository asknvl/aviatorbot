using asknvl.logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace aviatorbot.Models.bot.group_moderator.message_aggregator
{
    public class MessageAgregator
    {
        #region const
        const int ag_time = 1 * 60 * 1000;
        #endregion

        #region vars
        List<AgregationItem> aggregationItems = new List<AgregationItem>();
        object lockObj = new();
        CancellationTokenSource cts;
        ILogger logger;
        string tag;
        #endregion

        public MessageAgregator(string geotag, ILogger logger)
        {
            tag = $"{geotag} AG";
            this.logger = logger;               
        }

        #region public
        public void Add(long tg_id, string text)
        {
            lock (lockObj)
            {
                var found = aggregationItems.FirstOrDefault(i => i.tg_id == tg_id);
                if (found != null)
                {
                    found.text += text;
                }
                else
                {
                    aggregationItems.Add(new AgregationItem()
                    {
                        tg_id = tg_id,
                        text = text,
                        start_time = DateTime.Now
                    });
                }
            }
        }        

        public Task Start()
        {

            cts = new CancellationTokenSource();

            return Task.Run(async () => {

                try
                {

                    while (true)
                    {

                        cts.Token.ThrowIfCancellationRequested();

                        lock (lockObj)
                        {
                            var aggregated = aggregationItems.Where(i => (DateTime.Now - i.start_time).TotalMilliseconds >= ag_time).ToList();

                            foreach (var item in aggregated)
                            {
                                //aggregationItems.Remove(item);
                                var found = aggregationItems.FirstOrDefault(i => i.tg_id ==  item.tg_id);
                                if (found != null)
                                    aggregationItems.Remove(found);

                            }

                            AggregatedEvent?.Invoke(aggregated);

                        }

                        logger.dbg(tag, $"list length={aggregationItems.Count}");

                        await Task.Delay(5000);
                    }
                } catch (OperationCanceledException ex) { 
                };
            });
        }

        public void Stop()
        {
            cts?.Cancel();
        }
        #endregion

        public event Action<List<AgregationItem>> AggregatedEvent;
    }

    public class AgregationItem
    {
        public long tg_id { get; set; }
        public string text { get; set; }
        public DateTime start_time { get; set; }
    }
}
