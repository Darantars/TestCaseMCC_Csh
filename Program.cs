using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello World!");

        var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

        var context = new MyDbContext(loggerFactory);
        context.Database.EnsureCreated();
        InitializeData(context);

        Console.WriteLine("All posts:");
        var data = context.BlogPosts.Select(x => x.Title).ToList();
        Console.WriteLine(JsonSerializer.Serialize(data));
            
            
        Console.WriteLine("How many comments each user left:");
        //ToDo: write a query and dump the data to console
        // Expected result (format could be different, e.g. object serialized to JSON is ok):
        // Ivan: 4
        // Petr: 2
        // Elena: 3

        Console.WriteLine(JsonSerializer.Serialize(BlogService.NumberOfCommentsPerUser(context)));


        Console.WriteLine("Posts ordered by date of last comment. Result should include text of last comment:");
        //ToDo: write a query and dump the data to console
        // Expected result (format could be different, e.g. object serialized to JSON is ok):
        // Post2: '2020-03-06', '4'
        // Post1: '2020-03-05', '8'
        // Post3: '2020-02-14', '9'

        Console.WriteLine(JsonSerializer.Serialize(BlogService.PostsOrderedByLastCommentDate(context)));

        Console.WriteLine("How many last comments each user left:");
        // 'last comment' is the latest Comment in each Post
        //ToDo: write a query and dump the data to console
        // Expected result (format could be different, e.g. object serialized to JSON is ok):
        // Ivan: 2
        // Petr: 1

        Console.WriteLine(JsonSerializer.Serialize(BlogService.NumberOfLastCommentsLeftByUser(context)));

    }

    private static void InitializeData(MyDbContext context)
    {
        context.BlogPosts.Add(new BlogPost("Post1")
        {
            Comments = new List<BlogComment>()
            {
                new BlogComment("1", new DateTime(2020, 3, 2), "Petr"),
                new BlogComment("2", new DateTime(2020, 3, 4), "Elena"),
                new BlogComment("8", new DateTime(2020, 3, 5), "Ivan"),
            }
        });
        context.BlogPosts.Add(new BlogPost("Post2")
        {
            Comments = new List<BlogComment>()
            {
                new BlogComment("3", new DateTime(2020, 3, 5), "Elena"),
                new BlogComment("4", new DateTime(2020, 3, 6), "Ivan"),
            }
        });
        context.BlogPosts.Add(new BlogPost("Post3")
        {
            Comments = new List<BlogComment>()
            {
                new BlogComment("5", new DateTime(2020, 2, 7), "Ivan"),
                new BlogComment("6", new DateTime(2020, 2, 9), "Elena"),
                new BlogComment("7", new DateTime(2020, 2, 10), "Ivan"),
                new BlogComment("9", new DateTime(2020, 2, 14), "Petr"),
            }
        });
        context.SaveChanges();
    }

    public class BlogService
    {
        /// <summary>
        /// Метод, возвращающий запрос, который группирует комментарии по имени пользователя и возвращает количество комментариев для каждого пользователя и его имя.
        /// </summary>
        /// <param name="context">Контекст базы данных.</param>
        /// <returns>Запрос, который группирует комментарии по имени пользователя и возвращает количество комментариев для каждого пользователя и его имя.</returns>
        public static IQueryable<dynamic> NumberOfCommentsPerUser(MyDbContext context)
        {
            return context.BlogComments
                .GroupBy(c => c.UserName)
                .Select(g => new { UserName = g.Key, UserCommentsCount = g.Count() });
        }

        /// <summary>
        /// Метод, возвращающий запрос, который сортирует посты по дате последнего комментария и возвращает информацию об их заголовке и тексте последнего комментария.
        /// </summary>
        /// <param name="context">Контекст базы данных.</param>
        /// <returns>Запрос, который сортирует посты по дате последнего комментария и возвращает информацию об их заголовке и тексте последнего комментария.</returns>
        public static IQueryable<dynamic> PostsOrderedByLastCommentDate(MyDbContext context)
        {
            return context.BlogPosts
                .Select(p => new
                {
                    PostName = p.Title,
                    LastComment = p.Comments.OrderByDescending(c => c.CreatedDate).FirstOrDefault()
                })
                .Where(p => p.LastComment != null)
                .OrderByDescending(p => p.LastComment.CreatedDate)
                .Select(p => new
                {
                    p.PostName,
                    LastPostCommentDate = p.LastComment.CreatedDate.ToShortDateString(),
                    LastPostCommentText = p.LastComment.Text
                });
        }

        /// <summary>
        /// Метод, возвращающий запрос, который возвращает количество последних комментариев под постом для каждого пользователя.
        /// </summary>
        /// <param name="context">Контекст базы данных.</param>
        /// <returns>Запрос, который возвращает количество последних комментариев под постом для каждого пользователя.</returns>
        public static IQueryable<dynamic> NumberOfLastCommentsLeftByUser(MyDbContext context)
        {
            return context.BlogPosts
                .SelectMany(p => p.Comments.OrderByDescending(c => c.CreatedDate).Take(1),
                    (p, c) => new { c.UserName })
                .GroupBy(c => c.UserName)
                .Select(g => new { UserName = g.Key, UserLastPostCommentsCount = g.Count() });
        }
    }
}
