using MongoDB.Driver;
using WiseBlog.Shared.Models;
using Microsoft.JSInterop;
using static System.Runtime.InteropServices.JavaScript.JSType;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using System.Text;
using Supabase.Gotrue;
namespace WiseBlog.Services
{
    public class MongoDBService
    {
        private readonly IMongoCollection<Profile> _profileCollection;
        private readonly IMongoCollection<Blog> _blogCollection;
        private readonly IMongoCollection<Notification> _notificationCollection;
        private readonly IMongoCollection<Summary> _summaryCollection;
        private readonly GridFSBucket _gridFSBucket;
        //public readonly Supabase.Client _supabaseClient;
        //private readonly Secrets _secrets = new Secrets();
        //public readonly IJSRuntime _jsruntime;

        public MongoDBService(IConfiguration configuration)
        {
            var MongoUrl = configuration["MongoDB:URL"];
            var DatabaseName = configuration["MongoDB:DBNAME"];
            var ProfileCollectionName = configuration["MongoDB:PROFILE:COLLECTION"];
            var BlogCollectionName = configuration["MongoDB:BLOG:COLLECTION"];
            var NotificationCollectionName = configuration["MongoDB:NOTIFICATION:COLLECTION"];
            var SummaryCollectionName = configuration["MongoDB:SUMMARY:COLLECTION"];

            //_supabaseClient = supabaseClient;
            //_jsruntime = jsruntime;


            Console.WriteLine("Making Connection: "+ MongoUrl);
            var client = new MongoClient(MongoUrl);
            Console.WriteLine("Connection string works");
            var database = client.GetDatabase(DatabaseName);
            Console.WriteLine("Database got");
            _profileCollection = database.GetCollection<Profile>(ProfileCollectionName);
            _blogCollection = database.GetCollection<Blog>(BlogCollectionName);
            _notificationCollection = database.GetCollection<Notification>(NotificationCollectionName);
            _summaryCollection = database.GetCollection<Summary>(SummaryCollectionName);
            Console.WriteLine("Collection got");
            _gridFSBucket = new GridFSBucket(database);
            Console.WriteLine("GridFS Bucket Created");
        }


        //Profile Operations
        public async Task<Profile> RegisterProfile(Profile request)
        {
            var profile = request;
            Console.WriteLine("Profile Created : " + profile.userId);
            await _profileCollection.InsertOneAsync(profile);
            Console.WriteLine("Inserted");
            var filter = Builders<Profile>.Filter.Eq(task => task.userId, profile.userId);
            var inserted = _profileCollection.Find(filter).FirstOrDefault();
            Console.WriteLine("Inserted Doc : " + inserted);
            return inserted;
        }

        public async Task<Profile> GetProfile(string userId)
        {
            Console.WriteLine("Getting Profile");
            var filter = Builders<Profile>.Filter.Eq(u => u.userId, userId);
            Console.WriteLine("Getting Filter");
            Profile profile = await _profileCollection.Find(filter).FirstOrDefaultAsync();
            Console.WriteLine("Got Profile");
            return profile;
        }

        public async Task<string> ToggleFollow(string userId, string blogUserId, bool toFollow)
        {
            var user = await GetProfile(userId);
            var blogUser = await GetProfile(blogUserId);

            if (user == null || blogUser == null)
                return "Invalid";

            var userFilter = Builders<Profile>.Filter.Eq(u => u.userId, userId);
            var blogUserFilter = Builders<Profile>.Filter.Eq(u => u.userId, blogUserId);

            if (toFollow)
            {
                // Append blogUserId to user's following list
                var userUpdate = Builders<Profile>.Update.AddToSet(u => u.following, blogUserId);
                await _profileCollection.UpdateOneAsync(userFilter, userUpdate);

                // Append userId to blogUser's followers list
                var blogUserUpdate = Builders<Profile>.Update.AddToSet(u => u.followers, userId);
                await _profileCollection.UpdateOneAsync(blogUserFilter, blogUserUpdate);

                Notification notification = new Notification
                {
                    userId = blogUser.userId,
                    userName = blogUser.name,
                    description = $"{user.name} started following you!",
                    redirectTo = $"/profile?id={user.userId}"
                };

                await _notificationCollection.InsertOneAsync(notification);

                return "Started Following";
            }
            else
            {
                // Remove blogUserId from user's following list
                var userUpdate = Builders<Profile>.Update.Pull(u => u.following, blogUserId);
                await _profileCollection.UpdateOneAsync(userFilter, userUpdate);

                // Remove userId from blogUser's followers list
                var blogUserUpdate = Builders<Profile>.Update.Pull(u => u.followers, userId);
                await _profileCollection.UpdateOneAsync(blogUserFilter, blogUserUpdate);

                Notification notification = new Notification
                {
                    userId = blogUser.userId,
                    userName = blogUser.name,
                    description = $"{user.name} unfollowed you!",
                    redirectTo = $"/profile?id={user.userId}"
                };

                await _notificationCollection.InsertOneAsync(notification);

                return "Unfollowed";
            }

        }

        //Summary Operation
        public async Task SaveSummary(Summary summary)
        {
            await _summaryCollection.InsertOneAsync(summary);
            Console.WriteLine($"Summary saved with ID: {summary.Id}");
        }

        public async Task<List<Summary>?> GetSummaries(string userId)
        {
            Console.WriteLine("Getting Notifications for ID : " + userId);
            var filter = Builders<Summary>.Filter.Eq(u => u.userId, userId);
            return await _summaryCollection.Find(filter).ToListAsync();
        }

        public async Task DeleteSummary(string summaryId)
        {
            var filter = Builders<Summary>.Filter.Eq(s => s.Id, summaryId);
            var result = await _summaryCollection.DeleteOneAsync(filter);
        }

        //Notification Operations
        public async Task<List<Notification>?> GetNotifications(string userId)
        {
            Console.WriteLine("Getting Notifications for ID : " + userId);
            var filter = Builders<Notification>.Filter.Eq(u => u.userId, userId);
            return await _notificationCollection
                .Find(filter)
                .SortByDescending(notification => notification.Id) 
                .Limit(5)
                .ToListAsync();
        }

        //Blog Operations
        public async Task<string> UploadBlogToGridFS(Stream blogStream, string contentType)
        {
            Console.WriteLine($"In Mongo Function: Uploading...");

            var options = new GridFSUploadOptions
            {
                Metadata = new BsonDocument { { "contentType", contentType } }
            };

            var blogId = await _gridFSBucket.UploadFromStreamAsync("Blog Content", blogStream, options);

            Console.WriteLine($"In Mongo Function: Uploaded {blogId}");
            return blogId.ToString();
        }


        public async Task InsertBlogDocument(Blog blog)
        {
            await _blogCollection.InsertOneAsync(blog);
            Console.WriteLine($"Blog saved with ID: {blog.contentId}");
        }

        public async Task DeleteBlogDocument(string blogId)
        {
            var filter = Builders<Blog>.Filter.Eq(b => b.Id, blogId);
            var result = await _blogCollection.DeleteOneAsync(filter);
        }

        public async Task<List<Blog>> GetAllBlogs()
        {
            return await _blogCollection.Find(_ => true).ToListAsync();
        }

        public async Task<Blog> GetBlog(string blogId)
        {
            var filter = Builders<Blog>.Filter.Eq(u => u.Id, blogId);
            Blog blog = await _blogCollection.Find(filter).FirstOrDefaultAsync();
            return blog;
        }
        
        public async Task<string> GetProfileImage(string userId)
        {
            var filter = Builders<Profile>.Filter.Eq(u => u.userId, userId);
            var projection = Builders<Profile>.Projection.Expression(p => p.image);

            return await _profileCollection.Find(filter).Project(projection).FirstOrDefaultAsync();
        }

        public async Task<Stream> DownloadFileFromGridFS(string id)
        {
            return await _gridFSBucket.OpenDownloadStreamAsync(ObjectId.Parse(id));
        }
    }
}
