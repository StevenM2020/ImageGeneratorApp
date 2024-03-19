// Class:       Storage
// Author:      Steven Motz
// Date:        03/18/2024
// Description: This class contains functions for storing and retrieving data from the database. It contains functions for creating a user, checking if a user exists, checking if a password is correct, saving the user's ID, getting the user's email, saving an image, and getting images.
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ImageGeneratorApp
{
    internal static class Storage
    {
        const int keySize = 32;
        const int iterations = 350000;

        // stores a value in the secure storage
        public static async void SetSecureStorage(string key, string value)
        {
            await SecureStorage.SetAsync(key, value);
        }

        public static async void SetRememberUser(bool value)
        {
            SetSecureStorage("RememberUser", value.ToString());
        }

        public static async Task<bool> GetRememberUser()
        {
            string? strRememberUser = await GetSecureStorage("RememberUser");
            if (strRememberUser == null)
                return false;
            return Convert.ToBoolean(strRememberUser);
        }

        // retrieves a value from the secure storage
        public static async Task<string?> GetSecureStorage(string key)
        {
            try
            {
                return await SecureStorage.GetAsync(key);
            }
            catch (Exception e)
            {
                if (App.DebugMessagesOn())
                    Debug.WriteLine(e);
                return null;
            }
        }

        // adds a new user to the database
        internal static async void CreateUser(string strEmail, string strPassword)
        {
            var salt = RandomNumberGenerator.GetBytes(keySize);
            var hash = HashString(strPassword, salt);
            try
            {
                var cs = await GetSecureStorage("ConnectionString");
                using (var context = new ImageGeneratorDbContext(cs))
                {
                    var newUser = new User { Email = strEmail, Password = hash, Salt = Convert.ToBase64String(salt) };
                    context.User.Add(newUser);
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                if (App.DebugMessagesOn())
                    Debug.WriteLine(e);
            }
        }

        // checks if an email already exists in the database
        internal static async Task<bool> CheckUserExists(string strEmail)
        {
            try
            {
                var cs = await GetSecureStorage("ConnectionString");
                using (var context = new ImageGeneratorDbContext(cs))
                {
                    var user = await context.User.FirstOrDefaultAsync(u => u.Email == strEmail);
                    if (user != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                if (App.DebugMessagesOn())
                    Debug.WriteLine(e);
                return false;
            }
        }

        // checks if the password is correct for the given email
        internal static async Task<bool> CheckPassword(string strEmail, string strPassword)
        {
            try
            {
                var cs = await GetSecureStorage("ConnectionString");
                using (var context = new ImageGeneratorDbContext(cs))
                {
                    // Retrieve the user from the database based on the email
                    var user = await context.User.FirstOrDefaultAsync(u => u.Email == strEmail);
                    if (user != null)
                    {
                        byte[] storedSalt = Convert.FromBase64String(user.Salt);

                        // Hash the submitted password using the same function and salt
                        string hash = HashString(strPassword, storedSalt);

                        return hash == user.Password;
                    }
                    else
                    {
                        // User not found
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                if (App.DebugMessagesOn())
                    Debug.WriteLine(e);
                return false;
            }
        }

        // uses the email to retrieve the user's ID and store it in the secure storage
        internal static async void SaveUserID(string strEmail)
        {
            try
            {
                var cs = await GetSecureStorage("ConnectionString");
                using (var context = new ImageGeneratorDbContext(cs))
                {
                    var user = await context.User.FirstOrDefaultAsync(u => u.Email == strEmail);
                    if (user != null)
                    {
                        SetSecureStorage("UserID", user.Id.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                if (App.DebugMessagesOn())
                    Debug.WriteLine(e);
            }
        }

        // get the users email from the database
        internal static async Task<string?> GetEmail()
        {
            try
            {
                var cs = await GetSecureStorage("ConnectionString");
                int id = Convert.ToInt32(await GetSecureStorage("UserID"));
                using (var context = new ImageGeneratorDbContext(cs))
                {
                    var user = await context.User.FirstOrDefaultAsync(u => u.Id == id);
                    if (user != null)
                    {
                        return user.Email;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                if (App.DebugMessagesOn())
                    Debug.WriteLine(e);
                return null;
            }
        }

        internal static async void SaveRememberUser()
        {
            return;
        }

        // hashes a string using the PBKDF2 algorithm
        internal static string HashString(string strPassword, byte[] salt)
        {
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(strPassword),
                salt,
                iterations,
                HashAlgorithmName.SHA512,
                keySize);

            return Convert.ToHexString(hash);
        }
        //https://code-maze.com/csharp-hashing-salting-passwords-best-practices/

        internal static async void SaveImage(string strImage)
        {
            try
            {
                var user_id = Convert.ToInt32(await GetSecureStorage("UserID"));
                if (user_id == null)
                    return;

                var cs = await GetSecureStorage("ConnectionString");
                using (var context = new ImageGeneratorDbContext(cs))
                {
                    var user = await context.User.FirstOrDefaultAsync(u =>
                        u.Id == Convert.ToInt32(user_id));
                    if (user != null)
                    {
                        var newImage = new ImageTable { Image = strImage, User_ID = user.Id };
                        context.Image.Add(newImage);
                        await context.SaveChangesAsync();
                        Debug.WriteLine("Image saved");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (App.DebugMessagesOn())
                    Debug.WriteLine(e);
            }
        }

        internal static async Task<string[]?> GetImages()
        {
            try
            {
                var user_id = Convert.ToInt32(await GetSecureStorage("UserID"));
                if (user_id == null)
                    return null;

                var cs = await GetSecureStorage("ConnectionString");
                using (var context = new ImageGeneratorDbContext(cs))
                {
                    int id = Convert.ToInt32(user_id);
                    var images = await context.Image.Where(i => i.User_ID == id).ToArrayAsync();
                    if (images != null)
                    {
                        string[] strImages = new string[images.Length];
                        for (int i = 0; i < images.Length; i++)
                        {
                            strImages[i] = images[i].Image;
                        }

                        return strImages;
                    }
                    else
                    {
                        if (App.DebugMessagesOn())
                            Debug.WriteLine("No images found");
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                if (App.DebugMessagesOn())
                    Debug.WriteLine(e);
                return null;
            }
        }


        public class User
        {
            public int Id { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string Salt { get; set; }
        }

        public class ImageTable
        {
            public int Id { get; set; }
            public string Image { get; set; }
            public int User_ID { get; set; }
        }


        public class ImageGeneratorDbContext : DbContext
        {
            private string connectionString;

            public ImageGeneratorDbContext(string strConnection)
            {
                connectionString = strConnection;
            }
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlServer(connectionString);
            }

            public DbSet<User> User { get; set; }
            public DbSet<ImageTable> Image { get; set; }
        }


        //https://learn.microsoft.com/en-us/ef/core/querying/sql-queries
        //https://dotnettutorials.net/lesson/linq-to-entities-in-entity-framework-core/

    }
}
