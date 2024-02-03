using asknvl.logger;
using asknvl.server;
using Avalonia.Controls;
using Avalonia.X11;
using aviatorbot.Model.bot;
using aviatorbot.Models.storage;
using aviatorbot.Operators;
using aviatorbot.rest;
using motivebot.Model.storage;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace aviatorbot.Models.bot
{
    public class infoBot : AviatorBotBase
    {
        #region vars
        InlineKeyboardMarkup inlineKeyboard = new(new[]
        {
            // first row
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "1.1", callbackData: "11"),
                InlineKeyboardButton.WithCallbackData(text: "1.2", callbackData: "12"),
            },
            // second row
            new []
            {
                InlineKeyboardButton.WithCallbackData(text: "2.1", callbackData: "21"),
                InlineKeyboardButton.WithCallbackData(text: "2.2", callbackData: "22"),
            },
        });
        #endregion

        #region properties
        public override BotType Type => BotType.getinfo_v0;
        #endregion
        public infoBot(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(operatorStorage, botStorage, logger)
        {
            Geotag = "INFO";
            Token = model.token;
            Link = null;
            PM = null;
            Channel = null;
            Postbacks = false;
        }

        #region public
        protected override async Task sendOperatorTextMessage(Operator op, long chat, string text)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = null;
            if (op.permissions.Any(p => p.type.Equals(OperatorPermissionType.set_user_status)) || op.permissions.Any(p => p.type.Equals(OperatorPermissionType.all)))
            {

                replyKeyboardMarkup = new(new[]
                        {
                            new KeyboardButton[] { $"INFO BY TG ID" },
                            new KeyboardButton[] { $"INFO BY PLAYER ID" },
                            new KeyboardButton[] { $"GIVE REG" },
                            new KeyboardButton[] { $"GIVE FD" },
                            //new KeyboardButton[] { $"GIVE VIP" },
                            //new KeyboardButton[] { $"CHECK STATUS" }
                        })
                {
                    ResizeKeyboard = true,
                    OneTimeKeyboard = false,
                };
            }
            else
            if (op.permissions.Any(p => p.type.Equals(OperatorPermissionType.get_user_status)))
            {
                replyKeyboardMarkup = new(new[]
                        {
                            new KeyboardButton[] { $"INFO BY TG ID" },
                            new KeyboardButton[] { $"INFO BY PLAYER ID" }
                        })
                {
                    ResizeKeyboard = true,
                    OneTimeKeyboard = false,
                };
            }


            await bot.SendTextMessageAsync(
                chat,
                text: text,
                replyMarkup: replyKeyboardMarkup
                /*parseMode: ParseMode.MarkdownV2*/);
        }

        async Task sendOperatorBotSelectMessage(Operator op, long chat, string[] bots)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = null;

            KeyboardButton[][] bots_buttons = new KeyboardButton[bots.Length][];
            for (int i = 0; i < bots.Length; i++)
            {
                bots_buttons[i] = new KeyboardButton[] { bots[i] };
            }


            replyKeyboardMarkup = new ReplyKeyboardMarkup(bots_buttons);
            replyKeyboardMarkup.ResizeKeyboard = true;

            await bot.SendTextMessageAsync(
                chat,
                text: "Выберите бота, на которого подписан лид:",
                replyMarkup: replyKeyboardMarkup,
                parseMode: ParseMode.MarkdownV2);
        }

        protected override async Task processOperator(Message message, Operator op)
        {
            var chat = message.From.Id;

            try
            {
                if (message.Text != null)
                {
                    if (message.Text.Equals("/start"))
                    {
                        await sendOperatorTextMessage(op, chat, $"{op.first_name} {op.last_name}, вы вошли как оператор");
                        op.state = State.free;
                        op.ClearCash();
                        return;
                    }

                    if (message.Text.Equals("INFO BY TG ID"))
                    {
                        await bot.SendTextMessageAsync(message.From.Id, "Введите TG ID для определения статуса:");
                        op.state = State.waiting_check_status_by_tg_id;
                        return;
                    }

                    if (message.Text.Equals("INFO BY PLAYER ID"))
                    {
                        await bot.SendTextMessageAsync(message.From.Id, "Введите PLAYER ID для определения статуса:");
                        op.state = State.waiting_check_status_by_player_id;
                        return;
                    }

                    if (message.Text.Equals("GIVE REG"))
                    {
                        await bot.SendTextMessageAsync(message.From.Id, "Введите TG id для установки статуса РЕГИСТРАЦИЯ:");
                        op.state = State.waiting_bot_selection_reg;
                        return;
                    }

                    if (message.Text.Equals("GIVE FD"))
                    {
                        await bot.SendTextMessageAsync(message.From.Id, "Введите TG id для установки статуса ФД:");
                        op.state = State.waiting_bot_selection_fd;
                        return;
                    }

                    if (message.Text.Contains("BOT"))
                    {
                        try
                        {
                            switch (op.state)
                            {
                                case State.waiting_bot_selection_reg:
                                    await bot.SendTextMessageAsync(message.From.Id, $"Введите Player ID для установки статуса РЕГИСТРАЦИЯ игроку {op.GetParamFromCash(ParamType.TGID)}:");
                                    op.state = State.waiting_player_id_reg_input;
                                    break;
                                case State.waiting_bot_selection_fd:
                                    await bot.SendTextMessageAsync(message.From.Id, $"Введите Player ID для установки статуса ФД игроку {op.GetParamFromCash(ParamType.TGID)}:");
                                    op.state = State.waiting_player_id_fd_input;
                                    break;
                            }

                            op.PutIntoCash(ParamType.GEO, message.Text);

                        }
                        catch (Exception ex)
                        {
                            op.ClearCash();
                            await sendOperatorTextMessage(op, chat, $"{ex.Message}");
                        }
                        return;
                    }
                }

                switch (op.state)
                {

                    case State.waiting_check_status_by_tg_id:
                        try
                        {
                            long tg_id = long.Parse(message.Text);

                            var resp = await server.GetUserInfoByTGid(tg_id);

                            foreach (var item in resp)
                            {
                                try
                                {
                                    await bot.SendTextMessageAsync(message.From.Id, item.ToString());

                                }
                                catch (Exception ex)
                                {

                                }
                            }

                        }
                        catch (NotFoundException ex)
                        {
                            await sendOperatorTextMessage(op, chat, $"{ex.Message}");
                        }
                        catch (Exception ex)
                        {
                            await sendOperatorTextMessage(op, chat, $"{ex.Message}");
                        } finally
                        {
                            op.state = State.free;
                        }
                        break;

                    case State.waiting_check_status_by_player_id:
                        try
                        {
                            string player_id = message.Text;

                            var resp = await server.GetUserInfoByPlayerId(player_id);

                            foreach (var item in resp)
                            {
                                try
                                {
                                    await bot.SendTextMessageAsync(message.From.Id, item.ToString());

                                }
                                catch (Exception ex)
                                {

                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            await sendOperatorTextMessage(op, chat, $"{ex.Message}");
                        } finally
                        {
                            op.state = State.free;
                        }
                        break;

                    case State.waiting_bot_selection_reg:
                    case State.waiting_bot_selection_fd:
                        try
                        {
                            long tg_id = long.Parse(message.Text);

                            string msg;

                            var resp = await server.GetUserInfoByTGid(tg_id);
                            var uinfo = resp.Where(o => !string.IsNullOrEmpty(o.uuid)).ToArray();
                            if (uinfo != null)
                            {
                                var bots = uinfo.Select(r => r.geo).ToArray();
                                if (bots != null)
                                {
                                    op.PutIntoCash(ParamType.TGID, $"{tg_id}");
                                    await sendOperatorBotSelectMessage(op, chat, bots);
                                }
                            }
                            else
                            {
                                msg = $"Пользователь {tg_id} не подписан ни на одного из ботов";
                                await sendOperatorTextMessage(op, chat, msg);
                                op.ClearCash();
                                op.state = State.free;
                            }
                        }
                        catch (Exception ex)
                        {
                            await sendOperatorTextMessage(op, chat, $"{ex.Message}");
                        }
                        break;

                    case State.waiting_player_id_reg_input:
                        try
                        {
                            string uuid;
                            string status;
                            long player_id;

                            long lead_tg = long.Parse(op.GetParamFromCash(ParamType.TGID));
                            string geotag = op.GetParamFromCash(ParamType.GEO);
                            (uuid, status) = await server.GetFollowerState(geotag, lead_tg);
                            try
                            {
                                player_id = long.Parse(message.Text);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("Неверный формат Player ID");
                            }
                            await server.SetFollowerRegistered($"{player_id}", uuid);
                            await sendOperatorTextMessage(op, chat, $"Пользователю зарегестрирован");

                        }
                        catch (Exception ex)
                        {
                            await sendOperatorTextMessage(op, chat, $"{ex.Message}");
                        } finally
                        {
                            op.ClearCash();
                            op.state = State.free;
                        }
                        break;

                    case State.waiting_player_id_fd_input:
                        try
                        {
                            long player_id;
                            try
                            {
                                player_id = long.Parse(message.Text);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("Неверный формат Player ID");
                            }

                            op.PutIntoCash(ParamType.PLID, $"{player_id}");
                            await bot.SendTextMessageAsync(message.From.Id, $"Введите сумму депозита, который внес игрок {op.GetParamFromCash(ParamType.TGID)}:");
                            op.state = State.waiting_fd_sum;
                        }
                        catch (Exception ex)
                        {
                            op.ClearCash();
                            op.state = State.free;
                            await sendOperatorTextMessage(op, chat, $"{ex.Message}");
                        }
                        break;

                    case State.waiting_fd_sum:
                        try
                        {
                            uint sum = 0;

                            try
                            {
                                sum = uint.Parse(message.Text);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("Неверный формат суммы депозита");
                            }

                            string uuid;
                            string status;

                            var geo = op.GetParamFromCash(ParamType.GEO);
                            var tg_id = long.Parse(op.GetParamFromCash(ParamType.TGID));

                            (uuid, status) = await server.GetFollowerState(geo, tg_id);
                            var plid = long.Parse(op.GetParamFromCash(ParamType.PLID));

                            await server.SetFollowerMadeDeposit(uuid, plid, sum);
                            await sendOperatorTextMessage(op, chat, $"Пользователю присвоена сумма депозита {sum}$");


                        }
                        catch (Exception ex)
                        {
                            await sendOperatorTextMessage(op, chat, $"{ex.Message}");
                        } finally
                        {
                            op.ClearCash();
                            op.state = State.free;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.err(Geotag, ex.Message);
            }
        }


        #endregion

        //async Task selectBot(long chat, string id, Operator op)
        //{
        //    long tg_id = long.Parse(id);
        //    string msg;

        //    var resp = await server.GetUserInfoByTGid(tg_id);
        //    var uinfo = resp.Where(o => !string.IsNullOrEmpty(o.uuid)).ToArray();
        //    if (uinfo != null)
        //    {
        //        var bots = uinfo.Select(r => r.geo).ToArray();
        //        if (bots != null)
        //        {
        //            op.PutIntoCash(ParamType.TGID, $"{tg_id}");
        //            await sendOperatorBotSelectMessage(op, chat, bots);


        //        }
        //    }
        //    else
        //    {
        //        msg = $"Пользователь {tg_id} не подписан ни на одного из ботов";
        //        await sendOperatorTextMessage(op, chat, msg);
        //        op.ClearCash();
        //        op.state = State.free;
        //    }
        //}

        async Task sendLeadData(NotifyDTO dto)
        {
            long chat = dto.closer_tg_id;
            var info = dto.lead_info;

            var un = (!string.IsNullOrEmpty(info.username)) ? $"@{info.username}" : "";
            var fn = (!string.IsNullOrEmpty(info.firstname)) ? info.firstname : "";
            var ln = (!string.IsNullOrEmpty(info.lastname)) ? info.lastname : ""; 
            var tg = info.lead_tg_id;
            var subs = (info.sources.Count > 0) ? info.sources[0] : "NO";

            string search = $"{fn} {ln}".Trim();
            string offer = (!string.IsNullOrEmpty(info.offer_link)) ? $"Offer: `{info.offer_link}`\n" : "";

            string text = $"Username: `{un}`\n" +
                          $"FirstName: `{fn}`\n" +
                          $"LastName: `{ln}`\n" +
                          $"ToSearch: `{search}`\n" +
                          $"Subscribed: `{subs}`\n" +
                          $"ID: `{tg}`\n" +
                          offer;

            string dbg_text = $"Closer: {chat}\n" +
                              $"Username: {un}\n" +
                              $"FirstName: {fn}\n" +
                              $"LastName: {ln}\n" +
                              $"ToSearch: {search}\n" +
                              $"Subscribed: {subs}\n" +
                              $"ID: {tg}";

            try
            {
                logger.inf_urgent(Geotag, dbg_text);

                await bot.SendTextMessageAsync(chat, text, parseMode: ParseMode.MarkdownV2);
                
            } catch (Exception ex)
            {
                logger.err(Geotag, $"sendLeadData: {chat} {ex.Message}");
            }
        }

        async Task notifyClosers(NotifyData notifyData)
        {
            foreach (var item in notifyData.data)
            {
                await sendLeadData(item);
            }
        }

        #region public
        public override Task processFollower(Message message)
        {
            return Task.CompletedTask;
        }

        public override Task UpdateStatus(StatusUpdateDataDto updateData)
        {
            throw new NotImplementedException();
        }

        public override async Task Notify(object notifyObject)
        {
            switch (notifyObject)
            {
                case NotifyData data:
                    await notifyClosers(data);
                    break;
            }
        }
        #endregion
    }
}
