﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using PockeTwit;
using PockeTwit.Library;
using Yedda;

namespace LocalStorage
{
    internal class DataBaseUtility
    {
        #region SQL Constants

        private const string SQLCountFromCache =
            @"SELECT     COUNT(id) AS newItems
                          FROM         statuses WHERE timestamp>(SELECT timestamp FROM statuses WHERE id=@id) AND ";

        private const string SQLFetchDirects = "(statuses.statustypes & 2)";

        private const string SQLFetchFriends = "(statuses.statustypes == 0)";

        private const string SQLFetchFromCache =
            @"SELECT     statuses.id, statuses.fulltext, statuses.userid, statuses.[timestamp], 
                                     statuses.in_reply_to_id, statuses.favorited, statuses.clientSource, 
                                     statuses.accountSummary, statuses.statustypes, users.screenname, 
                                     users.fullname, users.description, users.avatarURL, statuses.statustypes
                          FROM       statuses INNER JOIN users ON statuses.userid = users.id";

        private const string SQLFetchReplies = "(statuses.statustypes & 1)";

        private const string SQLFetchRepliesAndMessages = " (statuses.statustypes & 1 OR statuses.statustypes & 2) ";

        private const string SQLFetchSearches = "(statuses.statustypes & 4)";

        private const string SQLGetLastStatusID =
            @"SELECT    statuses.id
                          FROM statuses 
                          WHERE statuses.accountSummary = @accountsummary";

        private const string SQLIgnoreGrouped =
            //@" INNER JOIN usersInGroups ON statuses.userid <> usersInGroups.userid ";
            @" AND ((SELECT COUNT(id) FROM usersInGroups WHERE usersInGroups.userid=statuses.userid ) = 0)  ";

        private const string SQLLimit = " LIMIT @count ";
        private const string SQLOrder = " ORDER BY statuses.[timestamp] DESC ";

        #endregion

        private const string DBVersion = "0007";
        private static readonly string DBPath = ClientSettings.AppPath + "\\LocalStorage\\LocalCache.db";

        public static void CheckDBSchema()
        {
            try
            {
                using (SQLiteConnection conn = GetConnection())
                {
                    conn.Open();
                    using (var comm = new SQLiteCommand(conn))
                    {
                        comm.CommandText = "SELECT value from DBProperties WHERE name='dbversion'";
                        var versionNum = (string) comm.ExecuteScalar();

                        if (versionNum == DBVersion)
                        {
                            return;
                        }
                    }
                    conn.Close();
                }
            }
            catch (SQLiteException)
            {
            }

            DeleteDB();
        }

        private static void DeleteDB()
        {
            if (File.Exists(DBPath))
            {
                File.Delete(DBPath);
            }
        }

        public static SQLiteConnection GetConnection()
        {
            if (!File.Exists(DBPath))
            {
                CreateDB();
            }
            return new SQLiteConnection("Data Source=" + ClientSettings.AppPath + "\\LocalStorage\\LocalCache.db");
        }

        //Update this number if you change the schema of the database -- it'll
        // force the client to recreate it.

        public static void CreateDB()
        {
            if (!Directory.Exists(ClientSettings.AppPath + "\\LocalStorage"))
            {
                Directory.CreateDirectory(ClientSettings.AppPath + "\\LocalStorage");
            }
            if (!File.Exists(DBPath))
            {
                SQLiteConnection.CreateFile(DBPath);
            }
            try
            {
                using (SQLiteConnection conn = GetConnection())
                {
                    using (var comm = new SQLiteCommand(conn))
                    {
                        conn.Open();
                        SQLiteTransaction t = conn.BeginTransaction();

                        comm.CommandText =
                            @"CREATE TABLE IF NOT EXISTS DBProperties (name VARCHAR(50) PRIMARY KEY,
                            value NVARCHAR(255))
                            ";
                        comm.ExecuteNonQuery();

                        try
                        {
                            comm.CommandText =
                                @"INSERT INTO DBProperties (name,value) VALUES (@name,@value)";
                            comm.Parameters.Add(new SQLiteParameter("@name", "dbversion"));
                            comm.Parameters.Add(new SQLiteParameter("@value", DBVersion));
                            comm.ExecuteNonQuery();
                            comm.Parameters.Clear();
                        }
                        catch (SQLiteException)
                        {
                        }
                        comm.CommandText =
                            @"CREATE TABLE IF NOT EXISTS statuses (id VARCHAR(50) PRIMARY KEY,
                            fulltext NVARCHAR(255),
                            userid VARCHAR(50),
                            timestamp DATETIME,
                            in_reply_to_id VARCHAR(50),
                            favorited BIT,
                            clientSource VARCHAR(50),
                            accountSummary VARCHAR(50),
                            statustypes SMALLINT(2),
                            UNIQUE (id) )
                                       ";
                        comm.ExecuteNonQuery();

                        comm.CommandText =
                            @"CREATE INDEX IF NOT EXISTS DateINDEX ON statuses (timestamp DESC)";
                        comm.ExecuteNonQuery();
                        
                        comm.CommandText =
                            @"CREATE TABLE IF NOT EXISTS users (id VARCHAR(50) PRIMARY KEY,
                            screenname NVARCHAR(255),
                            fullname NVARCHAR(255),
                            description NVARCHAR(255),
                            avatarURL VARCHAR(255),
                            UNIQUE (id) )
                                       ";
                        comm.ExecuteNonQuery();

                        comm.CommandText =
                            @"CREATE TABLE IF NOT EXISTS groups (groupname NVARCHAR(50) PRIMARY KEY ON CONFLICT IGNORE)";
                        comm.ExecuteNonQuery();

                        comm.CommandText =
                            @"CREATE TABLE IF NOT EXISTS usersInGroups (id NVARCHAR(100) PRIMARY KEY ON CONFLICT IGNORE,
                            groupname NVARCHAR(50),
                            userid VARCHAR(50))";
                        comm.ExecuteNonQuery();

                        comm.CommandText =
                            @"CREATE  TABLE  IF NOT EXISTS avatarCache 
                                    (avatar BLOB NOT NULL,
                                     url VARCHAR(255) PRIMARY KEY NOT NULL )";
                        comm.ExecuteNonQuery();

                        comm.CommandText = "PRAGMA locking_mode=EXCLUSIVE; ";
                        comm.ExecuteNonQuery();

                        comm.CommandText = "PRAGMA journal_mode = OFF; ";
                        comm.ExecuteNonQuery();

                        t.Commit();
                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        public static List<status> GetList(TimelineManagement.TimeLineType typeToGet, int Count)
        {
            string Constraints = null;
            if (typeToGet == TimelineManagement.TimeLineType.Friends)
            {
                Constraints = SQLIgnoreGrouped;
            }
            return GetList(typeToGet, Count, Constraints);
        }

        public static List<status> GetList(TimelineManagement.TimeLineType typeToGet, int Count, string Constraints)
        {
            var cache = new List<status>();
            using (SQLiteConnection conn = GetConnection())
            {
                string FetchQuery = SQLFetchFromCache;

                FetchQuery = FetchQuery + " WHERE " + AddTypeWhereClause(typeToGet) + Constraints + SQLOrder + SQLLimit;

                using (var comm = new SQLiteCommand(FetchQuery, conn))
                {
                    comm.Parameters.Add(new SQLiteParameter("@count", Count));
                    conn.Open();
                    using (SQLiteDataReader r = comm.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var newStat = new status
                                              {
                                                  id = r.GetString(0),
                                                  text = r.GetString(1),
                                                  TypeofMessage = ((StatusTypes) r.GetInt32(13)),
                                                  createdAt = r.GetDateTime(3),
                                                  in_reply_to_status_id = r.GetString(4),
                                                  favorited = r.GetString(5),
                                                  source = r.GetString(6),
                                                  AccountSummary = r.GetString(7)
                                              };

                            var u = new User
                                        {
                                            id = r.GetString(2),
                                            screen_name = r.GetString(9),
                                            name = r.GetString(10),
                                            description = r.GetString(11),
                                            profile_image_url = r.GetString(12)
                                        };

                            newStat.user = u;
                            cache.Add(newStat);
                        }
                    }
                    conn.Close();
                }
            }
            return cache;
        }

        private static string AddTypeWhereClause(TimelineManagement.TimeLineType typeToGet)
        {
            switch (typeToGet)
            {
                case TimelineManagement.TimeLineType.Friends:
                    return SQLFetchFriends;
                case TimelineManagement.TimeLineType.Messages:
                    return SQLFetchRepliesAndMessages;
            }
            return null;
        }

        public static int CountItemsNewerThan(TimelineManagement.TimeLineType typeToGet, string ID,
                                            string Constraints)
        {
            if (ID == null)
            {
                return 0;
            }
            using (SQLiteConnection conn = GetConnection())
            {
                string FetchQuery = SQLCountFromCache;
                string midClause = Constraints + AddTypeWhereClause(typeToGet);
                FetchQuery = FetchQuery + midClause + SQLOrder;
                using (var comm = new SQLiteCommand(FetchQuery, conn))
                {
                    comm.Parameters.Add(new SQLiteParameter("@id", ID));
                    conn.Open();
                    object o = comm.ExecuteScalar();
                    return Convert.ToInt32(o);
                }
            }
        }


        public static void CleanDB(int OlderThan)
        {
            DateTime SinceDate = DateTime.Now.AddDays(1);
            if (OlderThan > 0)
            {
                SinceDate = DateTime.Now.Subtract(new TimeSpan(OlderThan, 0, 0, 0));
            }
            using (SQLiteConnection conn = GetConnection())
            {
                conn.Open();
                using (SQLiteTransaction t = conn.BeginTransaction())
                {
                    using (var comm = new SQLiteCommand(conn))
                    {
                        comm.CommandText = "DELETE FROM statuses WHERE timestamp<@SinceDay";
                        comm.Parameters.Add(new SQLiteParameter("@SinceDay", SinceDate));
                        int results = comm.ExecuteNonQuery();
                        comm.Parameters.Clear();

                        comm.CommandText =
                            @"DELETE FROM users WHERE id NOT IN (
                                                SELECT DISTINCT userid FROM statuses
                                                )";
                        results = comm.ExecuteNonQuery();
                    }
                    t.Commit();
                }
                conn.Close();
            }
        }

        public static void SaveItems(List<status> TempLine)
        {
            using (SQLiteConnection conn = GetConnection())
            {
                conn.Open();
                SQLiteTransaction t = conn.BeginTransaction();
                foreach (status status in TempLine)
                {
                    status.Save(conn);
                }
                t.Commit();
                conn.Close();
            }
        }

        public static string GetLatestItem(Twitter.Account account, TimelineManagement.TimeLineType typeToGet)
        {
            string FetchQuery = SQLGetLastStatusID;
            switch (typeToGet)
            {
                case TimelineManagement.TimeLineType.Friends:
                    FetchQuery = FetchQuery + " AND " + SQLFetchFriends + SQLOrder + SQLLimit;
                    break;
                case TimelineManagement.TimeLineType.Replies:
                    FetchQuery = FetchQuery + " AND " + SQLFetchReplies + SQLOrder + SQLLimit;
                    break;
                case TimelineManagement.TimeLineType.Direct:
                    FetchQuery = FetchQuery + " AND " + SQLFetchDirects + SQLOrder + SQLLimit;
                    break;
                case TimelineManagement.TimeLineType.Messages:
                    FetchQuery = FetchQuery + " AND " + SQLFetchRepliesAndMessages + SQLOrder + SQLLimit;
                    break;
            }

            using (SQLiteConnection conn = GetConnection())
            {
                using (var comm = new SQLiteCommand(FetchQuery, conn))
                {
                    comm.Parameters.Add(new SQLiteParameter("@accountsummary", account.Summary));
                    comm.Parameters.Add(new SQLiteParameter("@count", 1));

                    conn.Open();
                    return (string) comm.ExecuteScalar();
                }
            }
        }

        public static void VacuumDB()
        {
            using (SQLiteCommand comm = GetConnection().CreateCommand())
            {
                comm.Connection.Open();
                comm.CommandText = "vacuum;";
                comm.ExecuteNonQuery();
                comm.Connection.Close();
            }
        }
    }
}