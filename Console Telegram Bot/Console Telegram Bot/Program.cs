using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Console_Telegram_Bot
{
    public static class Program
    {
        private static readonly TelegramBotClient Bot = new TelegramBotClient("563529619:AAF-CTPttZqDOwSOKne2bzU9HAoA-PTMvo0");

        public static void Main(string[] args)
        {
            // Initialize logger
            Logger.InitLogger();
            var me = Bot.GetMeAsync().Result;
            Console.Title = me.Username;
            Logger.Log.Info(String.Format("Started bot: {0}", me.Username));

            // Events subscription
            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnMessageEdited += BotOnMessageReceived;
            Bot.OnInlineResultChosen += BotOnChosenInlineResultReceived;
            Bot.OnReceiveError += BotOnReceiveError;
            Bot.OnCallbackQuery += BotOnCallbackQueryReceived;

            Bot.StartReceiving(new List<UpdateType>().ToArray());
            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            Logger.Log.Info(String.Format("Stopped bot: {0}", me.Username));
            Bot.StopReceiving();
        }

        private static async void SendDefaultActionButtons(long chatId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(new[]
                   {
                        new [] // first row
                        {
                            InlineKeyboardButton.WithCallbackData("When?"),
                            InlineKeyboardButton.WithCallbackData("Where?"),
                        },
                        new [] // second row
                        {
                            InlineKeyboardButton.WithCallbackData("How to get there?")
                        }
                    });

            await Bot.SendTextMessageAsync(
                chatId,
                "Chose option =)",
                replyMarkup: inlineKeyboard);
        }

        /// <summary>
        /// String message received
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="messageEventArgs"></param>
        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            try
            {
                var message = messageEventArgs.Message;
                if (message == null || message.Type != MessageType.Text)
                    return;

                var userFirstName = message.From.FirstName;
                var userLastName = message.From.LastName;
                var chatId = message.Chat.Id;
                var messageText = message.Text.Split(' ').First();
                Logger.Log.Info(String.Format("Message received from: {0} {1} text is {2}", userFirstName, userLastName, messageText));

                switch (messageText)
                {

                    case "/about":
                        // Send message with explanation
                        await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
                        await Bot.SendTextMessageAsync(
                            message.Chat.Id,
                            "Greetings to you, " + userFirstName + "!");

                        await Bot.SendTextMessageAsync(
                            message.Chat.Id,
                            "This small bot was created to communicate and help you.");

                        await Bot.SendTextMessageAsync(
                           message.Chat.Id,
                           "The fact is that one redhead (photo below) will soon have a birthday. So, why do not you come to visit her and congratulate her solemnly?");

                        await Bot.SendTextMessageAsync(
                            message.Chat.Id,
                            "To do this, use the auxiliary buttons.");

                        await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

                        // Send photo
                        const string file = @"..\..\Photo.jpg";
                        using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            await Bot.SendPhotoAsync(
                                chatId,
                                fileStream,
                                "Some funny photo");
                        }

                        SendDefaultActionButtons(chatId);

                        break;

                    //send custom keyboard
                    case "/what":

                        SendDefaultActionButtons(chatId);
                        break;

                    default:
                        const string usage = @"
                            This is a small bot-invitation, created just for fun.
How to use:
/about   - information about this bot";

                        await Bot.SendTextMessageAsync(
                            chatId,
                            usage,
                            replyMarkup: new ReplyKeyboardRemove());
                        break;
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error("Error in BotOnMessageReceived", e);
            }
        }
        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            try
            {
                var callbackQuery = callbackQueryEventArgs.CallbackQuery;
                Logger.Log.Info(String.Format("CallbackQuery received from: {0} {1} text is {2}", callbackQueryEventArgs.CallbackQuery.From.FirstName, callbackQueryEventArgs.CallbackQuery.From.LastName, callbackQuery.Data));

                long chatId = callbackQuery.Message.Chat.Id;
                switch (callbackQuery.Data)
                {
                    case "When?":

                        await Bot.SendChatActionAsync(chatId, ChatAction.Typing);
                        await Bot.SendTextMessageAsync(
                            chatId,
                            $"I will wait for you on the 30th of June.");

                        SendDefaultActionButtons(chatId);
                        break;

                    case "Where?":

                        await Bot.SendTextMessageAsync(
                            chatId,
                            $"Hmmm, you want to know where, unexpectedly!");

                        await Bot.SendChatActionAsync(chatId, ChatAction.Typing);
                        await Bot.SendTextMessageAsync(
                            chatId,
                            $"You need to go here: 46°28'42.8\"N 30°45'22.1\"E");

                        await Bot.SendChatActionAsync(chatId, ChatAction.Typing);
                        await Task.Delay(5000);
                        await Bot.SendTextMessageAsync(
                            chatId,
                            $"Well, here: https://www.google.ru/maps/place/46%C2%B028'42.8%22N+30%C2%B045'22.1%22E/@46.4785597,30.7539373,17z/data=!3m1!4b1!4m6!3m5!1s0x0:0x0!7e2!8m2!3d46.4785563!4d30.7561257");

                        SendDefaultActionButtons(chatId);
                        break;

                    case "How to get there?":
                        await Bot.SendTextMessageAsync(
                            chatId,
                            $"Well, it's very easy.\n" +
                            $"There are two ways to go there:");

                        await Bot.SendTextMessageAsync(
                            chatId,
                            $"1) The first one");

                        await Bot.SendTextMessageAsync(
                            chatId,
                            $"2) The second one");

                        SendDefaultActionButtons(chatId);
                        break;
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error("Error in BotOnCallbackQueryReceived", e);
            }
        }

        private static void BotOnChosenInlineResultReceived(object sender, ChosenInlineResultEventArgs chosenInlineResultEventArgs)
        {
            Logger.Log.Info($"Received inline result: {chosenInlineResultEventArgs.ChosenInlineResult.ResultId}");
        }

        private static void BotOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            Logger.Log.Error(String.Format("Received error: {0} — {1}",
                receiveErrorEventArgs.ApiRequestException.ErrorCode,
                receiveErrorEventArgs.ApiRequestException.Message));
        }
    }
}