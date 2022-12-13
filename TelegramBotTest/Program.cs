using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
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
                var message = update.Message;
                if (update.Type == UpdateType.Message && message?.Text != null)
                {
                    await HandleMessage(botClient, message);
                    return;
                }

                if (update?.Type == UpdateType.CallbackQuery && update?.CallbackQuery != null)
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
            new KeyboardButton[] {"Поиск по ключу", "Команды консоли (AutoCAD)"},
            new KeyboardButton[] {"Конструктор шапок", "Конструктор колодцев"}
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
                InlineKeyboardButton.WithCallbackData("Поиск по ключу", "search_key"),                 //Создает кнопку которая при нажатии ее отправляет боту CallbackQuerry
                InlineKeyboardButton.WithCallbackData("Команды консоли (AutoCAD)", "command_AutoCAD"),
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
                if (message.Text.ToLower().Contains("ошибка 88"))
                {
                    await using Stream stream = System.IO.File.OpenRead("C:\\WORK\\Профиль Документация\\Описание ошибки 88.pdf");
                    Message mess = await botClient.SendDocumentAsync(
                                                    message.Chat.Id,
                                                    document: new InputOnlineFile(content: stream, fileName: "Ошибка 88.pdf"),
                                                    caption: "Описание ошибки 88");
                    return;
                }
                await botClient.SendTextMessageAsync(message.Chat.Id, $"You said:\n{message.Text}");
            }

            async static Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
            {
                if (callbackQuery.Data.StartsWith("search"))
                {
                    await botClient.SendTextMessageAsync(
                        callbackQuery.Message.Chat.Id,
                        $"Напишите ключевое слово, по которому стоит искать"
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
