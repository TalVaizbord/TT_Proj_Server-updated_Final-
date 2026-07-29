using TT_Proj_Server.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Metrics;
using System.IO;
using System.Xml.Linq;

namespace TT_Proj_Server.DAL
{
    public class DBServices
    {
        private string connectionString;

        public DBServices()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            connectionString = config.GetConnectionString("RuppinDB");
        }


        public DBServices(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("RuppinDB");
        }

        public SqlConnection Connect()
        {
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            return con;
        }


        public int InsertCountry(Country country)
        {
            int numEffected = 0;
            try
            {
                using (SqlConnection con = Connect())
                {
                    SqlCommand cmd = new SqlCommand("SP_InsertCountry_TT", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", (object)country.Id ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Name", (object)country.Name ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Region", (object)country.Region ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@City", (object)country.City ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Population", country.Population);
                    cmd.Parameters.AddWithValue("@Currency", (object)country.Currency ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Language", (object)country.Language ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Area", country.Area);
                    cmd.Parameters.AddWithValue("@FlagImageUrl", (object)country.FlagImageUrl ?? DBNull.Value);

                    numEffected = cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {


                Console.WriteLine("SQL Error: " + ex.Message);
                throw;
            }
            return numEffected;
        }

        public List<Country> GetAllCountries()
        {
            List<Country> list = new List<Country>();
            using (SqlConnection con = Connect())
            {
                SqlCommand cmd = new SqlCommand("SP_GetAllCountries_TT", con);
                cmd.CommandType = CommandType.StoredProcedure;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Country
                        {
                            Id = reader["Id"].ToString(),
                            Name = reader["Name"]?.ToString() ?? "",
                            Region = reader["Region"]?.ToString() ?? "",
                            City = reader["City"]?.ToString() ?? "",
                            Population = Convert.ToInt64(reader["Population"]),
                            Currency = reader["Currency"]?.ToString() ?? "",
                            Language = reader["Language"]?.ToString() ?? "",
                            Area = Convert.ToDouble(reader["Area"]),
                            FlagImageUrl = reader["FlagImageUrl"]?.ToString() ?? ""
                        });
                    }
                }
            }
            return list;
        }
        public bool UpdateCountry(Country country)
        {
            using (SqlConnection con = Connect())
            {
                SqlCommand cmd = new SqlCommand("SP_UpdateCountry_TT", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", country.Id);
                cmd.Parameters.AddWithValue("@Name", country.Name);
                cmd.Parameters.AddWithValue("@Region", country.Region);
                cmd.Parameters.AddWithValue("@City", country.City);
                cmd.Parameters.AddWithValue("@Population", country.Population);
                cmd.Parameters.AddWithValue("@Currency", country.Currency);
                cmd.Parameters.AddWithValue("@Language", country.Language);
                cmd.Parameters.AddWithValue("@Area", country.Area);
                cmd.Parameters.AddWithValue("@FlagImageUrl", country.FlagImageUrl);

                return cmd.ExecuteNonQuery() > 0;
            }
        }
        public bool DeleteCountry(string id)
        {
            using (SqlConnection con = Connect())
            {
                SqlCommand cmd = new SqlCommand("SP_DeleteCountry_TT", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", id);

                return cmd.ExecuteNonQuery() > 0;
            }
        }
        public List<Country> SearchCountries(string name, string region, string language, string currency, long? minPopulation, double? minArea)
        {
            List<Country> countriesList = new List<Country>();

            using (SqlConnection con = Connect())
            {
                SqlCommand cmd = new SqlCommand("SP_SearchCountries_TT", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Name", (object)name ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Region", (object)region ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Language", (object)language ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Currency", (object)currency ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MinPopulation", (object)minPopulation ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MinArea", (object)minArea ?? DBNull.Value);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Country country = new Country();
                    country.Id = Convert.ToString(reader["Id"]);
                    country.Name = Convert.ToString(reader["Name"]);
                    country.Region = Convert.ToString(reader["Region"]);
                    country.City = Convert.ToString(reader["City"]);
                    country.Population = Convert.ToInt64(reader["Population"]);
                    country.Currency = Convert.ToString(reader["Currency"]);
                    country.Language = Convert.ToString(reader["Language"]);
                    country.Area = Convert.ToDouble(reader["Area"]);
                    country.FlagImageUrl = Convert.ToString(reader["FlagImageUrl"]);
                    countriesList.Add(country);
                }
            }
            return countriesList;
        }
        public int InsertUser(User user)
        {
            using (SqlConnection con = Connect())
            {
                SqlCommand cmd = new SqlCommand("sp_InsertUser_TT", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
                cmd.Parameters.AddWithValue("@LastName", user.LastName);
                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@Password", user.Password);
                cmd.Parameters.AddWithValue("@FavContinent", user.FavContinent);
                cmd.Parameters.AddWithValue("@LanguageProficiency", user.LanguageProficiency);
                cmd.Parameters.AddWithValue("@IsAdmin", user.IsAdmin);

                try
                {
                    return cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception("SQL Error: " + ex.Message);
                }
            }
        }

        public bool UpdateUserProfile(User user)
        {
            using (SqlConnection con = Connect())
            {
                SqlCommand cmd = new SqlCommand("sp_UpdateUserProfile_TT", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
                cmd.Parameters.AddWithValue("@LastName", user.LastName);
                cmd.Parameters.AddWithValue("@Password", user.Password);

                try
                {
                    return cmd.ExecuteNonQuery() > 0;
                }
                catch (Exception ex)
                {
                    throw new Exception("SQL Error: " + ex.Message);
                }
            }
        }
        public User Login(string email, string password)
        {
            using (SqlConnection con = Connect())
            {
                SqlCommand cmd = new SqlCommand("sp_Login_TT", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Password", password);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new User
                        {
                            Email = reader["Email"].ToString(),
                            FirstName = reader["FirstName"].ToString(),
                            LastName = reader["LastName"].ToString(),
                            IsAdmin = Convert.ToBoolean(reader["IsAdmin"]),
                            IsLocked = reader["IsLocked"] != DBNull.Value ? Convert.ToBoolean(reader["IsLocked"]) : false,
                            IsMaster = reader["IsMaster"] != DBNull.Value ? Convert.ToBoolean(reader["IsMaster"]) : false
                        };
                    }
                    return null;
                }
            }
        }



        public List<Share> GetSharesByCountry(string countryId)
        {
            List<Share> sharesList = new List<Share>();
            using (SqlConnection con = Connect())
            {
                SqlCommand cmd = new SqlCommand("SP_GetSharesByCountry_TT", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CountryId", countryId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sharesList.Add(new Share
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            CountryId = reader["CountryId"].ToString(),
                            UserEmail = reader["UserEmail"].ToString(),
                            UserName = reader["UserName"].ToString(),
                            Content = reader["Content"].ToString(),
                            DatePosted = Convert.ToDateTime(reader["DatePosted"])
                        });
                    }
                }
            }
            return sharesList;
        }

        public int InsertShare(Share share)
        {
            int numEffected = 0;
            using (SqlConnection con = Connect())
            {
                SqlCommand cmd = new SqlCommand("SP_InsertShare_TT", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@CountryId", share.CountryId);
                cmd.Parameters.AddWithValue("@UserEmail", share.UserEmail);
                cmd.Parameters.AddWithValue("@UserName", share.UserName);
                cmd.Parameters.AddWithValue("@Content", share.Content);

                numEffected = cmd.ExecuteNonQuery();
            }
            return numEffected;
        }

        public bool UpdateShare(int id, string content)
        {
            using (SqlConnection con = Connect())
            {
                SqlCommand cmd = new SqlCommand("SP_UpdateShare_TT", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Content", content);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool DeleteShare(int id)
        {
            using (SqlConnection con = Connect())
            {
                SqlCommand cmd = new SqlCommand("SP_DeleteShare_TT", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", id);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool AddPointsToUser(string email, int pointsToAdd)
        {
            using (SqlConnection con = Connect())
            {
                SqlCommand cmd = new SqlCommand("SP_AddPointsToUser_TT", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@PointsToAdd", pointsToAdd);

                return cmd.ExecuteNonQuery() > 0;
            }
        }



        public void LogUserLogin(string email)
        {
            using (SqlConnection con = Connect())
            {
                SqlCommand cmd = new SqlCommand("SP_LogUserLogin_TT", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.ExecuteNonQuery();
            }
        }

        public List<User> GetAllUsersForAdmin()
        {
            List<User> list = new List<User>();
            using (SqlConnection con = Connect())
            {
                SqlCommand cmd = new SqlCommand("SP_GetAllUsersForAdmin_TT", con);
                cmd.CommandType = CommandType.StoredProcedure;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new User
                        {
                            FirstName = reader["FirstName"].ToString(),
                            LastName = reader["LastName"].ToString(),
                            Email = reader["Email"].ToString(),
                            IsAdmin = Convert.ToBoolean(reader["IsAdmin"]),
                            IsLocked = reader["IsLocked"] != DBNull.Value ? Convert.ToBoolean(reader["IsLocked"]) : false,
                            IsMaster = reader["IsMaster"] != DBNull.Value ? Convert.ToBoolean(reader["IsMaster"]) : false
                        });
                    }
                }
            }
            return list;
        }

        public bool UpdateUserLockStatus(string email, bool isLocked)
        {
            using (SqlConnection con = Connect())
            {
                SqlCommand cmd = new SqlCommand("SP_ToggleUserLock_TT", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IsLocked", isLocked);
                cmd.Parameters.AddWithValue("@Email", email);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool UpdateUserAdminStatus(string email, bool isAdmin)
        {
            using (SqlConnection con = Connect())
            {
                SqlCommand cmd = new SqlCommand("SP_ToggleUserAdmin_TT", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IsAdmin", isAdmin);
                cmd.Parameters.AddWithValue("@Email", email);
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public object GetAdminStatistics()
        {
            using (SqlConnection con = Connect())
            {
                int totalCountries = 0, totalShares = 0, dailyLogins = 0;

                SqlCommand cmd = new SqlCommand("SP_GetAdminStatistics_TT", con);
                cmd.CommandType = CommandType.StoredProcedure;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read()) 
                    {
                        totalCountries = Convert.ToInt32(reader["TotalCountries"]);
                        totalShares = Convert.ToInt32(reader["TotalShares"]);
                        dailyLogins = Convert.ToInt32(reader["DailyLogins"]);
                    }
                }

                return new
                {
                    TotalCountries = totalCountries,
                    TotalShares = totalShares,
                    DailyLogins = dailyLogins
                };
            }
        }

        public class LeaderboardEntry
        {
            public string FullName { get; set; }
            public int Score { get; set; }
        }

        public void UpdateHighScore(string email, string quizType, int newScore)
        {
            using (SqlConnection con = Connect())
            {
                SqlCommand cmd = new SqlCommand("sp_UpdateHighScore_TT", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@QuizType", quizType);
                cmd.Parameters.AddWithValue("@NewScore", newScore);

                cmd.ExecuteNonQuery();
            }
        }

        public List<LeaderboardEntry> GetTop10Scores(string quizType)
        {
            List<LeaderboardEntry> list = new List<LeaderboardEntry>();
            using (SqlConnection con = Connect())
            {
                SqlCommand cmd = new SqlCommand("sp_GetTop10Scores_TT", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@QuizType", quizType);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new LeaderboardEntry
                        {
                            FullName = reader["FirstName"].ToString() + " " + reader["LastName"].ToString(),
                            Score = Convert.ToInt32(reader["Score"])
                        });
                    }
                }
            }
            return list;
        }

        public Dictionary<string, List<string>> GetUserLists(string email)
        {
            Dictionary<string, List<string>> userLists = new Dictionary<string, List<string>>()
    {
        { "מועדפים ❤️", new List<string>() },
        { "יעדים לטיסה ✈️", new List<string>() },
        { "הייתי שם 🌍", new List<string>() }
    };

            using (SqlConnection con = Connect())
            {
                SqlCommand cmd = new SqlCommand("SP_GetUserLists_TT", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Email", email);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string countryId = reader["CountryId"].ToString();
                        string listName = reader["ListName"].ToString();

                        if (userLists.ContainsKey(listName))
                        {
                            userLists[listName].Add(countryId);
                        }
                    }
                }
            }
            return userLists;
        }

        public bool SaveUserLists(string email, Dictionary<string, List<string>> lists)
        {
            using (SqlConnection con = Connect())
            {
                SqlCommand cmdClear = new SqlCommand("SP_ClearUserLists_TT", con);
                cmdClear.CommandType = CommandType.StoredProcedure;
                cmdClear.Parameters.AddWithValue("@Email", email);
                cmdClear.ExecuteNonQuery();

                foreach (var list in lists)
                {
                    string listName = list.Key;
                    foreach (var countryId in list.Value)
                    {
                        SqlCommand cmdInsert = new SqlCommand("SP_InsertUserList_TT", con);
                        cmdInsert.CommandType = CommandType.StoredProcedure;
                        cmdInsert.Parameters.AddWithValue("@Email", email);
                        cmdInsert.Parameters.AddWithValue("@CountryId", countryId);
                        cmdInsert.Parameters.AddWithValue("@ListName", listName);

                        cmdInsert.ExecuteNonQuery();
                    }
                }
                return true;
            }
        }
    }
}