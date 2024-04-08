using Avalonia.X11;
using DynamicData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace aviatorbot.Models.bot
{
    public class errorMessageGenerator
    {
        public static string getUserStatusOnStartError(Exception ex)
        {
            return $"Ошибка запроса статуса пользователя {ex.Message}";
        }        
        public static string getAddUserBotDBError(string userinfo)
        {
            return $"Ошибка добавления пользователя бота {userinfo} в базу данных";
        }
        public static string getStartUserError(string userinfo)
        {
            return $"Ошибка обработки /start пользователя {userinfo}";
        }
        public static string getProcessCallbackQueryError(string userinfo)
        {
            return $"Ошибка обработки активности пользователя {userinfo}";
        }
        public static string getProcessChatJoinRequestError(long tg_id, string channel_tag)
        {
            return $"Ошибка одобрения запроса на подписку пользователя {tg_id} в канал {channel_tag}";
        }
        public static string getAddUserChatDBError(long tg_id, string channel_tag)
        {
            return $"Ошибка обновления данных о подписке {tg_id} {channel_tag} в БД";
        }
        public static string getRemoveUserChatDBError(long tg_id, string channel_tag)
        {
            return $"Ошибка обновления данных об отписке {tg_id} {channel_tag} в БД";
        }
        public static string getProcessChatMemberError(Exception ex)
        {
            return $"Ошибка обработки processChatMember {ex.Message}";
        }
        public static string getOpertatorProcessError(long tg_id, Exception ex)
        {
            return $"Ошибка обработки действия оператора {tg_id} {ex.Message}";
        }
        public static string getBotApiError(string message)
        {
            return $"Ошибка API: {message}";
        }
        public static string getUserStatusUpdateError(Exception ex)
        {
            return $"Ошибка обновления статуса пользователя: {ex.Message}";
        }
        public static string getUserPushError(long tg_id, string status, string code, Exception ex)
        {
            return $"Ошибка отправки push {tg_id} {status} {code}: {ex.Message}";
        }
        public static string getSetMessageError(string status, string source)
        {
            return $"Не установлено сообщение {status} в {source}";
        }
    }
}
