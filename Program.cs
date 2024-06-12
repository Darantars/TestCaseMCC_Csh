using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.Logging;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

            Console.WriteLine(
             JsonSerializer.Serialize(BlogService.NumberOfCommentsPerUser(context)));




        Console.WriteLine("Posts ordered by date of last comment. Result should include text of last comment:");
        //ToDo: write a query and dump the data to console
        // Expected result (format could be different, e.g. object serialized to JSON is ok):
        // Post2: '2020-03-06', '4'
        // Post1: '2020-03-05', '8'
        // Post3: '2020-02-14', '9'

        Console.WriteLine(
            JsonSerializer.Serialize(BlogService.PostsOrderedByLastCommentDate(context)));

        Console.WriteLine("How many last comments each user left:");
        // 'last comment' is the latest Comment in each Post
        //ToDo: write a query and dump the data to console
        // Expected result (format could be different, e.g. object serialized to JSON is ok):
        // Ivan: 2
        // Petr: 1

        Console.WriteLine(
            JsonSerializer.Serialize(BlogService.NumberOfLastCommentsLeftByUser(context)));

    }

    public class BlogService
    {
        public static IQueryable NumberOfCommentsPerUser(MyDbContext context)
        {
            IQueryable myFirstQuery = (from comments in context.BlogComments //Можно было добавить <dynamic>, но задание не подорузумевает неккоректных данных другого из БД
                                group comments by comments.UserName into dump
                                select new { UserName = dump.Key, Count = dump.Count()});
            return myFirstQuery;
        }

        public static IQueryable PostsOrderedByLastCommentDate(MyDbContext context)
        {
            IQueryable mySecondQuery = (from posts in context.BlogPosts
                                 let lastComment = posts.Comments.OrderByDescending(c => c.CreatedDate).FirstOrDefault()
                                 let lastCommentDate = lastComment.CreatedDate.Date
                                 orderby lastCommentDate descending
                                 select new { PostName = posts.Title, LastCommentDate = lastCommentDate, LastCommentText = lastComment.Text });
            return mySecondQuery;
        }

        public static IQueryable NumberOfLastCommentsLeftByUser(MyDbContext context)
        {
            IQueryable myThirdQuery = (from posts in context.BlogPosts
                                        let lastComment = posts.Comments.OrderByDescending(c => c.CreatedDate).FirstOrDefault()
                                        let lastCommentDate = lastComment.CreatedDate.Date
                                        orderby lastCommentDate descending
                                        select new { PostName = posts.Title, LastCommentDate = lastCommentDate, LastCommentText = lastComment.Text });
            return myThirdQuery;
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
}