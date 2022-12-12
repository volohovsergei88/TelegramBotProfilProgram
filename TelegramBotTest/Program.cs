using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var botClient = new TelegramBotClient("5807686806:AAGt8rDk5Ps34ja-0oZDGtcAsa5Npk5Jil8");

            using var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }
            };

            botClient.StartReceiving(
                HandleUpdatesAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken: cts.Token);

         //   var me = await botClient.GetMeAsync();

          //  Console.WriteLine($"Начал прослушку @{me.Username}");
            Console.ReadLine();

            cts.Cancel();
        
            async static Task HandleUpdatesAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            {
                if (update.Type == UpdateType.Message && update?.Message?.Text != null)
                {
                    await HandleMessage(botClient, update.Message);
                    return;
                }

                if (update?.Type == UpdateType.CallbackQuery)
                {
                    await HandleCallbackQuery(botClient, update.CallbackQuery);
                    return;
                }
            }

            async static Task HandleMessage(ITelegramBotClient botClient, Message message)
            {
                
                if (message.Text == "/start")
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Choose commands: /inline | /keyboard");
                    return;
                }

                if (message.Text == "/keyboard")
                {
                    ReplyKeyboardMarkup keyboard = new(new[]
                    {
            new KeyboardButton[] {"Hello", "Goodbye"},
            new KeyboardButton[] {"Привет", "Пока"}
        })
                    {
                        ResizeKeyboard = true
                    };
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Choose:", replyMarkup: keyboard);
                    return;
                }

                if (message.Text == "/inline")
                {
                    InlineKeyboardMarkup keyboard = new(new[]
                    {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Buy 50c", "buy_50c"),
                InlineKeyboardButton.WithCallbackData("Buy 100c", "buy_100c"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Sell 50c", "sell_50c"),
                InlineKeyboardButton.WithCallbackData("Sell 100c", "sell_100c"),
            },
        });
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Choose inline:", replyMarkup: keyboard);
                    return;
                }

                await botClient.SendTextMessageAsync(message.Chat.Id, $"You said:\n{message.Text}");
            }
            async static Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
            {
                if (callbackQuery.Data.StartsWith("buy"))
                {
                    await botClient.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        $"Вы хотите купить?"
                    );
                    return;
                }
                if (callbackQuery.Data.StartsWith("sell"))
                {
                    await botClient.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        $"Вы хотите продать?"
                    );
                    return;
                }
                await botClient.SendTextMessageAsync(
                    callbackQuery.Message.Chat.Id,
                    $"You choose with data: {callbackQuery.Data}"
                    );
                return;
            }

            Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
            {
                var ErrorMessage = exception switch
                {
                    ApiRequestException apiRequestException
                        => $"Ошибка телеграм АПИ:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
                    _ => exception.ToString()
                };
                Console.WriteLine(ErrorMessage);
                return Task.CompletedTask;
            }
            Console.ReadLine();
        }
    }
}
