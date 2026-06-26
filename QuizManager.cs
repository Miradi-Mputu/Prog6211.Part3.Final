using System;
using System.Collections.Generic;

namespace CyberBotGUI
{
    public class QuizManager
    {
        public List<QuizQuestion> Questions = new List<QuizQuestion>();
        public int CurrentIndex = 0;
        public int Score = 0;
        public bool IsActive = false;
        public Random Rng = new Random();
        public TaskManager TaskMgr;
        private int currentRound = 0;
        private int lastAnsweredRound = -1;

        public QuizManager(TaskManager taskMgr)
        {
            TaskMgr = taskMgr;
            LoadQuestions();
        }

        private void LoadQuestions()
        {
            Questions = new List<QuizQuestion>
            {
                new QuizQuestion("What should you do if you receive an email asking for your password?",
                    new List<string> { "Reply with your password", "Delete the email", "Report it as phishing", "Ignore it" },
                    2, "Reporting phishing emails helps prevent scams."),

                new QuizQuestion("Using the same password for multiple accounts is safe.",
                    new List<string> { "True", "False" },
                    1, "Never reuse passwords. One breach compromises all accounts."),

                new QuizQuestion("Which is a strong password?",
                    new List<string> { "password123", "My#Cat@Eats2Fish!", "qwerty", "123456789" },
                    1, "Strong passwords use a mix of characters and are easy to remember."),

                new QuizQuestion("Two-factor authentication adds an extra layer of security.",
                    new List<string> { "True", "False" },
                    0, "2FA requires a second verification step, making accounts harder to hack."),

                new QuizQuestion("What is phishing?",
                    new List<string> { "A type of virus", "Fake emails from trusted companies", "A firewall", "A password manager" },
                    1, "Phishing is a scam where attackers send fake messages to steal information."),

                new QuizQuestion("It is safe to click links in emails from unknown senders.",
                    new List<string> { "True", "False" },
                    1, "Never click links from unknown senders. They could lead to malicious sites."),

                new QuizQuestion("What does a firewall do?",
                    new List<string> { "Installs viruses", "Monitors and blocks suspicious traffic", "Creates passwords", "Deletes files" },
                    1, "A firewall acts like a security guard for your network."),

                new QuizQuestion("Ransomware locks your files and demands payment.",
                    new List<string> { "True", "False" },
                    0, "Ransomware encrypts files and demands payment for decryption."),

                new QuizQuestion("Best way to protect against data breaches?",
                    new List<string> { "Use same password everywhere", "Use unique passwords for each account", "Share passwords", "Write passwords on notes" },
                    1, "Unique passwords ensure one breach doesn't compromise all accounts."),

                new QuizQuestion("What is social engineering?",
                    new List<string> { "A type of hacking", "Manipulating people for information", "A firewall", "Encryption software" },
                    1, "Social engineering exploits human emotions to reveal information."),

                new QuizQuestion("A VPN hides your online activity from others on the same network.",
                    new List<string> { "True", "False" },
                    0, "A VPN encrypts internet traffic, making it invisible to others."),

                new QuizQuestion("What should you do if you suspect a data breach?",
                    new List<string> { "Ignore it", "Change passwords immediately", "Share on social media", "Delete all accounts" },
                    1, "Changing passwords immediately protects accounts from unauthorised access.")
            };
        }

        public string StartQuiz()
        {
            CurrentIndex = 0;
            Score = 0;
            IsActive = true;
            currentRound = 1;
            lastAnsweredRound = -1;
            ShuffleQuestions();
            TaskMgr.LogAction("Quiz Started", "User began the cybersecurity quiz");
            return "Let's test your cybersecurity knowledge!\n" + GetCurrentQuestion();
        }

        private void ShuffleQuestions()
        {
            for (int i = Questions.Count - 1; i > 0; i--)
            {
                int j = Rng.Next(i + 1);
                var temp = Questions[i];
                Questions[i] = Questions[j];
                Questions[j] = temp;
            }
        }

        public string GetCurrentQuestion()
        {
            if (CurrentIndex >= Questions.Count)
                return GetFinalResult();

            var q = Questions[CurrentIndex];
            string result = "\nQuestion " + (CurrentIndex + 1) + " of " + Questions.Count + ": " + q.Question + "\n";
            for (int i = 0; i < q.Options.Count; i++)
            {
                char letter = (char)('A' + i);
                result += "  " + letter + ") " + q.Options[i] + "\n";
            }
            return result + "Type A, B, C, or D to answer.";
        }

        public string SubmitAnswer(string input)
        {
            if (!IsActive)
                return "No quiz is active. Type 'start quiz' to begin.";

            if (CurrentIndex >= Questions.Count)
                return GetFinalResult();

            if (currentRound == lastAnsweredRound)
                return GetCurrentQuestion();

            var q = Questions[CurrentIndex];
            int selected = -1;
            string cleanInput = input.ToUpper().Trim();

            if (cleanInput.Length == 1 && cleanInput[0] >= 'A' && cleanInput[0] <= 'D')
            {
                selected = cleanInput[0] - 'A';
            }
            else if (cleanInput.Length == 1 && cleanInput[0] >= '1' && cleanInput[0] <= '4')
            {
                selected = cleanInput[0] - '1';
            }
            else if (cleanInput == "TRUE")
            {
                selected = 0;
            }
            else if (cleanInput == "FALSE")
            {
                selected = 1;
            }

            if (selected < 0 || selected >= q.Options.Count)
                return "Please type A, B, C, or D to answer.";

            lastAnsweredRound = currentRound;

            bool correct = (selected == q.CorrectIndex);
            string result = correct ? "Correct! " : "Wrong. ";
            if (correct) Score++;

            CurrentIndex++;
            currentRound++;

            result += q.Explanation + "\n";

            if (CurrentIndex >= Questions.Count)
                result += GetFinalResult();
            else
                result += GetCurrentQuestion();

            return result;
        }

        public string GetFinalResult()
        {
            IsActive = false;
            double pct = (double)Score / Questions.Count * 100;

            TaskMgr.SaveQuizResult(TaskMgr.CurrentUser, Score, Questions.Count, pct);

            string feedback = pct >= 80 ? "Excellent! You're a cybersecurity pro!" :
                             pct >= 60 ? "Good job! Keep learning!" :
                             "Keep learning! Review the topics in the menu.";

            return "\n=== QUIZ COMPLETE ===\nScore: " + Score + " out of " + Questions.Count + " (" + pct.ToString("F0") + "%)\n" +
                   feedback + "\nType 'menu' to return.";
        }
    }

    public class QuizQuestion
    {
        public string Question { get; set; }
        public List<string> Options { get; set; }
        public int CorrectIndex { get; set; }
        public string Explanation { get; set; }

        public QuizQuestion(string question, List<string> options, int correctIndex, string explanation)
        {
            Question = question;
            Options = options;
            CorrectIndex = correctIndex;
            Explanation = explanation;
        }
    }
}