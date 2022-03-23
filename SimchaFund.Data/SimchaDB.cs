using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SimchaFund.Data
{
    public class Contributor
    {
        //TESTTTT
        //Please work!!!!!!!!!!
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public bool AlwaysInclude { get; set; }
        public decimal Balance { get; set; }
        public decimal? Amount { get; set; }
        public bool Include { get; set; }
 
    }

    public class Simcha
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public DateTime Date{ get; set; }
        public string ContributorCount { get; set; }
        public decimal Total { get; set; }
    }

    public class Transaction
    {
        public string Action { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
    }

    public class Deposit
    {
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public int ContributorID { get; set; }
    }
    public class SimchaDB
    {
        public string _connectionString { get; set; }
        public SimchaDB(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Simcha> GetSimchos()
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"SELECT s.*, COUNT(c.Amount) as 'ContributorCount', SUM(c.Amount) AS 'Total' 
                                FROM Simchos s
                                LEFT JOIN Contributions c 
                                ON s.ID = c.SimchaID
                                GROUP BY s.ID, s.Name, s.Date";
            connection.Open();
            var simchos = new List<Simcha>();
            var reader = cmd.ExecuteReader();
            var totalContributos = GetContributorCount();
            while (reader.Read())
            {
                simchos.Add(new Simcha
                {
                    ID = (int)reader["ID"],
                    Name = (string)reader["Name"],
                    Date = (DateTime)reader["Date"],
                    ContributorCount = $"{(int)reader["ContributorCount"]} / {totalContributos}",
                    Total = reader.GetOrNull<decimal>("Total")
                });
            }
            return simchos;
        }

        public string GetSimchaName(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Name FROM Simchos WHERE ID = @id";
            cmd.Parameters.AddWithValue("@id", id);
            connection.Open();
            return (string)cmd.ExecuteScalar();
        }

        public int GetContributorCount()
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Contributors";
            connection.Open();
            return (int)cmd.ExecuteScalar();
        }

        public void AddSimcha(Simcha simcha)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Simchos VALUES(@name, @date) SELECT SCOPE_IDENTITY()";
            cmd.Parameters.AddWithValue("@name", simcha.Name);
            cmd.Parameters.AddWithValue("@date", simcha.Date);
            connection.Open();
            simcha.ID = (int)(decimal)cmd.ExecuteScalar();
        }

        public List<Contributor> GetContributors(bool contributions, int simchaId = -1)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"SELECT  c.ID, c.FirstName, c.LastName, c.AlwaysInclude, c.Number, c.Date,
								SUM(d.Amount) - ISNULL((SELECT SUM(Amount) FROM Contributions 
								WHERE ContributorID = c.ID), 0) AS 'Balance'
                                FROM contributors c                             
                                JOIN Deposits d
                                ON d.ContributorID = c.ID
                                GROUP BY c.ID, c.FirstName, c.LastName, c.AlwaysInclude, c.Number, c.Date";
            connection.Open();
            var reader = cmd.ExecuteReader();
            var contributors = new List<Contributor>();
            while (reader.Read())
            {
                var id = (int)reader["ID"];
                var contributor = new Contributor
                {
                    ID = id,
                    Balance = (decimal)reader["Balance"],
                    FirstName = (string)reader["FirstName"],
                    LastName = (string)reader["LastName"],
                    Number = (string)reader["Number"],
                    Date = (DateTime)reader["Date"],
                    AlwaysInclude = (bool)reader["AlwaysInclude"]
                };
                if (contributions)
                {
                    contributor.Amount = GetContributionAmount(id, simchaId);
                }
                contributors.Add(contributor);
            }

            return contributors;
        }

        public decimal GetDepostisForContributor(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT SUM(Amount) FROM Deposits WHERE ContributorID = @id";
            cmd.Parameters.AddWithValue("@id", id);
            connection.Open();
            return (decimal)cmd.ExecuteScalar();

        }

        public decimal GetTotal()
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT SUM(Amount) - ISNULL((SELECT SUM(Amount) FROM Contributions), 0) FROM Deposits";
            connection.Open();
            return (decimal)cmd.ExecuteScalar();
        }

        public decimal? GetContributionAmount(int contributorId, int simchaId)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Amount FROM Contributions WHERE SimchaID = @simchaId AND ContributorID = @contributorID";
            cmd.Parameters.AddWithValue("@simchaId", simchaId);
            cmd.Parameters.AddWithValue("@contributorId", contributorId);
            connection.Open();
            decimal? amount = cmd.ExecuteScalar() == DBNull.Value ? null : (decimal?)cmd.ExecuteScalar();
            return amount;
        }

        public void AddContributor(Contributor contributor, decimal amount)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"INSERT INTO Contributors 
                                VALUES(@firstName, @lastName, @number, @date, @alwaysInclude) SELECT SCOPE_IDENTITY()";
            cmd.Parameters.AddWithValue("@firstName", contributor.FirstName);
            cmd.Parameters.AddWithValue("@lastName", contributor.LastName);
            cmd.Parameters.AddWithValue("@number", contributor.Number);
            cmd.Parameters.AddWithValue("@date", contributor.Date);
            cmd.Parameters.AddWithValue("@alwaysInclude", contributor.AlwaysInclude);
            connection.Open();
            var id = (int)(decimal)cmd.ExecuteScalar();
            var deposit = new Deposit { ContributorID = id, Amount = amount, Date = contributor.Date };
            AddDeposit(deposit);
        }

        public void AddDeposit(Deposit deposit)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO Deposits VALUES(@amount, @date, @contributorId)";
            cmd.Parameters.AddWithValue("@amount", deposit.Amount);
            cmd.Parameters.AddWithValue("@date", deposit.Date);
            cmd.Parameters.AddWithValue("@contributorId", deposit.ContributorID);
            connection.Open();
            cmd.ExecuteNonQuery();
        }

        public List<Transaction> GetHistory(int contributorId)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"SELECT * FROM Deposits WHERE ContributorID = @contributorId";
            cmd.Parameters.AddWithValue("@contributorId", contributorId);
            connection.Open();
            var history = new List<Transaction>();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                history.Add(new Transaction
                {
                    Amount = (decimal)reader["Amount"],
                    Date = (DateTime)reader["Date"],
                    Action = "Deposit"
                });
            }
            reader.Close();
            cmd.CommandText = @"SELECT c.*, s.Name from Contributions c
                                JOIN Simchos s
                                ON s.ID = c.SimchaID
                                WHERE c.ContributorID = @contributorId";

            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                history.Add(new Transaction
                {
                    Amount = -(decimal)reader["Amount"],
                    Date = (DateTime)reader["Date"],
                    Action = $"Contribution for the {(string)reader["Name"]}"

                });
            }
            return history;
        }

        public decimal GetContributorBalance(int contributorId)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"SELECT ISNULL(SUM(Amount), 0) - (SELECT ISNULL(SUM(Amount), 0) 
                                FROM Contributions WHERE ContributorID = @contributorId)
								AS 'Balance' FROM Deposits  
								WHERE ContributorID =  @contributorId";
            cmd.Parameters.AddWithValue("@contributorId", contributorId);
            connection.Open();
            return (decimal)cmd.ExecuteScalar();
        }

        public string GetContributorName(int contributorId)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"SELECT FirstName, LastName FROM Contributors
								 WHERE ID = @contributorId";
            cmd.Parameters.AddWithValue("@contributorId", contributorId);
            connection.Open();
            var reader = cmd.ExecuteReader();
            reader.Read();
            return $"{(string)reader["FirstName"]} {(string)reader["LastName"]}";
        }

        public void UpdateContributor(Contributor contributor)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"UPDATE Contributors SET FirstName = @firstName, LastName = @lastName, 
                                Number = @number, Date = @date, AlwaysInclude = @alwaysInclude WHERE ID = @id";
            cmd.Parameters.AddWithValue("@firstName", contributor.FirstName);
            cmd.Parameters.AddWithValue("@lastName", contributor.LastName);
            cmd.Parameters.AddWithValue("@number", contributor.Number);
            cmd.Parameters.AddWithValue("@date", contributor.Date);
            cmd.Parameters.AddWithValue("@alwaysInclude", contributor.AlwaysInclude);
            cmd.Parameters.AddWithValue("@id", contributor.ID);
            connection.Open();
            cmd.ExecuteNonQuery();
        }

        public void DeleteContributionsForSimcha(int simchaId)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM Contributions WHERE SimchaID = @simchaID";
            cmd.Parameters.AddWithValue("@simchaId", simchaId);
            connection.Open();
            cmd.ExecuteNonQuery();
        }

        public void AddContributionsForSimcha(List<Contributor> contributors, int simchaId)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"INSERT INTO Contributions VALUES(@contributorId, @simchaId, @amount, @date)";
            connection.Open();
            foreach (var contributor in contributors)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@contributorId", contributor.ID);
                cmd.Parameters.AddWithValue("@simchaId", simchaId);
                cmd.Parameters.AddWithValue("@amount", contributor.Amount);
                cmd.Parameters.AddWithValue("@date", DateTime.Now);
                cmd.ExecuteNonQuery();
            }


        }
    }

    public static class Extensions
    {
        public static T GetOrNull<T>(this SqlDataReader reader, string columnName)
        {
            object value = reader[columnName];
            if (value == DBNull.Value)
            {
                return default(T);
            }

            return (T)value;
        }
    }
}
