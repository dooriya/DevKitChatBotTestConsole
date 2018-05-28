namespace DevKitChatBotTestConsole
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Connector.DirectLine;
    using Newtonsoft.Json;

    public class DirectLineClientTest
    {
        private readonly string directLineSecret = ConfigurationManager.AppSettings.Get("DirectLineSecret");
        private readonly string botId = ConfigurationManager.AppSettings.Get("BotId");
        private readonly string fromUser = "DirectLineSampleClientUser";

        private string replyMessage;

        public async Task RunAsync()
        {
            await StartBotConversation();
        }

        private async Task StartBotConversation()
        {
            DirectLineClient client = new DirectLineClient(directLineSecret);

            var conversation = await client.Conversations.StartConversationAsync();

            new Thread(async () => await ReceiveBotMessagesAsync(client, conversation.ConversationId)).Start();

            Console.Write("Command> ");

            while (true)
            {
                string input = Console.ReadLine().Trim();

                if (input.ToLower() == "exit")
                {
                    break;
                }
                else
                {
                    if (input.Length > 0)
                    {
                        Activity userMessage = new Activity
                        {
                            From = new ChannelAccount(fromUser),
                            Text = input,
                            Type = ActivityTypes.Message
                        };

                        await client.Conversations.PostActivityAsync(conversation.ConversationId, userMessage);
                    }
                }
            }
        }

        public async Task<string> ReceiveBotMessagesAsync(DirectLineClient client, string conversationId)
        {
            string watermark = null;

            while (true)
            {
                var activitySet = await client.Conversations.GetActivitiesAsync(conversationId, watermark);
                watermark = activitySet?.Watermark;

                var activities = from x in activitySet.Activities
                                 where x.From.Id == botId
                                 select x;

                foreach (Activity activity in activities)
                {
                    Console.WriteLine(activity.Text);
                    Console.Write("Command> ");
                }

                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            }
        }
    }
}
