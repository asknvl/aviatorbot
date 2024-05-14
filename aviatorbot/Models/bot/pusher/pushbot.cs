using asknvl.logger;
using Avalonia.Platform;
using botservice.Model.bot;
using botservice.Models.messages;
using botservice.Models.storage;
using botservice.Operators;
using DynamicData;
using Microsoft.VisualBasic.FileIO;
using motivebot.Model.storage;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;

namespace botservice.Models.bot.pusher
{
    public class pushbot : BotBase
    {
        #region vars
        IMessageProcessorFactory messageProcessorFactory;
        CancellationTokenSource pushCts;
        #endregion

        #region properties
        public override BotType Type => BotType.pusher;

        string awaitedMessageCode;
        public string AwaitedMessageCode
        {
            get => awaitedMessageCode;
            set => this.RaiseAndSetIfChanged(ref awaitedMessageCode, value);
        }

        MessageProcessorBase messageProcessor;
        public MessageProcessorBase MessageProcessor
        {
            get => messageProcessor;
            set => this.RaiseAndSetIfChanged(ref messageProcessor, value);
        }

        string? pm;
        public string? PM
        {
            get => pm;
            set => this.RaiseAndSetIfChanged(ref pm, value);
        }

        string? channel;
        public string? Channel
        {
            get => channel;
            set => this.RaiseAndSetIfChanged(ref channel, value);
        }

        string? pushgeotag;
        public string? PushGeotag
        {
            get => pushgeotag;
            set => this.RaiseAndSetIfChanged(ref pushgeotag, value);
        }
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> pushCmd { get; set; }
        public ReactiveCommand<Unit, Unit> stopPushCmd { get; set; }
        #endregion

        public pushbot(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(model, operatorStorage, botStorage, logger)
        {
            Geotag = model.geotag;
            Token = model.token;

            #region commands
            pushCmd = ReactiveCommand.CreateFromTask(async () => {
                await push();
            });

            stopPushCmd = ReactiveCommand.Create(() => {
                pushCts?.Cancel();
            });

            #endregion
        }

        #region private
        List<string[]> readCSV(string path)
        {
            List<string[]> res = new();

            using (TextFieldParser parser = new TextFieldParser(path))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                while (!parser.EndOfData)
                {
                    string[] row = parser.ReadFields();
                    res.Add(row);
                }
            }

            return res;
        } 

        List<long> getIDs()
        {
            List<long> res = new();

            string currentDir = Directory.GetCurrentDirectory();
            string path = Path.Combine(currentDir, "ids_push");

            var files = Directory.GetFiles(path).Where(f => f.Contains($"{PushGeotag}")).ToList();

            if (files.Count > 0)
            {
                List<string[]> data = readCSV(files[0]);
                int index = data[0].IndexOf("tg_user_id");

                for (int i = 1; i < data.Count; i++)
                {
                    res.Add(long.Parse(data[i][index]));
                }
            }

            //return res;

            return new List<long> { 6336125965 };
        }

        async Task push()
        {

            if (pushCts != null)
            {
                logger.err(Geotag, "pushing is running");
                return;
            }

            pushCts = new CancellationTokenSource();

            var ids = getIDs();

            var message = MessageProcessor.GetMessage("push_message");

            try
            {
                foreach (var id in ids)
                {
                    pushCts.Token.ThrowIfCancellationRequested();

                    try
                    {

                        if (message == null) {
                            logger.err(Geotag, "push_message not set");
                            break;
                        }                           

                        await message.Send(id, bot);

                        logger.inf_urgent(Geotag, $"{id} pushed OK");

                    } catch (Exception ex)
                    {
                        logger.err(Geotag, $"push: {id} {ex.Message}");
                    }

                }
            } catch (OperationCanceledException ex)
            {
                logger.inf(PushGeotag, $"Pushing cancelled");
            } finally
            {
                pushCts = null;
            }            

        }
        #endregion

        #region override
        protected override async Task processOperator(Message message, Operator op)
        {
            try
            {
                if (state == State.waiting_new_message)
                {
                    MessageProcessor.Add(AwaitedMessageCode, message, PM, channel: Channel);
                    state = State.free;
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.err(Geotag, ex.Message);
            }
        }

        public override async Task Start()
        {
            await base.Start().ContinueWith(t =>
            {

                messageProcessorFactory = new MessageProcessorFactory(logger);

                MessageProcessor = messageProcessorFactory.Get(Type, Geotag, Token, bot);

                if (MessageProcessor != null)
                {
                    MessageProcessor.UpdateMessageRequestEvent += async (code, description) =>
                    {
                        AwaitedMessageCode = code;
                        state = State.waiting_new_message;

                        var operators = operatorStorage.GetAll(Geotag).Where(o => o.permissions.Any(p => p.type.Equals(OperatorPermissionType.all)));

                        foreach (var op in operators)
                        {
                            try
                            {
                                await bot.SendTextMessageAsync(op.tg_id, $"Перешлите сообщение для: \n{description.ToLower()}");
                            }
                            catch (Exception ex)
                            {
                                logger.err(Geotag, $"UpdateMessageRequestEvent: {ex.Message}");
                            }
                        }
                    };

                    MessageProcessor.ShowMessageRequestEvent += async (message, code) =>
                    {
                        var operators = operatorStorage.GetAll(Geotag).Where(o => o.permissions.Any(p => p.type.Equals(OperatorPermissionType.all)));

                        foreach (var op in operators)
                        {
                            try
                            {
                                int id = await message.Send(op.tg_id, bot);
                            }
                            catch (Exception ex)
                            {
                                logger.err(Geotag, $"ShowMessageRequestEvent: {ex.Message}");
                            }
                        }
                    };

                    MessageProcessor.Init();
                }

            });
        }
        #region empty
        public override Task Notify(object notifyObject)
        {
            return Task.CompletedTask;
        }

        protected override Task processCallbackQuery(CallbackQuery query)
        {
            return Task.CompletedTask;
        }

        protected override Task processChatJoinRequest(ChatJoinRequest chatJoinRequest, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected override Task processChatMember(Update update, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected override Task processFollower(Message message)
        {
            return Task.CompletedTask;
        }

        protected override Task processSubscribe(Update update)
        {
            return Task.CompletedTask;
        }
        #endregion

        #endregion
    }
}
