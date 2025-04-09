using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using MuseumTourManagement.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


namespace MuseumTourManagement.Database
{
    public class Database
    {
        private static string connectionString = "Server=DESKTOP-IJNILMU;Database=MuseumDB;Trusted_Connection=True;";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        // 🔹 Get Today's Revenue
        public static decimal GetTodaysEarningsRevenue()
        {
            decimal total = 0;
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                string query = "SELECT SUM(Amount) FROM Earnings WHERE CAST(DateAdded AS DATE) = CAST(GETDATE() AS DATE);";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    object result = cmd.ExecuteScalar();
                    total = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                }
            }
            return total;
        }

        // 🔹 Get Upcoming Bookings Count
        public static int GetUpcomingBookings()
        {
            int count = 0;
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM Bookings WHERE Status = 'Confirmed';";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    count = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            return count;
        }

        // ✅ Updated GetAllCustomers to Include Booking Status
        public static List<CustomerExpedition> GetAllCustomers()
        {
            List<CustomerExpedition> customers = new List<CustomerExpedition>();

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT 
                        c.CustomerID, 
                        c.Name, 
                        c.Email, 
                        ISNULL(e.ExpeditionName, 'No Expedition') AS ExpeditionName, 
                        CASE 
                            WHEN b.BookingID IS NULL THEN 'No Booking'
                            ELSE 'Confirmed'
                        END AS BookingStatus
                    FROM Customers c
                    LEFT JOIN Bookings b ON c.CustomerID = b.CustomerID
                    LEFT JOIN Expeditions e ON b.ExpeditionID = e.ExpeditionID";


                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        customers.Add(new CustomerExpedition
                        {
                            CustomerID = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Email = reader.GetString(2),
                            ExpeditionName = reader.GetString(3),
                            BookingStatus = reader.GetString(4) // ✅ Now correctly retrieving Booking Status
                        });
                    }
                }
            }
            return customers;
        }

        // ✅ Get All Expeditions (Fixed)
        public static List<Expedition> GetAllExpeditions()
        {
            List<Expedition> expeditions = new List<Expedition>();

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT ExpeditionID, ExpeditionName, 
                    CONVERT(VARCHAR(10), TimeSlot, 108) AS TimeSlot 
                    FROM Expeditions";



                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        expeditions.Add(new Expedition
                        {
                            ExpeditionID = reader.GetInt32(0),
                            ExpeditionName = reader.GetString(1),
                            TimeSlot = reader.IsDBNull(2) ? "N/A" : reader.GetString(2)
                        });
                    }
                }
            }
            return expeditions;
        }

        // ✅ Get Customers for Selected Expedition (Fixed Table Name)
        public static List<CustomerExpedition> GetCustomerExpeditions(int expeditionID)
        {
            List<CustomerExpedition> customers = new List<CustomerExpedition>();

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT c.CustomerID, c.Name, c.Email, e.ExpeditionName, 
                           FORMAT(b.TimeSlot, 'hh:mm tt') AS TimeSlot  -- ✅ CORRECT: TimeSlot from Bookings
                    FROM MuseumDB.dbo.Bookings b
                    JOIN MuseumDB.dbo.Customers c ON b.CustomerID = c.CustomerID
                    JOIN MuseumDB.dbo.Expeditions e ON b.ExpeditionID = e.ExpeditionID
                    WHERE b.ExpeditionID = @ExpeditionID";



                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ExpeditionID", expeditionID);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            customers.Add(new CustomerExpedition
                            {
                                CustomerID = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Email = reader.GetString(2),
                                ExpeditionName = reader.GetString(3),
                                BookingStatus = reader.IsDBNull(4) ? "N/A" : reader.GetTimeSpan(4).ToString(@"hh\:mm tt")
                            });
                        }
                    }
                }
            }
            return customers;
        }

        public static DataTable GetAllBookings()
        {
            DataTable bookingsTable = new DataTable();

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                string query = @"
    SELECT 
        b.BookingID, 
        b.ExpeditionID,  -- 🔹 Ensure this is selected
        c.Name AS CustomerName, 
        e.ExpeditionName, 
        b.BookingDate, 
        FORMAT(b.TimeSlot, 'hh:mm tt') AS TimeSlot, 
        b.Status
    FROM Bookings b
    JOIN Customers c ON b.CustomerID = c.CustomerID
    JOIN Expeditions e ON b.ExpeditionID = e.ExpeditionID
    ORDER BY b.BookingDate DESC;";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(bookingsTable);
                }
            }
            return bookingsTable;
        }

        public static string GetExpeditionTimeSlot(int expeditionId)
        {
            string timeSlot = "";
            string query = "SELECT TimeSlot FROM Expeditions WHERE ExpeditionID = @ExpeditionID";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ExpeditionID", expeditionId);
                conn.Open();
                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    timeSlot = Convert.ToString(result);
                }
            }
            return timeSlot;
        }

        public static DataTable GetAllEmployeesDataTable()
        {
            DataTable table = new DataTable();

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                string query = "SELECT EmployeeID, Name, Position FROM Employees";  // ✅ Adjust column names if needed

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(table);
                }
            }

            return table;
        }



        // 🔹 Get Available Exhibitions
        public static DataTable GetAvailableExhibitions()
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                string query = "SELECT ExhibitionID, Name, TicketsAvailable FROM Exhibitions WHERE TicketsAvailable > 0;";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }
            return dt;
        }
        public static void AddEarning(decimal amount)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO Earnings (Amount, DateAdded) VALUES (@amount, @date)", conn);
                cmd.Parameters.AddWithValue("@amount", amount);
                cmd.Parameters.AddWithValue("@date", DateTime.Today);
                cmd.ExecuteNonQuery();
            }
        }

        public static Dictionary<DateTime, decimal> GetDailyEarnings()
        {
            Dictionary<DateTime, decimal> data = new Dictionary<DateTime, decimal>();

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
            SELECT DateAdded, SUM(Amount) AS Total
            FROM Earnings
            GROUP BY DateAdded
            ORDER BY DateAdded", conn);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime date = reader.GetDateTime(0);
                        decimal total = reader.GetDecimal(1);
                        data[date] = total;
                    }
                }
            }

            return data;
        }

        public static List<Earning> GetAllEarnings()
        {
            List<Earning> earnings = new List<Earning>();

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                string query = "SELECT Id, Amount, DateAdded FROM Earnings ORDER BY DateAdded DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        earnings.Add(new Earning
                        {
                            Id = reader.GetInt32(0),
                            Amount = reader.GetDecimal(1),
                            Date = reader.GetDateTime(2)
                        });
                    }
                }
            }

            return earnings;
        }


        public static void DeleteEarningByDateAndAmount(DateTime date, decimal amount)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
            DELETE TOP(1) FROM Earnings 
            WHERE CAST(DateAdded AS DATE) = @date AND Amount = @amount", conn);

                cmd.Parameters.AddWithValue("@date", date.Date);
                cmd.Parameters.AddWithValue("@amount", amount);
                cmd.ExecuteNonQuery();
            }
        }
        public static void DeleteEarningById(int id)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                string query = "DELETE FROM Earnings WHERE Id = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void AddEmployee(string name, string position)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO Employees (Name, Position) VALUES (@name, @position)", conn);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@position", position);
                cmd.ExecuteNonQuery();
            }
        }

        public static void DeleteEmployeeById(int employeeId)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                string query = "DELETE FROM Employees WHERE EmployeeID = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", employeeId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static int GetTotalEmployees()
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Employees", conn);
                return (int)cmd.ExecuteScalar();
            }
        }

        public static DataTable GetAllCustomersDataTable()
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                string query = "SELECT * FROM Customers";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    return table;
                }
            }
        }

    }
}
