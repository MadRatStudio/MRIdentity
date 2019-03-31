using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using MigrationRunner.Infrastructure.Attr;
using MigrationRunner.Infrastructure.Enum;
using MigrationRunner.Infrastructure.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MigrationRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            var migrations = FindAllMigrations();
            if(migrations.Count == 0)
            {
                LogAction("No migrations found", LogType.ERROR);
            }
            else
            {
                string input = string.Empty;
                while (true)
                {
                    Console.WriteLine($"Found {migrations.Count} migrations.");
                    Console.WriteLine("Select migration to apply.");
                    Console.WriteLine();

                    for (var i = 0; i < migrations.Count; i++)
                    {
                        var migration = migrations[i];
                        var line = $"{i + 1}. [{migration.CreateDate}] {migration.Name}.";
                        Console.WriteLine(line);
                    }

                    Console.WriteLine();
                    Console.WriteLine("-1. Exit migrations program.");

                    input = Console.ReadLine();

                    if(int.TryParse(input, out int result))
                    {
                        if(result == -1)
                        {
                            break;
                        }
                        else if(result > migrations.Count + 1)
                        {
                            Console.WriteLine($"Migrations with number {result} not found."); 
                        }
                        else
                        {
                            Console.WriteLine($"Migration #{result} started.");

                            try
                            {
                                migrations[result - 1].Migration.Run().Wait();
                                Console.WriteLine($"Migration #{result} finished success.");
                            }
                            catch(Exception ex)
                            {
                                Console.WriteLine($"Migration #${result} failed.");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Bad input.");
                        continue;
                    }

                }
            }

            Console.WriteLine("Migrations exit");
            Console.ReadLine();
        }


        static protected List<MigrationMeta> FindAllMigrations()
        {
            List<MigrationMeta> list = new List<MigrationMeta>();
            var webHost = StartupConfiguration();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    var attribs = type.GetCustomAttributes(typeof(MRMigrationAttribute), false);
                    if (attribs != null && attribs.Length > 0)
                    {
                        var meta = attribs.First() as MRMigrationAttribute;
                        IMigration migration = ((IMigration)Activator.CreateInstance(type)).Init(LogAction, webHost.Services);

                        list.Add(new MigrationMeta
                        {
                            CreateDate = meta.CreatedDate,
                            Name = meta.Name,
                            Migration = migration
                        });
                    }
                }
            }

            return list;
        }

        static protected IWebHost StartupConfiguration() => IdentityApi.Program.BuildWebHost(new string[] { });
        static void LogAction(object source, LogType type)
        {
            Console.WriteLine(source.ToString());
        }
    }

    public class MigrationMeta
    {
        public string Name { get; set; }
        public string CreateDate { get; set; }
        public IMigration Migration { get; set; }
    }
}
