using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using HtmlAgilityPack;


namespace DanishMasters
{
    public partial class danishMasters : Form
    {
        private List<CoachData> coachData;
        private List<string> tournamentData;
        private int numOfCoaches;
        public danishMasters()
        {
            InitializeComponent();
            coachData = new List<CoachData>();
        }

        private void upload_Click(object sender, EventArgs e)
        {
            numOfCoaches = 0;
            openFileDialog.InitialDirectory = "C:\\Users\\siamg\\Google Drev\\Blood Bowl\\Series Udregner\\DanishMasters";
            openFileDialog.Title = "Vælg fil";
            openFileDialog.ShowDialog();
            string page = File.ReadAllText(openFileDialog.FileName);

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(page);

            List<string> cellData = new List<string>();

            foreach (HtmlNode table in doc.DocumentNode.SelectNodes("//table"))
            {
                foreach (HtmlNode row in table.SelectNodes("tr"))
                {
                    foreach (HtmlNode cell in row.SelectNodes("td"))
                    {
                       // Console.WriteLine("cell: " + cell.InnerText);
                        cellData.Add(cell.InnerText);
                    }
                }
            }

            trimCellData(cellData);

            //test af fil indhold
            //String filename = openFileDialog.FileName;
            //writeToFile(tournamentData, filename.Substring(37, filename.Length - 42)+".txt");

            createCoachData();
            calculateStandings();
            //saveCoachData();
        }

        private void trimCellData(List<string> cellData)
        {
            tournamentData = new List<string>();

            cellData.RemoveRange(0, 9);

            for (int i = 0; i < cellData.Count(); i += 13)
            {
                numOfCoaches++;
            }

            for (int i = 1; i < cellData.Count(); i+=13)
            {
                tournamentData.Add(cellData.ElementAt(i).ToLower());
                tournamentData.Add(cellData.ElementAt(i+4));
                tournamentData.Add(cellData.ElementAt(i+7));
                if(numOfCoaches < 10)
                    tournamentData.Add(calculatePoints(13*((numOfCoaches*i)-1) ));
                else
                tournamentData.Add(calculatePoints(i));
            }
        }

        private string calculatePoints(int i)
        {
            int points = 0;
            double placering = i / 13 + 1;
            //placering
            switch (placering)
            {
                case 1:
                    points += 10;
                break;
                case 2:
                    points += 9;
                    break;
                case 3:
                    points += 8;
                    break;
                case 4:
                    points += 7;
                    break;
                case 5:
                    points += 6;
                    break;
                case 6:
                    points += 5;
                    break;
                case 7:
                    points += 4;
                    break;
                case 8:
                    points += 3;
                    break;
                case 9:
                    points += 2;
                    break;
                case 10:
                    points += 1;
                    break;
                default:
                    points += 0;
                    break;
            }
            points += participantsBonus(placering);
            points += int.Parse(tbDays.Text);

            return points.ToString();
        }

        private int participantsBonus(double i)
        {
            double bonus = 0;
            bonus = (numOfCoaches - i)/10;
            int bonusRounded = (int)Math.Floor(bonus);
            return bonusRounded;
        }

        private void writeToFile(List<string> tournamentData, string filename)
        {
            StreamWriter writer = new StreamWriter(@"C:\\Users\\siamg\\Google Drev\\Blood Bowl\\Series Udregner\\DanishMasters\\" + filename, false);
            foreach(String s in tournamentData)
            {
                writer.WriteLine(s);
            }
            writer.Close();
        }

        private void createCoachData()
        {
            Boolean newCoach = true;
            
            for (int i = 0; i < tournamentData.Count(); i += 4)
            {
                foreach (CoachData cd in coachData)
                {
                    if (cd.name.Equals(tournamentData.ElementAt(i)))
                    {
                        cd.TD.Add(int.Parse(tournamentData.ElementAt(i + 1)));
                        cd.CAS.Add(int.Parse(tournamentData.ElementAt(i + 2)));
                        cd.Points.Add(int.Parse(tournamentData.ElementAt(i + 3)));
                        if (i == 0)
                            cd.firstPlaces += 1;
                        if (i == 4)
                            cd.secondPlaces += 1;
                        newCoach = false;
                    }
                }
                if (newCoach)
                {
                    if (i == 0)
                        coachData.Add(new CoachData(tournamentData.ElementAt(i), int.Parse(tournamentData.ElementAt(i + 1)), int.Parse(tournamentData.ElementAt(i + 2)), int.Parse(tournamentData.ElementAt(i + 3)), 1, 0));
                    else if (i == 4)
                        coachData.Add(new CoachData(tournamentData.ElementAt(i), int.Parse(tournamentData.ElementAt(i + 1)), int.Parse(tournamentData.ElementAt(i + 2)), int.Parse(tournamentData.ElementAt(i + 3)), 0, 1));
                    else
                        coachData.Add(new CoachData(tournamentData.ElementAt(i), int.Parse(tournamentData.ElementAt(i + 1)), int.Parse(tournamentData.ElementAt(i + 2)), int.Parse(tournamentData.ElementAt(i + 3)), 0, 0));
                }
                newCoach = true;
            }
        }

        private void calculateStandings()
        {
            int lenght = 0;
            //Points
            List<Placement> placementPoints = new List<Placement>();
            foreach(CoachData cd in coachData)
            {
                int points = 0;
                cd.Points.Sort();
                cd.Points.Reverse();
                                
                if (cd.Points.Count()<5)
                {
                    lenght = cd.Points.Count();
                }
                else
                {
                    lenght = 5;
                }
                
                for (int i=0; i< lenght; i++)
                {
                    points += cd.Points.ElementAt(i);
                }
                placementPoints.Add(new Placement(cd.name, points));
            }
            placementPoints = placementPoints.OrderBy(o=>o.value).ToList();
            placementPoints.Reverse();

            Console.WriteLine("----Points----");
            for (int i=0;i<5;i++)
            {
                Console.WriteLine(placementPoints.ElementAt(i).name+" "+ placementPoints.ElementAt(i).value);
            }

            //Touchdowns
            List<Placement> placementTD = new List<Placement>();
            foreach (CoachData cd in coachData)
            {
                if (cd.TD.Count() < 5)
                {
                    lenght = cd.TD.Count();
                }
                else
                {
                    lenght = 5;
                }
                int TD = 0;
                cd.TD.Sort();
                cd.TD.Reverse();
                for (int i = 0; i < lenght; i++)
                {
                    TD += cd.TD.ElementAt(i);
                }
                placementTD.Add(new Placement(cd.name, TD));
            }
            placementTD = placementTD.OrderBy(o => o.value).ToList();
            placementTD.Reverse();
            Console.WriteLine("----TD----");
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine(placementTD.ElementAt(i).name + " " + placementTD.ElementAt(i).value);
            }

            //Casualties
            List<Placement> placementCAS = new List<Placement>();
            foreach (CoachData cd in coachData)
            {
                if (cd.CAS.Count() < 5)
                {
                    lenght = cd.CAS.Count();
                }
                else
                {
                    lenght = 5;
                }
                int CAS = 0;
                cd.CAS.Sort();
                cd.CAS.Reverse();
                for (int i = 0; i < lenght; i++)
                {
                    CAS += cd.CAS.ElementAt(i);
                }
                placementCAS.Add(new Placement(cd.name, CAS));
            }
            placementCAS = placementCAS.OrderBy(o => o.value).ToList();
            placementCAS.Reverse();
            Console.WriteLine("----CAS----");
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine(placementCAS.ElementAt(i).name + " " + placementCAS.ElementAt(i).value);
            }

            saveCoachData();
        }

        private void saveCoachData()
        {
            StreamWriter writer = new StreamWriter(@"C:\\Users\\siamg\\Google Drev\\Blood Bowl\\Series Udregner\\DanishMasters\\CoachData.txt", false);
            foreach (CoachData cd in coachData)
            {
                //name
                writer.WriteLine(cd.name);
                //TD
                string td = "";
                foreach (int TD in cd.TD)
                {
                    td += TD + ";";
                }
                writer.WriteLine("TD: "+td.Substring(0, td.Length-1));
                //CAS
                string cas = "";
                foreach (int CAS in cd.CAS)
                {
                    cas += CAS + ";";
                }
                writer.WriteLine("CAS: "+cas.Substring(0, cas.Length - 1));
                //Points
                string points = "";
                foreach (int Points in cd.Points)
                {
                    points += Points + ";";
                }
                writer.WriteLine("Point: "+points.Substring(0, points.Length - 1));
            }
            writer.Close();

            writer = new StreamWriter(@"C:\\Users\\siamg\\Google Drev\\Blood Bowl\\Series Udregner\\DanishMasters\\Placement.txt", false);
            foreach (CoachData cd in coachData)
            {
                //name
                writer.WriteLine(cd.name);
                //Number of tournements
                writer.WriteLine("Number of tournaments: " + cd.Points.Count());
                //Number of first places
                writer.WriteLine("First places: " + cd.firstPlaces);
                //Number of second places
                writer.WriteLine("Second places: " + cd.secondPlaces);
                writer.WriteLine("");
            }
            writer.Close();
        }
    }
}
