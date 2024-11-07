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
            var highlights = HighlightsEditor.Text;
            var challenges = ChallengesEditor.Text;
            var interesting = InterestingEditor.Text;
            var objectives = ObjectivesEditor.Text;

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
            string fileName = $"Weekly_Report_{dateRange.Replace("/", "-")}.docx";
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filePath = Path.Combine(desktopPath, fileName);

            if (File.Exists(filePath))
            {
                string action = await DisplayActionSheet("File Exists", "Cancel", null, "Overwrite", "Save with New Name");

                if (action == "Overwrite")
                {
                    // Overwrite the file without changing filePath
                }
                else if (action == "Save with New Name")
                {
                    // Save a new file with a unique name
                    int counter = 1;
                    while (File.Exists(filePath))
                    {
                        string newFileName = $"Weekly_Report_{dateRange.Replace("/", "-")}_{counter}.docx";
                        filePath = Path.Combine(desktopPath, newFileName);
                        counter++;
                    }
                }
                else
                {
                    await DisplayAlert("Save Cancelled", "The report was not saved.", "OK");
                    return null;
                }
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
                    Body = "Please find the weekly report attached.",
                    To = new List<string> { "manager@example.com" }
                };

                // Attach the saved file to the email
                var attachment = new EmailAttachment(filePath);
                message.Attachments.Add(attachment);

                await Email.ComposeAsync(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email error: {ex.Message}");
            }
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Editor editor)
            {
                string[] lines = editor.Text.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    // Add a bullet if the line does not start with one
                    if (!string.IsNullOrWhiteSpace(lines[i]) && !lines[i].StartsWith("• "))
                    {
                        lines[i] = "• " + lines[i].TrimStart();
                    }
                }

                // Update text without triggering redundant events
                string newText = string.Join("\n", lines);
                if (editor.Text != newText)
                {
                    editor.Text = newText;
                }
            }
        }
    }
}
