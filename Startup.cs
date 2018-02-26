using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebDevEsports.Data;
using WebDevEsports.Models;
using WebDevEsports.Services;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace WebDevEsports
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IFileProvider>(
                new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")));

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();

            // Configure Identity
            services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;
                
                // User settings
                options.User.RequireUniqueEmail = true;
            });

            services.AddMvc();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(o => o.LoginPath = new PathString("/login"));

            services.AddAuthorization(options =>
            {
                options.AddPolicy("MemberRights", policy => policy.RequireRole("Member"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider, ApplicationDbContext context)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            context.Database.Migrate();

            // Create roles and assign a user to a role for testing
            SeedData(serviceProvider, context).Wait();
        }

        private async Task SeedData(IServiceProvider serviceProvider, ApplicationDbContext context)
        {
            //initializing custom roles 
            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var UserManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            string[] roleNames = { "Member", "Customer" };
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                var roleExist = await RoleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    //create the roles and seed them to the database
                    roleResult = await RoleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var user = await UserManager.FindByEmailAsync("Member1@email.com");
            // if the user doesn't exist, create it
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = "Member1@email.com",
                    Email = "Member1@email.com",
                    DisplayName = "Member1"
                };
                await UserManager.CreateAsync(user, "password");
            }

            //here we tie the new user to the role
            await UserManager.AddToRoleAsync(user, "Member");

            // Seed users in database
            CreateUser(UserManager, "Customer1@email.com", "Customer1").Wait();
            CreateUser(UserManager, "Customer2@email.com", "Customer2").Wait();
            CreateUser(UserManager, "Customer3@email.com", "Customer3").Wait();
            CreateUser(UserManager, "Customer4@email.com", "Customer4").Wait();
            CreateUser(UserManager, "Customer5@email.com", "Customer5").Wait();

            // Seed announcements in database
            if (context.Announcement.Count() == 0)
            {
                SeedAnnouncements(UserManager, context).Wait();
            }

            // Seed players in database
            if (context.Player.Count() == 0)
            {
                SeedPlayers(context).Wait();
            }
        }

        private static async Task CreateUser(UserManager<ApplicationUser> UserManager, string UserName, string DisplayName)
        {
            var user = await UserManager.FindByEmailAsync(UserName);
            // if the user doesn't exist, create it
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = UserName,
                    Email = UserName,
                    DisplayName = DisplayName
                };
                await UserManager.CreateAsync(user, "password");
            }
        }
        
        private static async Task SeedAnnouncements(UserManager<ApplicationUser> UserManager, ApplicationDbContext context)
        {
            ApplicationUser user = await UserManager.FindByEmailAsync("Member1@email.com");

            Announcement a = new Announcement()
            {
                Title = "G2 Welcomes bodyy",
                Content = "Today marks the beginning of a new era in our Counter-Strike team. When we signed the former Titan squad, back in February, it was clear that the bar would be set very high. The players shared our ambition and set themselves the target of fighting for every international title. I am happy to welcome Alexandre “bodyy” Pianaro to the team. He stood out to us as having that “little bit extra” that only the greatest players have, and we look forward to adding his skillset to our team. As a result of this roster change, Richard “shox” Papillon will inherit the heavy role of team captain, further cementing the beginning of a new era.”“It is with a great motivation and desire to win that I join G2 today. I feel incredibly blessed to have been approached by this team, and I will prove that I can live up to their expectations. I want to start by thanking Team LDLC for all they have given me during this past year playing under their colors. I would particularly like to thank my former teammates, without them none of this would be possible and they have all become my good friends. I cannot thank them enough for the opportunity they gave to me two years ago, and I wish them the best for the future.“I feel that this is unreal, going from the French subtop to one of the best teams in the world. The chance I’ve been given is just incredible, and I want to thank everyone for the confidence they have put in me. I intend to give 2000% to prove my value, to advance and reach the goals of the team, and to together win the most prestigious competitions.”You will be able to watch Pianaro’s debut for G2 in just a few hours as we will face Gambit Gaming in the qualifiers for the Esports Championship Series (ECS). His first offline appearance for the team will come next week, at DreamHack Masters Malmö, kicking off on Tuesday, April 12th!",
                DateTime = DateTime.Now,
                AuthorDisplayName = user.DisplayName,
                Author = user,
                NumberViews = 0,
                ImageName = "bodyy-joins-g2.jpg"
            };

            Announcement b = new Announcement()
            {
                Title = "We Will Be Competing At IEM Oakland 2017",
                Content = "Just this week ESL announced that Intel® Extreme Masters is coming back to the Bay Area for the 4th year and G2 Esports CS:GO squad will also be there! The event will be taking place on November 18-19, at the Oracle Arena, home of the American professional basketball team Golden State Warriors. In Oakland we will be joining 11 other world -class teams, all fighting for their share of a $300,000 prize pool.Two of our future opponents are already known – it will be FaZe Clan and Astralis.In total 8 invites will be sent out to the top CS:GO squads, four additional teams will qualify through a series of online and offline qualifiers open for teams from Europe and North America. Last year we ended up in the middle of the table sharing 5th-6th place with Immortals. With our current strength we feel that in the 2017 iteration of the tournament we can aim for a much longer run with a great dose of excitement for our fans.",
                DateTime = DateTime.Now,
                AuthorDisplayName = user.DisplayName,
                Author = user,
                NumberViews = 0,
                ImageName = "iem-oakland.jpg"
            };

            Announcement c = new Announcement()
            {
                Title = "Gamescom 2017: Meet G2 Players!",
                Content = "The annual trade fair for digital gaming culture, gamescom, is one of the main yearly attractions for gamers in Cologne, Germany. The German Chancellor Angela Merkel will be there this year, of course, G2 Esports wouldn’t miss such a happening too! This year around we will not have full teams greetings fans at gamescom due to intensive practice schedule.However,we couldn’t leave G2 Esports fans hanging and prepared some fun action together with our partners paysafecard and Kinguin! The G2 day will be 25th of August, Friday.Fans will have a chance to meet our CS:GO players bodyy and apEX, as well as our Hearthstone star Thijs.Some secret information for Thijs fans – you can meet him around gamescom on other days too as he’ll be streaming from the event! On Friday 25th we’ll start our morning early. From 9.30 AM bodyy and apEX will face each other in the third rendition of paysafecard’s “Beat the Legends”. Make sure to watch the teams, led by our players, battle it out in the ESL Arena. The action will go on here until approximately 3 PM. The second highlight of the day will be the signing session with Thijs, apEX and bodyy at the Kinguin booth (hall 10.01 booth D051). Don’t miss this exclusive opportunity to take a picture, get an autograph or just wish our players the best of luck in upcoming tournaments!",
                DateTime = DateTime.Now,
                AuthorDisplayName = user.DisplayName,
                Author = user,
                NumberViews = 0,
                ImageName = "gamescom2017.jpg"
            };
            
            context.Announcement.Add(a);
            context.Announcement.Add(b);
            context.Announcement.Add(c);
            await context.SaveChangesAsync();
        }
        
        private static async Task SeedPlayers(ApplicationDbContext context)
        {
            Player a = new Player()
            {
                FirstName = "Kenny",
                LastName = "Schrub",
                GamerName = "KennyS",
                Position = "AWP",
                Bio = "Kenny “kennyS” Schrub is the new AWPer of G2 Esports. In 2012, Kenny emerged as one of the top AWPers in the CS: Source scene, which earned him a move to the legendary VeryGames team. Over the last three years, kennyS has had a big evolution in his game, to the point that he is now regarded as the most well-rounded sniper in the game. Kenny not only helped to break the rifle dominance during the early stages of CS:GO but he also proved that it is possible for an AWPer to become the best player in the world. In the last four years, he has been ever present in HLTV’s top 20 player ranking, placing sixth in 2014 and 2015.",
                ImageName = "kennyS.png"
            };

            Player b = new Player()
            {
                FirstName = "Richard",
                LastName = "Papillon",
                GamerName = "KennyS",
                Position = "Lurk",
                Bio = "Richard “shox” Papillon has been at the top of the pile since the early days of CS:GO, and his presence in HLTV.org’s Top 20 year rankings in the last three years is proof of that. Boasting an incredible rating of 1.13 – one of the highest among top CS:GO players -, shox is heralded as one of the most versatile players in the game, and he will be looking to inspire the team to win international silverware after clinching a major title in 2014.",
                ImageName = "shox.png"
            };

            Player c = new Player()
            {
                FirstName = "Nathan",
                LastName = "Schmitt",
                GamerName = "NBK",
                Position = "Support",
                Bio = "Nathan “NBK-” Schmitt joins G2 Esports with a huge number of international titles on his back, including two Major crowns. NBK- has played an integral role in every top French team, being called the “kingmaker” by renowned esports journalist and caster Duncan “Thorin” Shields. Known for his incredible consistency, NBK- featured in HLTV.org’s prestigious Top 20 ranking for three years in a row. After a year of only modest success, NBK- will be looking to crown new kings, now as part of G2.",
                ImageName = "nbk.png"
            };

            Player d = new Player()
            {
                FirstName = "Dan",
                LastName = "Madesclaire",
                GamerName = "apEX",
                Position = "Entry Fragger",
                Bio = "Dan “apEX” Madesclaire is one of the newest members of G2 Esports. He is a veteran of the French scene, having played for a number of top teams since 2009, including the final CS: Source lineup of VeryGames. Despite the fact that he has been playing at the top since CS:GO was released, apEX only truly gained notoriety when he helped Clan-Mystik to win ESWC 2013, one of the biggest upsets of that year. He went on to play for LDLC, Titan and EnVyUs, winning some of the biggest events in the world, including DreamHack Open Cluj-Napoca Major, Gfinity CS:GO Invitational and WESG 2016. apEX, who was named by HLTV.org as one of the top 20 players in the world in 2014 and 2015, is known for being one of the most reliable players in the scene, and his explosiveness makes him a constant threat to the opponents he faces ingame.",
                ImageName = "apex.png"
            };

            Player e = new Player()
            {
                FirstName = "Alexandre",
                LastName = "Pianaro",
                GamerName = "bodyy",
                Position = "Support",
                Bio = "Alexandre “bodyy” Pianaro is the newest player on our Counter-Strike team. He was an integral part of the Platinium eSports team that stunned Ninjas in Pyjamas at ESWC 2014 before joining LDLC White, who would go on to win Assembly Winter 2016. After impressing the scene with his raw talent and great ingame knowledge, Pianaro, 19, is ready to take the next step in his career and prove his credentials as one of France’s rising talents.",
                ImageName = "body.png"
            };

            context.Player.Add(a);
            context.Player.Add(b);
            context.Player.Add(c);
            context.Player.Add(d);
            context.Player.Add(e);
            await context.SaveChangesAsync();
        }
    }
}
