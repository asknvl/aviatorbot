using asknvl.logger;
using aviatorbot.Model.bot;
using aviatorbot.Models.storage;
using aviatorbot.Operators;
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
        public infoBot(BotModel model, IOperatorStorage operatorStorage, ILogger logger) : base(operatorStorage, logger)
        {
            Geotag = "INFO";
            Token = model.token;
            Link = null;
            PM = null;
            Channel = null;
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
            }
            await bot.SendTextMessageAsync(
                chat,
                text: text,
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
                    if (message.Text.Equals("CHECK STATUS"))
                    {
                        await bot.SendTextMessageAsync(message.From.Id, "Введите TG ID для определения статуса:");
                        op.state = State.waiting_check_status;
                        return;
                    }                    
                }

                switch (op.state)
                {
                    case State.waiting_new_message:
                        MessageProcessor.Add(AwaitedMessageCode, message, PM);
                        state = State.free;
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
                            logger.inf(geotag, msg);

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
                            logger.inf(geotag, msg);

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
                                logger.inf(geotag, msg);
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

                    case State.waiting_check_status:
                        try
                        {
                            long tg_id = long.Parse(message.Text);
                            var resp = await server.GetFollowerStateResponse(Geotag, tg_id);

                            string text_status = "";

                            switch (resp.status_code)
                            {
                                case "WREG":
                                    text_status = "Не зарегистрирован";
                                    break;
                                case "WFDEP":
                                    text_status = "Ожидается ФД";
                                    break;
                                default:
                                    if (resp.status_code.Contains("WREDEP"))
                                    {
                                        text_status = $"Ожидается редепозит {resp.status_code.Replace("WREDEP", "")}";
                                    }
                                    break;
                            }

                            string affId = (!string.IsNullOrEmpty(resp.player_id)) ? $", id в ПП: {resp.player_id}" : "";

                            string msg = $"Cтатус пользователя `{tg_id}`: {text_status} {affId}";
                            await sendOperatorTextMessage(op, chat, msg);
                            logger.inf(geotag, msg);
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
    }
}
