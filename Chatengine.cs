using System;
using System.Collections.Generic;

namespace CyberBotGUI
{
    public class ChatEngine
    {
        public string UserName { get; set; } = "";
        public Random Rng = new Random();
        public string LastTopic = "";
        public TaskManager TaskMgr = new TaskManager();
        public QuizManager QuizMgr;

        public ChatEngine()
        {
            QuizMgr = new QuizManager(TaskMgr);
        }

        // Response database - each topic has one detailed response
        public Dictionary<string, string> Responses = new()
        {
            ["password"] = "Strong passwords use 12+ characters with uppercase, lowercase, numbers, and symbols. Example: $uN#8kPz!mQ2@. Never reuse passwords. Use a passphrase like My#Cat@Eats2Fish. A password manager stores all your passwords safely.",

            ["phishing"] = "Phishing is when scammers send fake emails pretending to be trusted companies. Look for urgent language, spelling mistakes, and suspicious links. Never click links in suspicious emails. Your bank will never ask for your password by email.",

            ["safe browsing"] = "Always look for https and a padlock icon in the address bar before entering passwords or credit cards. Avoid pop-up ads that say 'You've won!' Keep your browser updated for security fixes.",

            ["two factor"] = "Two-factor authentication (2FA) adds a second login step. Even if someone steals your password, they still need the code from your phone. Enable 2FA on email and banking accounts.",

            ["malware"] = "Malware is harmful software that damages your device or steals information. Types include viruses, trojans, and spyware. Install antivirus software and keep it updated. Never open attachments from unknown senders.",

            ["ransomware"] = "Ransomware locks your files and demands money to unlock them. Never open unexpected attachments. Back up your files regularly to an external drive or cloud storage.",

            ["firewall"] = "A firewall monitors your internet traffic and blocks suspicious connections. Windows and macOS have built-in firewalls. Make sure yours is turned on, especially on public Wi-Fi.",

            ["vpn"] = "A VPN encrypts your internet traffic and hides your online activity. This is important on public Wi-Fi. Be careful with free VPNs - they often sell your data. Choose a reputable paid VPN.",

            ["social engineering"] = "Social engineering tricks people into giving away information by manipulating emotions like fear or urgency. Always verify who you're talking to. If something feels wrong, hang up and call the official number.",

            ["data breach"] = "A data breach happens when hackers steal user information from a company. Check haveibeenpwned.com to see if your email was exposed. Use unique passwords for every account.",

            ["encryption"] = "Encryption scrambles your data so only authorised people can read it. Your password is encrypted before being sent online. WhatsApp and Signal use end-to-end encryption for messages."
        };

        // Extra tips for follow-up questions
        public Dictionary<string, List<string>> ExtraTips = new()
        {
            ["password"] = new List<string> {
                "Never write passwords on sticky notes!",
                "Change important passwords every 3-6 months.",
                "Don't use the same password for multiple sites."
            },
            ["phishing"] = new List<string> {
                "Always check the sender's email address carefully.",
                "Hover over links before clicking to see the real address."
            },
            ["firewall"] = new List<string> {
                "Configure your firewall to block unnecessary incoming connections.",
                "Regularly update your firewall rules to address new threats."
            },
            ["social engineering"] = new List<string> {
                "Avoid opening unknown emails or clicking suspicious links.",
                "Trust no one who asks for personal information."
            },
            ["encryption"] = new List<string> {
                "Always have a random key lock to assure the safety of the data.",
                "Do not use the same password for multiple sites."
            },
            ["malware"] = new List<string> {
                "Keep your operating system updated for security patches.",
                "Be careful with USB drives from unknown sources.",
                "Use a standard user account instead of admin for daily tasks."
            }
        };

        // Empathetic responses for user emotions
        public Dictionary<string, string> SentimentResponses = new()
        {
            ["worried"] = "I understand you are worried. ",
            ["scared"] = "No need to panic. ",
            ["confused"] = "Let me explain clearly. ",
            ["frustrated"] = "I understand your frustration. ",
            ["curious"] = "Great question! ",
            ["nervous"] = "It's okay to feel nervous. "
        };

        // NLP command variations
        public Dictionary<string, List<string>> NLPVariations = new()
        {
            ["addtask"] = new List<string> { "add task", "create task", "new task", "add a task", "create a task" },
            ["viewtasks"] = new List<string> { "view tasks", "show tasks", "list tasks", "my tasks", "see tasks", "display tasks" },
            ["completetask"] = new List<string> { "complete task", "finish task", "mark done", "task complete", "complete" },
            ["deletetask"] = new List<string> { "delete task", "remove task", "clear task", "delete" },
            ["startquiz"] = new List<string> { "start quiz", "begin quiz", "take quiz", "start game", "quiz" },
            ["activitylog"] = new List<string> { "activity log", "show log", "what have you done", "recent actions", "log" }
        };

        // Detect task-related commands using NLP
        public string DetectTaskCommand(string input)
        {
            // Check for "remind me to" pattern
            if (input.Contains("remind me to"))
            {
                int start = input.IndexOf("remind me to") + "remind me to".Length;
                string taskTitle = input.Substring(start).Trim();
                if (!string.IsNullOrEmpty(taskTitle))
                {
                    TaskMgr.AddTask(taskTitle, "", DateTime.Now.AddDays(3));
                    return "Reminder set for '" + taskTitle + "' for 3 days from now.";
                }
            }

            // Check for add task
            foreach (var variation in NLPVariations["addtask"])
            {
                if (input.Contains(variation))
                {
                    int start = input.IndexOf(variation) + variation.Length;
                    string details = input.Substring(start).Trim();

                    if (string.IsNullOrEmpty(details))
                        return "What task would you like to add?";

                    DateTime? reminder = null;
                    string title = details;

                    if (details.Contains("in ") && details.Contains("day"))
                    {
                        string[] parts = details.Split(new string[] { " in " }, StringSplitOptions.None);
                        title = parts[0].Trim();
                        string timePart = parts.Length > 1 ? parts[1].Trim() : "";

                        string[] words = timePart.Split(' ');
                        foreach (string word in words)
                        {
                            if (int.TryParse(word, out int days))
                            {
                                reminder = DateTime.Now.AddDays(days);
                                break;
                            }
                        }
                    }

                    if (TaskMgr.AddTask(title, "", reminder))
                    {
                        string response = "Task added: '" + title + "'.";
                        if (reminder.HasValue)
                            response += " Reminder set for " + reminder.Value.ToShortDateString() + ".";
                        return response;
                    }
                    return "Sorry, couldn't add the task.";
                }
            }

            // Check for view tasks
            foreach (var variation in NLPVariations["viewtasks"])
            {
                if (input.Contains(variation))
                {
                    var tasks = TaskMgr.GetAllTasks();
                    if (tasks.Count == 0)
                        return "You don't have any tasks yet.";

                    string result = "Your tasks:\n";
                    int count = 0;
                    foreach (var task in tasks)
                    {
                        count++;
                        string status = task.IsCompleted ? "[DONE]" : "[PENDING]";
                        string reminder = task.ReminderDate.HasValue ? " (Reminder: " + task.ReminderDate.Value.ToShortDateString() + ")" : "";
                        result += count + ". " + task.Title + " " + status + reminder + "\n";
                    }
                    return result;
                }
            }

            // Check for complete task
            foreach (var variation in NLPVariations["completetask"])
            {
                if (input.Contains(variation))
                {
                    string[] words = input.Split(' ');
                    foreach (string word in words)
                    {
                        if (int.TryParse(word, out int id))
                            return TaskMgr.CompleteTask(id) ? "Task " + id + " completed!" : "Task not found.";
                    }
                    return "Specify which task to complete (e.g., 'complete task 1').";
                }
            }

            // Check for delete task
            foreach (var variation in NLPVariations["deletetask"])
            {
                if (input.Contains(variation))
                {
                    string[] words = input.Split(' ');
                    foreach (string word in words)
                    {
                        if (int.TryParse(word, out int id))
                            return TaskMgr.DeleteTask(id) ? "Task " + id + " deleted." : "Task not found.";
                    }
                    return "Specify which task to delete (e.g., 'delete task 1').";
                }
            }

            // Check for quiz
            foreach (var variation in NLPVariations["startquiz"])
            {
                if (input.Contains(variation))
                {
                    TaskMgr.LogAction("Quiz Started", "User began the cybersecurity quiz");
                    return QuizMgr.StartQuiz();
                }
            }

            // Check for activity log
            foreach (var variation in NLPVariations["activitylog"])
            {
                if (input.Contains(variation))
                {
                    return TaskMgr.GetActivityLog();
                }
            }

            return "";
        }

        // Detects topic from user input
        public string GetTopic(string input)
        {
            if (QuizMgr.IsActive)
            {
                string cleanInput = input.ToUpper().Trim();
                bool isAnswer = cleanInput == "A" || cleanInput == "B" || cleanInput == "C" || cleanInput == "D" ||
                                 cleanInput == "TRUE" || cleanInput == "FALSE" ||
                                 cleanInput == "1" || cleanInput == "2" || cleanInput == "3" || cleanInput == "4";
                if (isAnswer)
                    return "quizresponse";
            }

            string taskCmd = DetectTaskCommand(input);
            if (!string.IsNullOrEmpty(taskCmd))
                return "taskcommand";

            if (input.Contains("tell me more") || input.Contains("explain more"))
                return "tellmemore";

            if (input.Contains("another tip") || input.Contains("give me a tip"))
                return "anothertip";

            if (input.Contains("password") || input.Contains("passwords")) return "password";
            if (input.Contains("phishing")) return "phishing";
            if (input.Contains("safe browsing") || input.Contains("browsing")) return "safe browsing";
            if (input.Contains("two factor") || input.Contains("2fa")) return "two factor";
            if (input.Contains("malware")) return "malware";
            if (input.Contains("ransomware")) return "ransomware";
            if (input.Contains("firewall")) return "firewall";
            if (input.Contains("vpn")) return "vpn";
            if (input.Contains("social engineering")) return "social engineering";
            if (input.Contains("data breach")) return "data breach";
            if (input.Contains("encryption")) return "encryption";
            if (input.Contains("how are you")) return "howareyou";
            if (input.Contains("purpose") || input.Contains("what can you do")) return "purpose";
            if (input.Contains("my name is") || input.Contains("call me")) return "setname";
            if (input.Contains("menu") || input.Contains("topics")) return "menu";
            if (input.Contains("exit") || input.Contains("quit") || input.Contains("bye")) return "exit";

            return "unknown";
        }

        public string DetectSentiment(string input)
        {
            foreach (var emotion in SentimentResponses)
            {
                if (input.Contains(emotion.Key))
                    return emotion.Value;
            }
            return "";
        }

        public string GetExtraTip(string topic)
        {
            if (ExtraTips.ContainsKey(topic) && ExtraTips[topic].Count > 0)
            {
                return ExtraTips[topic][Rng.Next(ExtraTips[topic].Count)];
            }
            return "";
        }

        public string GetReply(string rawInput)
        {
            string input = rawInput.ToLower().Trim();

            if (string.IsNullOrWhiteSpace(input))
                return "Please type something so I can help you.";

            string topic = GetTopic(input);
            string sentiment = DetectSentiment(input);

            if (topic == "quizresponse")
                return QuizMgr.SubmitAnswer(input);

            if (topic == "taskcommand")
            {
                string response = DetectTaskCommand(input);
                if (!string.IsNullOrEmpty(response))
                    return response;
            }

            if (topic == "menu")
                return GetTopicList();

            if (topic == "exit")
                return "exit";

            if (topic == "howareyou")
                return "I'm running smoothly and ready to help you stay safe online!";

            if (topic == "purpose")
                return "I'm Cyber Bot - your online safety guide. I can help with cybersecurity topics, tasks, quiz, and activity log.";

            if (topic == "setname")
            {
                int nameStart = input.IndexOf("my name is") + "my name is".Length;
                if (nameStart < 0 || nameStart >= rawInput.Length)
                    return "Please say: my name is [your name].";

                string newName = rawInput.Substring(nameStart).Trim().TrimEnd('.');
                if (!string.IsNullOrWhiteSpace(newName))
                {
                    UserName = char.ToUpper(newName[0]) + newName.Substring(1);
                    TaskMgr.CurrentUser = UserName;
                    TaskMgr.LogAction("Name Set", "User name set to: " + UserName);
                    return "Nice to meet you, " + UserName + "! What would you like to learn about?";
                }
                return "Please say: my name is [your name].";
            }

            if (topic == "tellmemore" || topic == "anothertip")
            {
                if (!string.IsNullOrEmpty(LastTopic) && Responses.ContainsKey(LastTopic))
                {
                    string tip = GetExtraTip(LastTopic);
                    return !string.IsNullOrEmpty(tip) ? tip : Responses[LastTopic];
                }
                return "Ask about a topic first, like 'phishing' or 'passwords'.";
            }

            if (Responses.ContainsKey(topic))
            {
                LastTopic = topic;
                TaskMgr.LogAction("User Asked About Topic", topic);

                string prefix = (!string.IsNullOrWhiteSpace(UserName) && Rng.Next(3) == 0) ? "Good question, " + UserName + "! " : "";
                return prefix + sentiment + Responses[topic] + " Type 'menu' for all topics.";
            }

            if (!string.IsNullOrEmpty(sentiment))
                return sentiment + "Type 'menu' to see topics I can help with.";

            return "I didn't understand that. Type 'menu' to see what I can help with.";
        }

        public string GetTopicList()
        {
            return "=== CYBER BOT MENU ===\n" +
                   "\nLEARN ABOUT:\n" +
                   "1. passwords\n" +
                   "2. phishing\n" +
                   "3. safe browsing\n" +
                   "4. two factor (2FA)\n" +
                   "5. malware\n" +
                   "6. ransomware\n" +
                   "7. firewall\n" +
                   "8. vpn\n" +
                   "9. social engineering\n" +
                   "10. data breach\n" +
                   "11. encryption\n" +
                   "\nTASK MANAGEMENT:\n" +
                   "add task [title] - Add a new task\n" +
                   "view tasks - See all your tasks\n" +
                   "complete task [number] - Mark task as done\n" +
                   "delete task [number] - Remove a task\n" +
                   "remind me to [task] - Set a reminder\n" +
                   "\nQUIZ:\n" +
                   "start quiz - Test your cybersecurity knowledge\n" +
                   "\nACTIVITY:\n" +
                   "activity log - See recent actions\n" +
                   "\nOTHER:\n" +
                   "tell me more - Get extra tips\n" +
                   "another tip - Get another tip\n" +
                   "my name is [name] - Set your name\n" +
                   "exit - Close the application";
        }
    }
}