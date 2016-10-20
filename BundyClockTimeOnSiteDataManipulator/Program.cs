using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ConsoleApplication5
{
    class ReviewEntryData
    {
        public string name { get; set;}
        public string employeenumber { get; set; }
        public DateTime accesstime { get; set; }
        public string direction { get; set; }
        public string jobreferencenumber { get; set; }
        public string comments { get; set; }
        public string location { get; set; }
    }

    class user
    {
      public List<ReviewEntryData> AccessEvents;
      public string username;   

      public user()
        {
          AccessEvents = new List<ReviewEntryData>(); 
        }
    }

    class dataUtility
    {
        public List<ReviewEntryData> LoadedAccessEvents;
        public List<string> Usernames;
        public List<user> UserObjectList;

        public dataUtility()
        {
            LoadedAccessEvents = new List<ReviewEntryData>();
        }

        //gets a list of unique user namse from the List of Review Data
        public List<string> get_distinct_list_of_users(List<ReviewEntryData> T)
        {
            List<string> names = new List<string>();
            foreach (ReviewEntryData value in T)
            {
                names.Add(value.name);
            }
            List<string> distinctnames = names.Distinct().ToList();
            return distinctnames;
        }
     
        public List<user> map_records_to_user_objects(List<string> names, List<ReviewEntryData> T)
        {
            List<user> u = new List<user>();
            int numberofrecords = T.Count;

            foreach (string value in names)
            {
                user _u = new user();
                for (int i = 0; i < numberofrecords; i++)
                {
                    if(T[i].name == value)
                    {
                        _u.username = T[i].name;
                        _u.AccessEvents.Add(T[i]);                       
                    }
                }
                u.Add(_u);
                Console.WriteLine("{0} complete", _u.username);
            }
            return u;
        }
        
        //method removes OUT as first entry and IN as last entry
        public void data_validator(List<user> U)
        {
            foreach(user value in U )
            {
                if(LoadedAccessEvents[LoadedAccessEvents.Count-1].direction == "In")
                {
                    LoadedAccessEvents.Remove(LoadedAccessEvents[LoadedAccessEvents.Count-1]);
                }                
                if(LoadedAccessEvents[0].direction == "Out")
                {
                    LoadedAccessEvents.Remove(LoadedAccessEvents[0]);
                }
            }            
        }
    }


    class Program
    {
        static void Main(string[] args)
        {

            try {
                dataUtility datahandler = new dataUtility();

                var reader = new StreamReader(File.OpenRead(@"C:\Reports\BundyClockReportOutput.csv"), Encoding.UTF8);
                
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');
                        DateTime t;
                        ReviewEntryData R = new ReviewEntryData();
                        R.name = values[0].ToString();
                        R.employeenumber = values[1].ToString();

                        if (DateTime.TryParse(values[2], out t))
                        {
                            R.accesstime = t;
                        }

                        R.direction = values[3].ToString();
                        R.jobreferencenumber = values[4].ToString();
                        R.comments = values[5].ToString();
                        R.location = values[6].ToString();
                        datahandler.LoadedAccessEvents.Add(R);
                    }

                    datahandler.Usernames = datahandler.get_distinct_list_of_users(datahandler.LoadedAccessEvents);
                    datahandler.UserObjectList = datahandler.map_records_to_user_objects(datahandler.Usernames, datahandler.LoadedAccessEvents);
                    datahandler.data_validator(datahandler.UserObjectList);
                    

                    using (StreamWriter file = new StreamWriter(@"C:\Reports\BundyClockReport.csv"))
                    {
                        file.WriteLine("ReferenceNumber" + "," + "Date" + "," + "Start" + "," + "End" + "," + "," + "," + "," + "," + "ManagerName" + "," + "LocationName" + ","  + "," + ",");
                    foreach (user u in datahandler.UserObjectList)
                        {

                            var offset = 0;

                            while (offset < u.AccessEvents.Count - 1)
                            {
                                if (u.AccessEvents[offset].direction == "Out")
                                {
                                    offset++;
                                }
                                else
                                {
                                    string timeout = null;
                                    var name = u.AccessEvents[offset].name;
                                    var employeenumber = u.AccessEvents[offset].employeenumber;
                                    var datein = u.AccessEvents[offset].accesstime.ToShortDateString();
                                    var timein = u.AccessEvents[offset].accesstime.ToString("HH:mm");
                                    var jobreference = u.AccessEvents[offset].jobreferencenumber;
                                    var comments = u.AccessEvents[offset].comments;
                                    var position = u.AccessEvents[offset].location;
                                    offset++;
                                    TimeSpan duration = u.AccessEvents[offset].accesstime - u.AccessEvents[offset - 1].accesstime;
                                    if (u.AccessEvents[offset].direction != u.AccessEvents[offset - 1].direction || duration.TotalHours < 22)
                                    {
                                    timeout = u.AccessEvents[offset].accesstime.ToString("HH:mm");
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
