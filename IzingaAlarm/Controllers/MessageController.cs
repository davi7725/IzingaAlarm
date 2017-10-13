using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.SqlClient;

namespace IzingaAlarm.Controllers
{
    public class MessageController : ApiController
    {
        [Route("AlarmTriggers/{phoneNr}/{message}")]
        [HttpGet]
        public string AlarmTriggers(string phoneNr, string message)
        {
            string queryAlarmTypeId = "Select AlarmTypeId from AlarmType Where Type = @alarmType";
            string queryPersonsToAlert = "Select * from PersonToManage WHERE AlarmType=@alarmTypeId and AlarmNumber = @phoneNumber";
            string returnString = "";

            try
            {
                SqlConnection conn = openNewConnection();
                SqlCommand cmd = new SqlCommand(queryAlarmTypeId, conn);
                cmd.Parameters.Add(new SqlParameter("@alarmType", message));
                SqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.HasRows)
                {
                    rdr.Read();
                    int alarmTypeId = rdr.GetInt32(0);
                    rdr.Close();

                    cmd = new SqlCommand(queryPersonsToAlert, conn);
                    cmd.Parameters.Add(new SqlParameter("@alarmTypeId", alarmTypeId));
                    cmd.Parameters.Add(new SqlParameter("@phoneNumber", phoneNr));

                    rdr = cmd.ExecuteReader();
                    if(rdr.HasRows)
                    {
                        while(rdr.Read())
                        {
                            returnString += "PERSON: " + rdr.GetString(1) + " For alarm: " + rdr.GetString(2) + " with the type " + rdr.GetInt32(0).ToString() + "\n";
                        }
                    }
                }
                else
                {
                    returnString = "No such an alarm type, check input.";
                }

            }
            catch (SqlException sqlE)
            {
                returnString = sqlE.Message;
            }

            return returnString;
        }

        [Route("SubscribeAlarm/{name}/{phoneNr}/{alarmPhoneNumber}/{alarmType}")]
        [HttpPost]
        public string SubscribeAlarm(string name, string phoneNr, string alarmPhoneNumber, string alarmType)
        {
            string queryCreatePerson = "Insert into ResponsiblePerson Values (@phoneNumber,@name)";
            string queryAlarmTypeId = "Select AlarmTypeId from AlarmType Where Type = @alarmType";
            string querySubscribe = "Insert into PersonToManage Values (@alarmTypeId,@personPhone,@alarmNr)";
            string returnString = "";

            try
            {
                SqlConnection conn = openNewConnection();
                SqlCommand cmd = new SqlCommand(queryCreatePerson,conn);
                cmd.Parameters.Add(new SqlParameter("@name", name));
                cmd.Parameters.Add(new SqlParameter("@phoneNumber", phoneNr));
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch(SqlException e)
                { }

                cmd = new SqlCommand(queryAlarmTypeId,conn);
                cmd.Parameters.Add(new SqlParameter("@alarmType", alarmType));
                SqlDataReader rdr = cmd.ExecuteReader();
                if(rdr.HasRows)
                {
                    rdr.Read();
                    int alarmTypeId = rdr.GetInt32(0);
                    rdr.Close();

                    cmd = new SqlCommand(querySubscribe, conn);
                    cmd.Parameters.Add(new SqlParameter("@alarmNr", alarmPhoneNumber));
                    cmd.Parameters.Add(new SqlParameter("@alarmTypeId", alarmTypeId));
                    cmd.Parameters.Add(new SqlParameter("@personPhone", phoneNr));

                    if(cmd.ExecuteNonQuery() > 0)
                    {
                        returnString = "Successfuly added a subscription";
                    }
                    else
                    {
                        returnString = "An error occurred, check input.";
                    }
                }
                else
                {
                    returnString = "An error occurred, check input.";
                }
                
            }catch(SqlException sqlE)
            {
                returnString = "User already subscribes this alarm";
            }

            return returnString;
        }

        [Route("UnsubscribeAlarm/{phoneNr}/{alarmPhoneNumber}/{alarmType}")]
        [HttpDelete]
        public string UnsubscribeAlarm(string phoneNr, string alarmPhoneNumber, string alarmType)
        {
            string queryAlarmTypeId = "Select AlarmTypeId from AlarmType Where Type = @alarmType";
            string queryUnsubscribe = "Delete from PersonToManage WHERE personNumber=@phoneNr and alarmNumber=@alarmPhoneNumber and alarmType=@alarmType";
            string returnString = "";

            try
            {
                SqlConnection conn = openNewConnection();
                SqlCommand cmd = new SqlCommand(queryAlarmTypeId, conn);
                cmd.Parameters.Add(new SqlParameter("@alarmType", alarmType));
                SqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.HasRows)
                {
                    rdr.Read();
                    int alarmTypeId = rdr.GetInt32(0);
                    rdr.Close();

                    cmd = new SqlCommand(queryUnsubscribe, conn);
                    cmd.Parameters.Add(new SqlParameter("@alarmPhoneNumber", alarmPhoneNumber));
                    cmd.Parameters.Add(new SqlParameter("@alarmType", alarmTypeId));
                    cmd.Parameters.Add(new SqlParameter("@phoneNr", phoneNr));

                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        returnString = "Successfuly removed from a subscription";
                    }
                    else
                    {
                        returnString = "There is no subscription for this alarm type and user";
                    }
                }
                else
                {
                    returnString = "No such an alarm type, check input.";
                }

            }
            catch (SqlException sqlE)
            {
                returnString = "An error ocurred, check input!";
            }

            return returnString;
        }

        [Route("RegisterAlarm/{phoneNr}")]
        [HttpPost]
        public string RegisterAlarm(string phoneNr)
        {
            string queryRegisterAlarm = "Insert into Alarm values(@phoneNr)";
            string returnString = "";

            try
            {
                SqlConnection conn = openNewConnection();
                SqlCommand cmd = new SqlCommand(queryRegisterAlarm, conn);
                    cmd.Parameters.Add(new SqlParameter("@phoneNr", phoneNr));

                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        returnString = "Successfuly added an alarm system";
                    }
                    else
                    {
                        returnString = "Can't seem to add an alarm right now, please try later";
                    }
            }
            catch (SqlException sqlE)
            {
                returnString = "An alarm with this number is already registered.";
            }

            return returnString;
        }

        private SqlConnection openNewConnection()
        {
            string connection = "Server=ealdb1.eal.local;Database=EJL66_DB;User ID=ejl66_usr;Password=Baz1nga66";
            SqlConnection conn = new SqlConnection(connection);
            conn.Open();
            return conn;
        }
    }
}
