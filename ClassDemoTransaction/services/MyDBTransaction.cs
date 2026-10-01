using ClassDemoTransaction.model;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassDemoTransaction.services
{
    public class MyDBTransaction
    {
        // instans felter
        private const string _connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=2semDemo;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";
        private SqlConnection _conn;


        private void ConnectToDB()
        {
            _conn = new SqlConnection(_connectionString);
            _conn.Open();
        }

        private void DisconnectFromDB()
        {
            _conn.Close();
        }


        private const string sql_getById = "SELECT *  FROM Konto WHERE Id = @id";
        public Konto GetById(int kontoId)
        {
            Konto konto = new Konto();

            ConnectToDB();
            SqlCommand cmd = new SqlCommand(sql_getById, _conn);
            cmd.Parameters.AddWithValue("@id", kontoId);

            SqlDataReader reader = cmd.ExecuteReader();


            if (reader.Read()) // OK
            {
                konto.Id = reader.GetInt32(0);
                konto.Name = reader.GetString(1);
                konto.Balance = reader.GetDecimal(2);
            }
            else // failed
            {
                DisconnectFromDB();
                throw new KeyNotFoundException();
            }

            DisconnectFromDB();
            return konto;
        }

        private const string sql_insertAmount = "UPDATE Konto set Balance = Balance + @amount WHERE Id = @id";
        public decimal InsertAmount(int kontoId, decimal amount)
        {
            decimal newAmount = 0;

            ConnectToDB();
            SqlCommand cmd = new SqlCommand(sql_insertAmount, _conn);
            cmd.Parameters.AddWithValue("@amount", amount);
            cmd.Parameters.AddWithValue("@id", kontoId);

            int row = cmd.ExecuteNonQuery();
            DisconnectFromDB();

            if (row == 0) // failed
            {
                throw new KeyNotFoundException();
            }

            Konto updatedKonto = GetById(kontoId);
            return updatedKonto.Balance;
        }


        public void MoveAmount(int FromId, int ToId, decimal amount)
        {
            InsertAmount(FromId, -amount);

            bool stop = simulateStop();
            if (stop)
            {
                return;
            }

            InsertAmount(ToId, amount);
        }


        private const string sql_moveAmountminus = "UPDATE Konto set Balance = Balance - @amount WHERE Id = @fromid";
        private const string sql_moveAmountplus = "UPDATE Konto set Balance = Balance + @plusamount WHERE Id = @toid";

        public void MoveAmountUsingTransaction(int FromId, int ToId, decimal amount)
        {
            ConnectToDB();
            SqlTransaction trans = _conn.BeginTransaction();

            SqlCommand cmd1 = new SqlCommand(sql_moveAmountminus, _conn, trans);
            cmd1.Parameters.AddWithValue("@amount", amount);
            cmd1.Parameters.AddWithValue("@fromid", FromId);
            int row = cmd1.ExecuteNonQuery();

            bool stop = simulateStop();
            if (stop)
            {
                trans.Rollback();
                DisconnectFromDB(); 
                return;
            }

            SqlCommand cmd2 = new SqlCommand(sql_moveAmountplus, _conn, trans);
            cmd2.Parameters.AddWithValue("@plusamount", amount);
            cmd2.Parameters.AddWithValue("@toid", ToId);
            int row2 = cmd2.ExecuteNonQuery();

            trans.Commit();
            DisconnectFromDB();

        }






        /*
         * hjælpe metoder
         */
        private bool simulateStop()
        {
            Console.Write("Er der opstået em fejl j/n");
            string? ok = Console.ReadLine();
            if (ok != null)
            {
                if (ok.ToLower() == "j")
                {
                    // stopper 
                    return true;
                }
            }

            return false;
        }
    }
}
