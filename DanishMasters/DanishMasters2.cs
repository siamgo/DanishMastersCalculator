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
        private string tournamentName;
        private DataTable dataTable;

        public danishMasters()
        {
            InitializeComponent();
            coachData = new List<CoachData>();
            dataTable = new DataTable();
            dataTable.Columns.Add("Tournament Data Added:", typeof(string));
            dataTable.Columns.Add("Days:", typeof(string));

            dataGridView1.DataSource = dataTable;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void upload_Click(object sender, EventArgs e)
        {
            numOfCoaches = 0;
         //   openFileDialog.InitialDirectory = @"..\Tournament Results";
            openFileDialog.Title = "Vælg fil";
            openFileDialog.ShowDialog();

            if (openFileDialog.FileName != "openFileDialog")
            {
                string page = File.ReadAllText(openFileDialog.FileName);

                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(page);

                List<string> cellData = new List<string>();

                string title = doc.DocumentNode.InnerHtml;

                int pFrom = title.IndexOf("<title>") + "<title>".Length;
                int pTo = title.LastIndexOf(" - ");

                tournamentName = title.Substring(pFrom, pTo - pFrom);

                dataTable.Rows.Add(tournamentName, tbDays.Text);

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

                createCoachData();
            }
        }

        private void trimCellData(List<string> cellData)
        {
            tournamentData = new List<string>();

            cellData.RemoveRange(0, 9);

            for (int i = 0; i < cellData.Count(); i += 13)
            {
                numOfCoaches++;
            }

            for (int i = 1; i < cellData.Count(); i += 13)
            {
                // Coach Name inserted
                tournamentData.Add(char.ToUpper(cellData.ElementAt(i)[0]) +cellData.ElementAt(i).ToLower().Substring(1));

                // Placement
                tournamentData.Add(cellData.ElementAt(i - 1));

                // Points used for Stunty
                tournamentData.Add(cellData.ElementAt(i + 3));

                // Race Inserted
                tournamentData.Add(cellData.ElementAt(i + 2));

                // Touchdowns
                tournamentData.Add(cellData.ElementAt(i + 4));

                // Casualties
                tournamentData.Add(cellData.ElementAt(i + 7));

                // Points for series
                if (numOfCoaches < 10)
                {
                    var points = numOfCoaches - i / 13 + 1;
                    points += int.Parse(tbDays.Text);
                    tournamentData.Add(points.ToString());
                }

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
            bonus = (numOfCoaches - i) / 10;
            int bonusRounded = (int)Math.Floor(bonus);
            return bonusRounded;
        }

        private void createCoachData()
        {
            Boolean newCoach = true;

            for (int i = 0; i < tournamentData.Count(); i += 7)
            {
                foreach (CoachData coach in coachData)
                {
                    if (coach.Name.Equals(tournamentData.ElementAt(i)))
                    {
                        coach.Tournaments.Add(new Tournament()
                        {
                            TournamentName = tournamentName,
                            Race = tournamentData.ElementAt(i + 3),
                            Casualties = tournamentData.ElementAt(i + 5),
                            Points = tournamentData.ElementAt(i + 6),
                            StuntyPoints = RaceIsStunty(tournamentData.ElementAt(i + 3)) ? tournamentData.ElementAt(i + 2) : "",
                            TouchDowns = tournamentData.ElementAt(i + 4),
                            Placement = tournamentData.ElementAt(i + 1) + " / " + numOfCoaches
                        });

                        newCoach = false;
                    }
                }
                if (newCoach)
                {
                    coachData.Add(new CoachData(tournamentData.ElementAt(i), new Tournament()
                    {
                        TournamentName = tournamentName,
                        Race = tournamentData.ElementAt(i + 3),
                        Casualties = tournamentData.ElementAt(i + 5),
                        Points = tournamentData.ElementAt(i + 6),
                        StuntyPoints = RaceIsStunty(tournamentData.ElementAt(i + 3)) ? tournamentData.ElementAt(i + 2) : "",
                        TouchDowns = tournamentData.ElementAt(i + 4),
                        Placement = tournamentData.ElementAt(i + 1) + " / " + numOfCoaches

                    }));
                }
                newCoach = true;
            }
        }

        private bool RaceIsStunty(string race)
        {
            if (race == "Goblin" || race == "Ogre" || race == "Halfling")
                return true;

            return false;
        }

        private void calculateStandings()
        {
            List<int> stunty = new List<int>();

            foreach (CoachData cd in coachData)
            {
                cd.CAS = 0;
                cd.Points = 0;
                cd.TD = 0;
                cd.StuntyPoints = 0;

                if (cd.Tournaments.Count() < 5)
                {
                    stunty = new List<int>();

                    foreach (var tournament in cd.Tournaments)
                    {
                        cd.CAS += int.Parse(tournament.Casualties);
                        cd.Points += int.Parse(tournament.Points);
                        cd.TD += int.Parse(tournament.TouchDowns);

                        if (!string.IsNullOrEmpty(tournament.StuntyPoints))
                            stunty.Add(int.Parse(tournament.StuntyPoints));
                        
                    }

                    stunty.Sort();
                    stunty.Reverse();

                    int i = 0;
                    foreach (var s in stunty)
                    {
                        if (i < 3)
                            cd.StuntyPoints += s;
                        i++;
                    }

                }
                else
                {
                    List<int> cas = new List<int>();
                    List<int> points = new List<int>();
                    List<int> tds = new List<int>();
                    stunty = new List<int>();

                    foreach (var tournament in cd.Tournaments)
                    {
                        cas.Add(int.Parse(tournament.Casualties));
                        points.Add(int.Parse(tournament.Points));
                        tds.Add(int.Parse(tournament.TouchDowns));

                        if (!string.IsNullOrEmpty(tournament.StuntyPoints))
                            stunty.Add(int.Parse(tournament.StuntyPoints));
                    }

                    cas.Sort();
                    cas.Reverse();

                    points.Sort();
                    points.Reverse();

                    tds.Sort();
                    tds.Reverse();

                    stunty.Sort();
                    stunty.Reverse();

                    for (int i = 0; i < 5; i++)
                    {
                        cd.CAS += cas.ElementAt(i);
                        cd.Points += points.ElementAt(i);
                        cd.TD += tds.ElementAt(i);

                        if (stunty.Count > i && i <= 3)
                            cd.StuntyPoints += stunty.ElementAt(i);
                    }
                }
            }
        }

        private void saveCoachData()
        {
            StreamWriter writer = new StreamWriter(@".\CoachData.txt", false);

            writer.WriteLine("first_name,score,touchdowns,casualties,stunty,numbermatches,played_matches");

            foreach (CoachData cd in coachData)
            {
                string record = $"{cd.Name},{cd.Points},{cd.TD},{cd.CAS},{cd.StuntyPoints},{cd.Tournaments.Count()},";
                string htmlRecord = "\"";

                foreach (var tournament in cd.Tournaments)
                {
                    htmlRecord += $"<ul><li><strong>Tournament:</strong> " + tournament.TournamentName +
                        " <strong>Finished:</strong> " + tournament.Placement +
                        " <strong>Race:</strong> " + tournament.Race +
                        " <strong>Points:</strong> " + tournament.Points +
                        " <strong>TD:</strong> " + tournament.TouchDowns +
                        " <strong>Cas:</strong> " + tournament.Casualties;

                    if (tournament.StuntyPoints != "0" && !string.IsNullOrEmpty(tournament.StuntyPoints))
                        htmlRecord += " <strong>Stunty Points:</strong> " + tournament.StuntyPoints;

                    htmlRecord += "</li></ul>&nbsp; ";
                }

                record += htmlRecord + "\"";

                writer.WriteLine(record);
            }
            writer.Close();
        }

        private void openFileDialog_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void saveData_Click(object sender, EventArgs e)
        {
            saveCoachData();
        }

        private void calculateStandings_Click(object sender, EventArgs e)
        {
            calculateStandings();
        }

        private void reset_data(object sender, EventArgs e)
        {
            coachData = new List<CoachData>();
            tournamentData = new List<string>();

            dataTable.Clear();

            File.WriteAllText(@".\CoachData.txt", string.Empty);
        }
    }
}
