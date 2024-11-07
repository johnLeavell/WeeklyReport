using Microsoft.Maui.ApplicationModel.Communication;
using Xceed.Words.NET;
using System.IO;
using Xceed.Document.NET;

namespace WeeklyReport
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnSubmitClicked(object sender, EventArgs e)
        {
            var highlights = HighlightsEntry.Text;
            var challenges = ChallengesEntry.Text;
            var interesting = InterestingEntry.Text;
            var objectives = ObjectivesEntry.Text;

            // Calculate the Monday and Friday of the current week for the report date range
            DateTime today = DateTime.Now;
            DateTime monday = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
            DateTime friday = monday.AddDays(4);
            string dateRange = $"{monday:MM/dd/yyyy} - {friday:MM/dd/yyyy}";

            // Save report content as a Word document
            string filePath = await SaveReportAsWordDoc(highlights, challenges, interesting, objectives, dateRange);

            // Send the email with the date range in the subject and the file attached
            await SendEmail(dateRange, filePath);
        }



        private async Task<string> SaveReportAsWordDoc(string highlights, string challenges, string interesting, string objectives, string dateRange)
        {
            // Adjust file path to save on the desktop
            string fileName = $"Weekly_Report_{dateRange.Replace("/", "-")}.docx";
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filePath = Path.Combine(desktopPath, fileName);

            int counter = -1;
            while (File.Exists(filePath))
            {
                string newFileName = $"Weekly_Report_{dateRange.Replace("/", "-")}_{counter}.docx";
                filePath = Path.Combine(desktopPath, newFileName);
                counter++;
            }


            using (var doc = DocX.Create(filePath))
            {
                doc.InsertParagraph($"Weekly Report: {dateRange}")
                    .FontSize(20)
                    .Bold()
                    .Alignment = Alignment.center;

                doc.InsertParagraph("Highlights:")
                    .FontSize(14)
                    .Bold();
                doc.InsertParagraph(highlights).SpacingAfter(10);

                doc.InsertParagraph("Challenges:")
                    .FontSize(14)
                    .Bold();
                doc.InsertParagraph(challenges).SpacingAfter(10);

                doc.InsertParagraph("Interesting:")
                    .FontSize(14)
                    .Bold();
                doc.InsertParagraph(interesting).SpacingAfter(10);

                doc.InsertParagraph("Objectives:")
                    .FontSize(14)
                    .Bold();
                doc.InsertParagraph(objectives).SpacingAfter(10);

                doc.Save();
            }

            await DisplayAlert("Report Saved", $"The weekly report has been saved on your desktop as {fileName}", "OK");

            return filePath;
        }


        private async Task SendEmail(string dateRange, string filePath)
        {
            try
            {
                var message = new EmailMessage
                {
                    Subject = $"Weekly Report {dateRange}",
                    Body = "Please find the weekly report attached.", // Optional message in the body
                    To = new List<string> { "manager@example.com" }
                };

                // Create an attachment from the saved file and add it to the email
                var attachment = new EmailAttachment(filePath);
                message.Attachments.Add(attachment);

                await Email.ComposeAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email error: {ex.Message}");
            }
        }


    }
}
