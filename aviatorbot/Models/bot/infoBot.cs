using asknvl.logger;
using asknvl.server;
using aviatorbot.Model.bot;
using aviatorbot.Models.storage;
using aviatorbot.Operators;
using aviatorbot.rest;
using motivebot.Model.storage;
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
        public infoBot(BotModel model, IOperatorStorage operatorStorage, IBotStorage botStorage, ILogger logger) : base(operatorStorage, botStorage, logger)
        {
            Geotag = "INFO";
            Token = model.token;
            Link = null;
            PM = null;
            Channel = null;
            Postbacks = false;
        }

        public override BotType Type => BotType.getinfo_v0;

        protected override async Task sendOperatorTextMessage(Operator op, long chat, string text)
        {
            ReplyKeyboardMarkup replyKeyboardMarkup = null;

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
            } else
                 if (op.permissions.Any(p => p.type.Equals(OperatorPermissionType.set_user_status)) || op.permissions.Any(p => p.type.Equals(OperatorPermissionType.all)))
            {

                replyKeyboardMarkup = new(new[]
                        {
                            new KeyboardButton[] { $"INFO BY TG ID" },
                            new KeyboardButton[] { $"INFO BY PLAYER ID" },
                            new KeyboardButton[] { $"GIVE REG" },
                            new KeyboardButton[] { $"GIVE FD" },
                            new KeyboardButton[] { $"GIVE VIP" },
                            new KeyboardButton[] { $"CHECK STATUS" }
                        })
                {
                    ResizeKeyboard = true,
                    OneTimeKeyboard = false,
                };
            }

            await bot.SendTextMessageAsync(
                chat,
                text: text,
                replyMarkup: replyKeyboardMarkup,
                parseMode: ParseMode.MarkdownV2);
        }

        public override Task processFollower(Message message)
        {
            return Task.CompletedTask;
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
                        op.state = State.waiting_reg_access;
                        return;
                    }

                    if (message.Text.Equals("GIVE FD"))
                    {
                        await bot.SendTextMessageAsync(message.From.Id, "Введите TG id для установки статуса ФД:");
                        op.state = State.waiting_fd_access;
                        return;
                    }

                    if (message.Text.Equals("GIVE VIP"))
                    {
                        await bot.SendTextMessageAsync(message.From.Id, "Введите TG id для предоставления VIP:");
                        op.state = State.waiting_vip_access;
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
                            await bot.SendTextMessageAsync(message.From.Id, $"{ex.Message}");
                        }
                        catch (Exception ex)
                        {
                            await bot.SendTextMessageAsync(message.From.Id, $"{ex.Message}");
                        } finally
                        {
                            state = State.free;
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
                            await bot.SendTextMessageAsync(message.From.Id, $"{ex.Message}");
                        }
                        break;

                    case State.waiting_reg_access:
                        try
                        {
                            long tg_id = long.Parse(message.Text);
                            string uuid = string.Empty;
                            string status = string.Empty;
                            (uuid, status) = await server.GetFollowerState(Geotag, tg_id);

                            await server.SetFollowerRegistered(uuid);

                            string msg = $"Пользователю {tg_id} установлен статус ЗАРЕГИСТРИРОВАН";
                            await sendOperatorTextMessage(op, chat, msg);
                            logger.inf(Geotag, msg);

                        }
                        catch (Exception ex)
                        {
                            await bot.SendTextMessageAsync(message.From.Id, $"{ex.Message}");
                        } finally
                        {
                            state = State.free;
                        }
                        break;

                    case State.waiting_fd_access:
                        try
                        {
                            long tg_id = long.Parse(message.Text);
                            string uuid = string.Empty;
                            string status = string.Empty;
                            (uuid, status) = await server.GetFollowerState(Geotag, tg_id);

                            await server.SetFollowerMadeDeposit(uuid);

                            string msg = $"Пользователю {tg_id} установлен статус ФД";
                            await sendOperatorTextMessage(op, chat, msg);
                            logger.inf(Geotag, msg);

                        }
                        catch (Exception ex)
                        {
                            await bot.SendTextMessageAsync(message.From.Id, $"{ex.Message}");
                        } finally
                        {
                            state = State.free;
                        }
                        break;

                    case State.waiting_vip_access:
                        try
                        {
                            long tg_id = long.Parse(message.Text);
                            string uuid = string.Empty;
                            string status = string.Empty;
                            (uuid, status) = await server.GetFollowerState(Geotag, tg_id);

                            if (!string.IsNullOrEmpty(uuid))
                            {
                                switch (status)
                                {
                                    case "WREG":
                                        await server.SetFollowerRegistered(uuid);
                                        await server.SetFollowerMadeDeposit(uuid);
                                        await server.SetFollowerMadeDeposit(uuid);
                                        break;
                                    case "WFDEP":
                                        await server.SetFollowerMadeDeposit(uuid);
                                        await server.SetFollowerMadeDeposit(uuid);
                                        break;
                                    case "WREDEP1":
                                        await server.SetFollowerMadeDeposit(uuid);
                                        break;
                                    case "WREDEP2":
                                        break;
                                    default:
                                        return;
                                }

                                string msg = $"Пользователю {tg_id} предоставлен доступ к каналу";
                                await sendOperatorTextMessage(op, chat, msg);
                                logger.inf(Geotag, msg);
                            }
                        }
                        catch (Exception ex)
                        {
                            await bot.SendTextMessageAsync(message.From.Id, $"{ex.Message}");
                        } finally
                        {
                            state = State.free;
                        }
                        break;


                }
            }
            catch (Exception ex)
            {
                logger.err(Geotag, ex.Message);
            }
        }

        public override Task UpdateStatus(StatusUpdateDataDto updateData)
        {
            throw new NotImplementedException();
        }
    }
}
