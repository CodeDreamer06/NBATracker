using HtmlAgilityPack;
using Scheduling;

namespace NBATracker;

class Program
{
    static readonly string EmailMessage = @"
<p>Hi there!</p> {0}
<i>Have a nice day! 😊<i>";

    static void Main()
    {
        Schedule.Every().Day().At("08:00").Run(() =>
        {
            var message = GetResults();
            var service = new EmailService();
            service.SetEmailConfig();
            service.SendEmail("Check the NBA results for today!", message);
        });
    }

    private static string GetResults()
    {
        var doc = new HtmlWeb().Load("https://www.basketball-reference.com/boxscores/").DocumentNode;
        var title = doc.SelectSingleNode("//*[@id='content']/h1").InnerHtml;

        var resultExists = doc.SelectSingleNode("//*[@id='content']/div[2]/div[1]/p/strong") is null;

        if (!resultExists) 
            return string.Format(EmailMessage, "No games were played today.");

        var games = doc.SelectNodes("//*[@id='content']/div[3]/div");

        Console.WriteLine(title);

        List<Score> scores = new();
        for (int i = 0; i < games.Count; i++)
        {
            var game = games[i];
            string scoreTable = $"//div[{i + 1}]/table[1]/tbody/";
            var winner = game.SelectSingleNode($"{scoreTable}tr[2]/td[1]/a").InnerHtml;
            var loser = game.SelectSingleNode($"{scoreTable}tr[1]/td[1]/a").InnerHtml;
            var winningScore = game.SelectSingleNode($"{scoreTable}tr[2]/td[2]").InnerHtml;
            var losingScore = game.SelectSingleNode($"{scoreTable}tr[1]/td[2]").InnerHtml;

            scores.Add(new Score
            {
                Winner = winner,
                WinningScore = winningScore,
                Loser = loser,
                LosingScore = losingScore
            });

            Console.WriteLine($"{winner} {winningScore} | {losingScore} {loser}");
        }

        string message = @"
<b>Here are the NBA scores for today:</b>
<table style=""border-collapse: collapse;
    text-align: left;
    margin: 25px 0;
    font-size: 0.9em;
    font-family: sans-serif;
    min-width: 400px;
    box-shadow: 0 0 20px rgba(0, 0, 0, 0.15);"">
<tr style=""    
    background-color: #009879;
    color: #ffffff;
""><th>Winner</th><th>Score</th><th>Loser</th><th>Score</th>";

        foreach (var score in scores)
        {
            message += @$"<tr>
                            <td>{score.Winner}</td>
                            <td>{score.WinningScore}</td>
                            <td>{score.Loser}</td>
                            <td>{score.LosingScore}</td></tr>";
        }

        message += "</table>";

        return string.Format(EmailMessage, message);
    }
}

public class Score
{
    public string Winner { get; set; } = null!;
    public string WinningScore { get; set; } = null!;
    public string Loser { get; set; } = null!;
    public string LosingScore { get; set; } = null!;
}