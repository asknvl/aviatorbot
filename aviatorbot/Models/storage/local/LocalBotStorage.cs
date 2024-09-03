using aksnvl.storage;
using asknvl.storage;
using botservice.Model.bot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace motivebot.Model.storage.local
{
    public class LocalBotStorage : IBotStorage 
    {
        #region vars
        IStorage<List<BotModel>> storage; 
        #endregion
        #region properties
        List<BotModel> BotModels { get; set; } = new List<BotModel>();
        #endregion
        public LocalBotStorage() {
            var subdir = Path.Combine("bots");
            storage = new Storage<List<BotModel>>("bots.json", subdir, BotModels);
            Load();
        }

        #region public
        public void Add(BotModel bot)
        {
            var found = BotModels.Any(m => m.geotag.Equals(bot.geotag));
            if (!found)
                BotModels.Add(bot);
            else
                throw new BotStorageException($"Бот с геотегом {bot.geotag} уже существует");

            storage.save(BotModels);
        }

        public void Remove(string geotag)
        {
            var found = BotModels.FirstOrDefault(m => m.geotag.Equals(geotag));
            if (found != null)
                BotModels.Remove(found);

            storage.save(BotModels);
        }

        public List<BotModel> GetAll()
        {
            Load();
            return BotModels;
        }

        public void Load()
        {
            try
            {
                BotModels = storage.load();

            } catch (Exception ex)
            {
                throw new BotStorageException("Не удалось загрузить данные");
            }
        }

        public void Save()
        {
            try
            {
                storage.save(BotModels);

            } catch (Exception ex)
            {
                throw new BotStorageException("Не удалось сохранить данные");
            }
        }

        public void Update(BotModel bot)
        {
            try
            {
                var found = BotModels.FirstOrDefault(m => m.geotag.Equals(bot.geotag));
                if (found != null)
                {
                    found.token = bot.token;

                    //found.operators_id = bot.operators_id;

                    found.operators = bot.operators;

                    found.link= bot.link;
                    found.pm = bot.pm;
                    found.channel = bot.channel;                    
                    found.channel_approve = bot.channel_approve;
                    found.postbacks = bot.postbacks;

                    found.help = bot.help;  
                    found.training = bot.training;
                    found.reveiews = bot.reveiews;
                    found.strategy = bot.strategy;
                    found.vip = bot.vip;

                    found.register_source = bot.register_source;
                    found.register_source_link = bot.register_source_link;
                    
                    if (bot.group_manager_settings != null)
                        found.group_manager_settings = bot.group_manager_settings;

                    storage.save(BotModels);
                }
            } catch (Exception ex)
            {
                throw new BotStorageException("Не удалось обновить данные");
            }
        }       
        #endregion
    }
}
