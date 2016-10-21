using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TimeOnSiteDataManipulator
{
    internal class ReviewEntryData
    {
        public string Name { get; set;}
        public string Employeenumber { get; set; }
        public DateTime Accesstime { get; set; }
        public string Direction { get; set; }
        public string Jobreferencenumber { get; set; }
        public string Comments { get; set; }
        public string Location { get; set; }
    }

    internal class User
    {
      public List<ReviewEntryData> AccessEvents;
      public string Username;   

      public User()
        {
          AccessEvents = new List<ReviewEntryData>(); 
        }
    }

    internal class DataUtility
    {
        public List<ReviewEntryData> LoadedAccessEvents;
        public List<string> Usernames;
        public List<User> UserObjectList;

        public DataUtility()
        {
            LoadedAccessEvents = new List<ReviewEntryData>();
        }

        //gets a list of unique user namse from the List of Review Data
        public List<string> get_distinct_list_of_users(List<ReviewEntryData> T)
        {
            List<string> names = new List<string>();
            T.ForEach(i => names.Add(i.Name));           
            List<string> distinctnames = names.Distinct().ToList();
            return distinctnames;
        }
     
        public List<User> map_records_to_user_objects(List<string> names, List<ReviewEntryData> T)
        {
            List<User> user = new List<User>();
            int numberofrecords = T.Count;

            foreach (string value in names)
            {
                User u = new User();
                for (int i = 0; i < numberofrecords; i++)
                {
                    if(T[i].Name == value)
                    {
                        u.Username = T[i].Name;
                        u.AccessEvents.Add(T[i]);                       
                    }
                }
                user.Add(u);
                Console.WriteLine("{0} complete", u.Username);
            }
            return user;
        }
    }


    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var datahandler = new DataUtility();
                var reader = new StreamReader(File.OpenRead(@"C:\Reports\BundyClockReportOutput.csv"), Encoding.UTF8);              
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');
                        DateTime t;
                        var r = new ReviewEntryData();
                        r.Name = values[0].ToString();
                        r.Employeenumber = values[1].ToString();

                        if (DateTime.TryParse(values[2], out t))
                        {
                            r.Accesstime = t;
                        }

                        r.Direction = values[3].ToString();
                        r.Jobreferencenumber = values[4].ToString();
                        r.Comments = values[5].ToString();
                        r.Location = values[6].ToString();
                        datahandler.LoadedAccessEvents.Add(r);
                    }

                    datahandler.Usernames = datahandler.get_distinct_list_of_users(datahandler.LoadedAccessEvents);
                    datahandler.UserObjectList = datahandler.map_records_to_user_objects(datahandler.Usernames, datahandler.LoadedAccessEvents);
                                      
                    using (StreamWriter file = new StreamWriter(@"C:\Reports\BundyClockReport.csv"))
                    {
                        file.WriteLine("ReferenceNumber" + "," + "Date" + "," + "Start" + "," + "End" + "," + "," + "," + "," + "," + "ManagerName" + "," + "LocationName" + ","  + "," + ",");
                    foreach (User u in datahandler.UserObjectList)
                        {

                            var offset = 0;

                            while (offset < u.AccessEvents.Count - 1)
                            {
                                if (u.AccessEvents[offset].Direction == "Out")
                                {
                                    offset++;
                                }
                                else
                                {
                                    string timeout = null;
                                    var name = u.AccessEvents[offset].Name;
                                    var employeenumber = u.AccessEvents[offset].Employeenumber;
                                    var datein = u.AccessEvents[offset].Accesstime.ToShortDateString();
                                    var timein = u.AccessEvents[offset].Accesstime.ToString("HH:mm");
                                    var jobreference = u.AccessEvents[offset].Jobreferencenumber;
                                    var comments = u.AccessEvents[offset].Comments;
                                    var position = u.AccessEvents[offset].Location;
                                    offset++;
                                    TimeSpan duration = u.AccessEvents[offset].Accesstime - u.AccessEvents[offset - 1].Accesstime;
                                    if (u.AccessEvents[offset].Direction != u.AccessEvents[offset - 1].Direction || duration.TotalHours < 22)
                                    {
                                    timeout = u.AccessEvents[offset].Accesstime.ToString("HH:mm");
                                    offset++;
                                    }

                                    else
                                    {
                                    timeout = string.Empty;
                                    }

                                    file.WriteLine(employeenumber + "," + datein + "," + timein + "," + timeout + "," + "," + "," + ","+ "," + " Braemar Cooinda" + "," + jobreference + "," + comments + "," + position);

                                }
                            }                      
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);              
            }

         }
    }
}
