using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using System.Net.Http;

using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Discord;

namespace DiscordBot
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;

        // Retrieve client and CommandService instance via ctor
        public CommandHandler(DiscordSocketClient client, CommandService commands)
        {
            _commands = commands;
            _client = client;
        }

        public async Task InstallCommandsAsync()
        {
            // Hook the MessageReceived event into our command handler
            _client.MessageReceived += HandleCommandAsync;

            // Here we discover all of the command modules in the entry 
            // assembly and load them. Starting from Discord.NET 2.0, a
            // service provider is required to be passed into the
            // module registration method to inject the 
            // required dependencies.
            //
            // If you do not use Dependency Injection, pass null.
            // See Dependency Injection guide for more information.
            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                                            services: null);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasCharPrefix('!', ref argPos) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(_client, message);

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.
            await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: null);
        }



        // Keep in mind your module **must** be public and inherit ModuleBase.
        // If it isn't, it will not be discovered by AddModulesAsync!
        // Create a module with no prefix
        public class InfoModule : ModuleBase<SocketCommandContext>
        {
            string esse = @"https://eu.finalfantasyxiv.com/lodestone/character/19772019/";
            // ~say hello world -> hello world
            [Command("pndm")]
            [Summary("Echoes a message.")]
            public Task SayAsync([Remainder] [Summary("The text to echo")] string echo)
                => ReplyAsync(echo);

            [Command("rev")]
            [Summary("Echoes a message.")]
            public async Task RevAsync()
            {
                HttpResponseMessage req = await "https://xivapi.com/character/19772019?data=FC".GetAsync();
                dynamic item = JsonConvert.DeserializeObject(
                    req.Content.ReadAsStringAsync().Result
                );
                esse = $"{item.Character.Name}";

                await ReplyAsync(esse);
            }

            [Command("jopep")]
            [Summary("Echoes a message.")]
            public async Task JopepAsync()
            {

                await ReplyAsync("FORZA NAPOLIIIIIIIIIIIIIIIIIIIIIIII");
            }

            [Command("helpme")]
            [Summary("Echoes a message.")]
            public async Task HelpAsync()
            {

                await ReplyAsync("!jopep");
            }


            [Command("pndmiam")]
            [Summary("Ricerca Lodestone")]
            public async Task IdSearchAsync(
    [Summary("Charname")]
        string charname, [Summary("Lastname")] string lastname)
            {
                string nametosearch = charname + "%20" + lastname;
                //nametosearch = charname.Replace(" ", "%20");
                string tosearch = "https://xivapi.com/character/search?name=" + nametosearch + "&server=ragnarok";
                //string msg= "https://xivapi.com/character/" + id + "?data=FC";
                HttpResponseMessage req = await tosearch.GetAsync();
                dynamic item = JsonConvert.DeserializeObject(
                    req.Content.ReadAsStringAsync().Result
                );

               
                string xyz = $"{item.Results[0].ID}";
                string msg = "https://xivapi.com/character/" + xyz + "?data=FC";

                HttpResponseMessage req2 = await msg.GetAsync();
                dynamic item2 = JsonConvert.DeserializeObject(
                    req2.Content.ReadAsStringAsync().Result
                );
                string fccheck = $"{item2.FreeCompany.ID}";

                if (fccheck.Equals("9237023573225243144")) //ID PNDM
                {
                    await Context.Channel.SendMessageAsync("Fai parte di Pandemonium,ti è stato assegnato il ruolo Official.");
                    var user = Context.User;
                    var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Official");
                    await (user as IGuildUser).AddRoleAsync(role);
                   // await (user as IGuildUser).ModifyAsync(x =>
                    //{
                      //  x.Nickname = charname+" "+lastname;
                    //});
                }
                else
                {
                    await Context.Channel.SendMessageAsync("Non fa parte di Pandemonium");

                }
                // We can also access the channel from the Command Context.
            }


            private async Task GetDataFromLodestone()
            {

                HttpResponseMessage req = await "https://xivapi.com/character/search?name=iroha%20revn&server=ragnarok".GetAsync();
                dynamic item = JsonConvert.DeserializeObject(
                    req.Content.ReadAsStringAsync().Result
                );

            }



            // ReplyAsync is a method on ModuleBase 
        }

        // Create a module with the 'sample' prefix
        [Group("sample")]
        public class SampleModule : ModuleBase<SocketCommandContext>
        {
            // ~sample square 20 -> 400
            [Command("square")]
            [Summary("Squares a number.")]
            public async Task SquareAsync(
                [Summary("The number to square.")]
        int num)
            {
                // We can also access the channel from the Command Context.
                await Context.Channel.SendMessageAsync($"{num}^2 = {Math.Pow(num, 2)}");
            }

            // ~sample userinfo --> foxbot#0282
            // ~sample userinfo @Khionu --> Khionu#8708
            // ~sample userinfo Khionu#8708 --> Khionu#8708
            // ~sample userinfo Khionu --> Khionu#8708
            // ~sample userinfo 96642168176807936 --> Khionu#8708
            // ~sample whois 96642168176807936 --> Khionu#8708
            [Command("userinfo")]
            [Summary
            ("Returns info about the current user, or the user parameter, if one passed.")]
            [Alias("user", "whois")]
            public async Task UserInfoAsync(
                [Summary("The (optional) user to get info from")]
        SocketUser user = null)
            {
                var userInfo = user ?? Context.Client.CurrentUser;
                await ReplyAsync($"{userInfo.Username}#{userInfo.Discriminator}");
            }
        }
    }
}
