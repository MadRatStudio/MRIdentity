using Dal;
using Dal.Tasks;
using Infrastructure.Entities;
using Infrastructure.Entities.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MigrationRunner.Infrastructure.Attr;
using MigrationRunner.Infrastructure.Interface;
using MRDbIdentity.Domain;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationRunner.Migrations
{
    [MRMigration("Startup migration", "31.03.2019")]
    public class StartupMigration : BasicMigration, IMigration
    {
        public override async Task Run()
        {
            await Seed(_serviceProvider);
        }

        private static List<Role> Roles = new List<Role>
        {
            new Role
            {
                Name = "ADMIN",
            },
            new Role
            {
                Name = "USER",
            },
            new Role
            {
                Name = "MANAGER"
            }
        };

        private static Dictionary<string, AppUser> Users = new Dictionary<string, AppUser>
        {
            {
                "Tf27324_()_",
                new AppUser
                {
                    Birthday = new DateTime(1995, 3, 20, 0, 0, 0, 0, DateTimeKind.Local),
                    FirstName = "Oleh",
                    LastName = "Tymofieiev",
                    Email = "oleg.timofeev20@gmail.com",
                    UserName = "somemyname",
                    Tels = new List<UserTel>
                    {
                        new UserTel
                        {
                            CreatedTime = DateTime.UtcNow,
                            Name = "Main",
                            Number = "+380508837161"
                        }
                    },
                    State = true,
                    Avatar = new UserAvatar
                    {
                        Src = "https://www.w3schools.com/howto/img_avatar.png"
                    }
                }
            }
        };

        private static List<EmailSendTask> EmailsOnStart = new List<EmailSendTask>
        {
            new EmailSendTask
            {
                Bot = EmailTaskBot.MadRatBot,
                Body = $"Service started at {DateTime.Now}",
                Status = EmailSendStatus.New,
                Subject = "Identity startup",
                ToEmail = "oleg.timofeev20@gmail.com",
                State = true
            }
        };

        const string LANGUAGES_FILE = "Data/LanguageCodes.json";

        public async Task Seed(IServiceProvider service)
        {
            _logAction($"Start seed database", Infrastructure.Enum.LogType.COMMON);

            await SeedUsers(service);
            Console.WriteLine("\n");

            await SeedLanguages(service);
            Console.WriteLine("\n");

            await SendEmails(service);

            Console.WriteLine($"Seed database finished");
        }

        private async Task SeedUsers(IServiceProvider service)
        {
            var userRepository = service.GetRequiredService<IUserStore<AppUser>>();
            var roleRepository = service.GetRequiredService<IRoleStore<Role>>();
            var userManager = service.GetRequiredService<AppUserManager>();

            Console.WriteLine($"Start seed users");

            foreach (var role in Roles)
            {
                var roleId = await roleRepository.GetRoleIdAsync(role, new System.Threading.CancellationToken());
                if (!string.IsNullOrWhiteSpace(roleId))
                {
                    role.Id = roleId;
                }
                else
                {
                    Console.WriteLine($"Add role {role.Name}");
                    await roleRepository.CreateAsync(role, new System.Threading.CancellationToken());
                }
            }

            foreach (var user in Users)
            {
                if ((await userManager.FindByEmailAsync(user.Value.Email)) == null)
                {
                    var result = await userManager.CreateAsync(user.Value);
                    if (!result.Succeeded)
                    {
                        Console.WriteLine("Error");
                        Console.WriteLine(string.Join("\n", result.Errors));
                    }

                    result = await userManager.AddPasswordAsync(user.Value, user.Key);

                    await userManager.AddToRolesAsync(user.Value, Roles.Select(x => x.Name));
                }
            }

            Console.WriteLine($"Seed users finished");
        }

        private async Task SeedLanguages(IServiceProvider service)
        {
            var languageRepository = service.GetRequiredService<LanguageRepository>();

            Console.WriteLine("Start seed languages");
            if (!File.Exists(LANGUAGES_FILE))
            {
                Console.WriteLine("No languages file found!");
                return;
            }

            List<Language> languages;

            using (var reader = new StreamReader(LANGUAGES_FILE))
            {
                var json = await reader.ReadToEndAsync();
                languages = JsonConvert.DeserializeObject<List<Language>>(json);
            }

            if (languages == null || !languages.Any())
            {
                Console.WriteLine("No languages in file!");
                return;
            }

            var languagesToAdd = new List<Language>();
            var languagesToDelete = new List<Language>();
            var languagesToUpdate = new List<Language>();

            // all languages
            var allLanguages = await languageRepository.Get(new MRDb.Tools.DbQuery<Language>().Where(x => x.Id != null));

            if (allLanguages != null && allLanguages.Any())
            {
                languagesToDelete = allLanguages.Select(x => x).ToList();
            }

            foreach (var lang in languages)
            {
                var exists = allLanguages.FirstOrDefault(x => x.Code == lang.Code);
                if (exists != null)
                {
                    languagesToDelete.RemoveAll(x => x.Code == exists.Code);

                    // update if changed
                    if (exists.Name != lang.Name || exists.NativeName != lang.NativeName)
                    {
                        exists.Name = lang.Name;
                        exists.NativeName = lang.NativeName;
                        languagesToUpdate.Add(exists);
                    }
                }
                // create if not exists
                else
                {
                    languagesToAdd.Add(new Language
                    {
                        Code = lang.Code,
                        Name = lang.Name,
                        NativeName = lang.NativeName
                    });
                }
            }

            // upload changes to database
            if (languagesToDelete.Any())
            {
                await languageRepository.Remove(languagesToDelete.Select(x => x.Id));
            }

            if (languagesToUpdate.Any())
            {
                await languageRepository.Replace(languagesToUpdate);
            }

            if (languagesToAdd.Any())
            {
                await languageRepository.Insert(languagesToAdd);
            }

            Console.WriteLine("Update languages finished");
            Console.WriteLine($"Total languages: {languages.Count}");
            Console.WriteLine($"Update languages: {languagesToUpdate.Count}");
            Console.WriteLine($"Added languages: {languagesToAdd.Count}");
            Console.WriteLine($"Deleted languages: {languagesToDelete.Count}");
        }

        private async Task SendEmails(IServiceProvider service)
        {
            Console.WriteLine("Setup emails to send");
            var repo = service.GetRequiredService<EmailSendTaskRepository>();
            await repo.Insert(EmailsOnStart);
        }
    }
}
