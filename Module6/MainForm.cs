using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Module6
{
    public partial class MainForm : Form
    {
        private Session6Entities db = new Session6Entities();

        // This is the current datetime
        // I set it this way because of the limitaion from the database data
        // Which is it only went up to 2017 while the current year is 2019
        private DateTime dateTime = new DateTime(2017, 10, 7, 0, 0, 0);

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            GetSetFlights();
            GetSetTopCustomers();
            GetSetNumberOfPassengerFlyings();
            GetSetTopOffices();

            GetSetRevenue();
            GetSetWeeklyReport();

            stopwatch.Stop();
            labelTime.Text = $"Report generated in {stopwatch.Elapsed.TotalSeconds} seconds";
        }

        private void GetSetFlights()
        {
            var schedules = db.Schedules.ToList().Where(x => x.Date.Add(x.Time) >= dateTime.AddDays(-30) && x.Date.Add(x.Time) <= dateTime);
            labelConfirmed.Text = schedules.Count(x => x.Confirmed).ToString();
            labelCancelled.Text = schedules.Count(x => x.Confirmed == false).ToString();
            labelAverage.Text = ((int)schedules.Average(x => x.Route.FlightTime)).ToString() + " minutes";
        }

        private void GetSetTopCustomers()
        {
            var q = db.Users.Distinct().OrderByDescending(x => x.Tickets.Count).ToList();
            labelCustomer1.Text = $"1. {q[0].FirstName} {q[0].LastName} ({q[0].Tickets.Where(x => x.Schedule.Date.Add(x.Schedule.Time) >= dateTime.AddDays(-30) && x.Schedule.Date.Add(x.Schedule.Time) <= dateTime).ToList().Count} Tickets)";
            labelCustomer2.Text = $"2. {q[1].FirstName} {q[1].LastName} ({q[1].Tickets.Where(x => x.Schedule.Date.Add(x.Schedule.Time) >= dateTime.AddDays(-30) && x.Schedule.Date.Add(x.Schedule.Time) <= dateTime).ToList().Count} Tickets)";
            labelCustomer3.Text = $"3. {q[2].FirstName} {q[2].LastName} ({q[2].Tickets.Where(x => x.Schedule.Date.Add(x.Schedule.Time) >= dateTime.AddDays(-30) && x.Schedule.Date.Add(x.Schedule.Time) <= dateTime).ToList().Count} Tickets)";
        }

        private void GetSetNumberOfPassengerFlyings()
        {
            var q = db.Schedules.ToList().Where(x => x.Date.Add(x.Time) >= dateTime.AddDays(-30) && x.Date.Add(x.Time) <= dateTime).ToList();
            labelBusiest.Text = $"{q.First(y => y.Tickets.Count == q.Max(x => x.Tickets.Count)).Date.ToString("dd/MM")} with {q.First(y => y.Tickets.Count == q.Max(x => x.Tickets.Count)).Tickets.Count} flying";
            labelMostQuiet.Text = $"{q.First(y => y.Tickets.Count == q.Min(x => x.Tickets.Count)).Date.ToString("dd/MM")} with {q.First(y => y.Tickets.Count == q.Min(x => x.Tickets.Count)).Tickets.Count} flying";
        }

        private void GetSetTopOffices()
        {
            var q = db.Schedules.ToList().Where(x => x.Date.Add(x.Time) >= dateTime.AddDays(-30) && x.Date.Add(x.Time) <= dateTime).OrderByDescending(x => x.Route.Airport.Country.Airports.Count).SelectMany(x => x.Route.Airport.Country.Offices).Distinct().Select(x => x.Title).ToList();
            labelOffice1.Text = $"1. {q[0]}";
            labelOffice2.Text = $"2. {q[2]}";
            labelOffice3.Text = $"3. {q[3]}";
        }

        private void GetSetRevenue()
        {
            var yesterday = db.Tickets.ToList().Where(x => x.Schedule.Date == dateTime.AddDays(-1)).Select(x => x).ToList();
            labelYesterday.Text = yesterday.Sum(x => GetPrice(x)).ToString("C2").Split('.')[0];

            var twoDays = db.Tickets.ToList().Where(x => x.Schedule.Date == dateTime.AddDays(-2)).Select(x => x).ToList();
            labelTwoDaysAgo.Text = twoDays.Sum(x => GetPrice(x)).ToString("C2").Split('.')[0];

            var threeDays = db.Tickets.ToList().Where(x => x.Schedule.Date == dateTime.AddDays(-3)).Select(x => x).ToList();
            labelThreeDaysAgo.Text = threeDays.Sum(x => GetPrice(x)).ToString("C2").Split('.')[0];
        }

        private void GetSetWeeklyReport()
        {
            var thisWeek = db.Schedules.ToList().Where(x => x.Date >= dateTime.AddDays(-7) && x.Date <= dateTime && x.Confirmed).ToList();
            var thisWeekList = new List<double>();
            foreach (var item in thisWeek)
            {
                thisWeekList.Add(((double)item.Tickets.Count) / ((double)item.Aircraft.TotalSeats) * 100d);
            }

            if (thisWeekList.Count > 0)
            {
                labelThisWeek.Text = $"{(int)thisWeekList.Average()} %";
            }
            else
            {
                labelThisWeek.Text = "No Data";
            }

            var lastWeek = db.Schedules.ToList().Where(x => x.Date >= dateTime.AddDays(-14) && x.Date <= dateTime.AddDays(-7) && x.Confirmed).ToList();
            var lastWeekList = new List<double>();
            foreach (var item in lastWeek)
            {
                lastWeekList.Add(((double)item.Tickets.Count) / ((double)item.Aircraft.TotalSeats) * 100d);
            }

            if (lastWeekList.Count > 0)
            {
                labelLastWeek.Text = $"{(int)lastWeekList.Average()} %";
            }
            else
            {
                labelLastWeek.Text = "No Data";
            }

            var twoWeek = db.Schedules.ToList().Where(x => x.Date >= dateTime.AddDays(-21) && x.Date <= dateTime.AddDays(-14) && x.Confirmed).ToList();
            var twoWeekList = new List<double>();
            foreach (var item in twoWeek)
            {
                twoWeekList.Add(((double)item.Tickets.Count) / ((double)item.Aircraft.TotalSeats) * 100d);
            }

            if (twoWeekList.Count > 0)
            {
                labelTwoWeek.Text = $"{(int)twoWeekList.Average()} %";
            }
            else
            {
                labelTwoWeek.Text = "No Data";
            }
        }

        private int GetPrice(Ticket ticket)
        {
            if (ticket.Confirmed)
            {
                if (ticket.CabinTypeID == 1)
                {
                    return (int)ticket.Schedule.EconomyPrice;
                }
                else if (ticket.CabinTypeID == 2)
                {
                    return (int)(ticket.Schedule.EconomyPrice + ticket.Schedule.EconomyPrice * 0.35m);
                }
                else if (ticket.CabinTypeID == 3)
                {
                    return (int)(2 * ticket.Schedule.EconomyPrice + ticket.Schedule.EconomyPrice * 0.35m + ticket.Schedule.EconomyPrice * 0.3m);
                }

                return 0;
            }

            return 0;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}