using aksnvl.messaging;
using aviatorbot.Models.messages.latam;
using System.Threading.Tasks;
using System.Threading;
using System;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using asknvl.logger;

public class pushStartProcess
{
    #region vars        
    CancellationTokenSource cts;
    ITelegramBotClient bot;
    MP_latam_basic_v2 mp;
    ILogger logger;
    string geotag;
    Action<PushMessageBase?, string, string> checkMessage;
    #endregion

    #region properties
    public long chat { get; set; }
    public bool is_running { get; set; }
    #endregion

    public pushStartProcess(string geotag, long chat, ITelegramBotClient bot, MP_latam_basic_v2 mp, ILogger logger, Action<PushMessageBase?, string, string> checkMessage)
    {
        this.geotag = geotag;
        this.chat = chat;
        this.bot = bot;
        this.mp = mp;
        this.logger = logger;
        this.checkMessage = checkMessage;


        cts = new CancellationTokenSource();
    }

    async void worker()
    {
        is_running = true;

        try
        {

            for (int i = 0; i < mp.start_push_number; i++)
            {
                try
                {
                    cts.Token.ThrowIfCancellationRequested();

                    PushMessageBase m = null;
                    ReplyKeyboardMarkup b = null;

                    (m, b) = mp.GetMessageAndReplyMarkup($"hi_{i}_in");
                    checkMessage(m, $"hi_{i}_in", "pushStartProcess");
                    await m.Send(chat, bot, b);
                    logger.dbg(geotag, $"{chat} > pushStartProcess sent {i}");
                    await Task.Delay(45000, cancellationToken: cts.Token);
                }
                catch (OperationCanceledException ex)
                {
                    logger.dbg(geotag, $"{chat} > pushStartProcess stopped");
                    break;
                }
                catch (Exception ex)
                {
                    logger.err(geotag, $"{chat} > pushStartProcess: unable to send start message {i}");
                }
            }
        }
        catch (Exception ex)
        {
            logger.err(geotag, $"{chat} > worker {ex.Message}");
        } finally
        {
            is_running = false;
        }
    }

    public void start()
    {
        Task.Run(() => worker());
    }
    public void stop()
    {
        cts?.Cancel();
    }
}