using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace MyFirstEfCoreApp
{
    public static class Commands
    {
        public static void ListAll()
        {
            using (var db = new AppDbContext())
            {
                foreach (var book in db.Books.AsNoTracking()
                                             .Include(a => a.Author))
                {
                    var webUrl = book.Author.WebUrl ?? "- no web url given -";
                    Console.WriteLine($"{book.Title} by {book.Author.Name}");
                    Console.WriteLine("    Published on " + $"{book.PublishedOn:dd-MMM-yyyy}. {webUrl}");
                }

            }
        }

        public static void ChangeWebUrl()
        {
            Console.WriteLine("New Quantum Neworking WebUrl > ");
            var newWebUrl = Console.ReadLine();

            using (var db = new AppDbContext())
            {
                var book = db.Books
                             .Include(b => b.Author)
                             .Single(b => b.Title == "Quantum Networking");

                book.Author.WebUrl = newWebUrl;
                db.SaveChanges();
                Console.WriteLine("... SaveChanges called.");

            }

            ListAll();
        }

        public static void ListAllWithLogs()
        {
            var logs = new List<string>();
            using (var db = new AppDbContext())
            {
                var serviceProvider = db.GetInfrastructure();
                var loggerFactory = (ILoggerFactory)serviceProvider.GetService(typeof(ILoggerFactory));
                loggerFactory.AddProvider(new MyLoggerProvider(logs));

                foreach (var book in db.Books.AsNoTracking()
                                             .Include(b => b.Author))
                {
                    var webUrl = book.Author.WebUrl ?? "- no web url given -";
                    Console.WriteLine($"{book.Title} by {book.Author.Name}");
                    Console.WriteLine("    Published on " + $"{book.PublishedOn:dd-MMM-yyyy}. {webUrl}");
                }
            }

            Console.WriteLine("---------- LOGS ---------------");
            foreach (var log in logs)
            {
                Console.WriteLine(log);
            }
        }

        public static void ChangeWebUrlWithLogs()
        {
            var logs = new List<string>();
            Console.WriteLine("New Quantum Neworking WebUrl > ");
            var newWebUrl = Console.ReadLine();

            using (var db = new AppDbContext())
            {
                var serviceProvider = db.GetInfrastructure();
                var loggerFactory = (ILoggerFactory)serviceProvider.GetService(typeof(ILoggerFactory));
                loggerFactory.AddProvider(new MyLoggerProvider(logs));

                var book = db.Books
                             .Include(b => b.Author)
                             .Single(b => b.Title == "Quantum Networking");

                book.Author.WebUrl = newWebUrl;
                db.SaveChanges();
                Console.WriteLine("... SaveChanges called.");

                Console.WriteLine("---------- LOGS ---------------");
                foreach (var log in logs)
                {
                    Console.WriteLine(log);
                }

                ListAll();
            }
        }

        public static bool WipeCreateSeed(bool onlyIfNoDatabase)
        {
            using (var db = new AppDbContext())
            {
                if (onlyIfNoDatabase &&
                    (db.GetService<IDatabaseCreator>() as RelationalDatabaseCreator).Exists()
                   )
                {
                    return false;
                }

                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                if (!db.Books.Any())
                {
                    db.WriteTestData();
                    Console.WriteLine("Seeded database");
                }
            }

            return true;
        }

        public static void WriteTestData(this AppDbContext db)
        {
            var martinFowler = new Author
            {
                Name = "Martin Fowler",
                WebUrl = "http://martinfowler.com"
            };

            var books = new List<Book>
            {
                new Book
                {
                    Title = "Refactoring",
                    Description = "Improving the design of existing code",
                    PublishedOn = new DateTime(1999, 7, 8),
                    Author = martinFowler
                },
                new Book
                {
                    Title = "Patterns of Enterprise Application Architecture",
                    Description = "Written in direct response to the stiff challenges",
                    PublishedOn = new DateTime(2002, 11, 15),
                    Author = martinFowler
                },
                new Book
                {
                    Title = "Domain-Driven Design",
                    Description = "Linking business needs to software design",
                    PublishedOn = new DateTime(2003, 8, 30),
                    Author = new Author { Name = "Eric Evans", WebUrl = "http://domainlanguage.com/"}
                },
                new Book
                {
                    Title = "Quantum Networking",
                    Description = "Entangled quantum networking provides faster-than-light data communications",
                    PublishedOn = new DateTime(2057, 1, 1),
                    Author = new Author { Name = "Future Person"}
                }
            };

            db.Books.AddRange(books);
            db.SaveChanges();
        }
    }
}
