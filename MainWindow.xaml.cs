using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CyberBotGUI
{
    public partial class MainWindow : Window
    {
        // Create the chatbot engine
        public ChatEngine Engine = new ChatEngine();

        // Constructor - runs when app starts
        public MainWindow()
        {
            InitializeComponent();

            // Play greeting sound when program opens
            AudioPlayer.PlayGreeting();

            // Focus on the name input box when window loads
            Loaded += (s, e) => NameInput.Focus();
        }

        // Press Enter key in name input to start
        private void NameInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true; // stop this keypress from being processed again elsewhere
                StartButton_Click(sender, e);
            }
        }

        // Start button clicked - save name and show chat
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameInput.Text.Trim();

            // Check if name is empty
            if (string.IsNullOrWhiteSpace(name))
            {
                NameError.Visibility = Visibility.Visible;
                return;
            }

            // Capitalise first letter of name
            name = char.ToUpper(name[0]) + name.Substring(1);
            Engine.UserName = name;

            // Set current user for database logging
            Engine.TaskMgr.CurrentUser = name;
            Engine.TaskMgr.LogAction("User Login", "User '" + name + "' logged in");

            // Update header with user's name
            HeaderUserName.Text = "CYBER BOT  |  Hello, " + name;

            // Switch from name screen to chat screen
            NameScreen.Visibility = Visibility.Collapsed;
            ChatScreen.Visibility = Visibility.Visible;

            // Focus on message input box
            MessageInput.Focus();

            // Show welcome message and topic menu
            AddMessageInOneBox("Bot: Welcome, " + name + "! I am Cyber Bot, your personal online safety guide.");
            AddMessageInOneBox("Bot: " + Engine.GetTopicList());
        }

        // Press Enter key in message input to send
        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true; // stop this keypress from being processed again elsewhere
                SendButton_Click(sender, e);
            }
        }

        // Send button clicked - process user message
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            // Guard against double-firing (e.g. fast double-click or Enter spam)
            if (!SendButton.IsEnabled)
                return;

            string message = MessageInput.Text.Trim();

            // Don't send empty messages
            if (string.IsNullOrWhiteSpace(message))
                return;

            // Disable input while we process this message so it cannot be triggered again
            SendButton.IsEnabled = false;
            MessageInput.IsEnabled = false;

            // Clear input box
            MessageInput.Clear();

            // Show user's message in chat
            AddMessageInOneBox("You: " + message);

            // Get bot's reply
            string reply = Engine.GetReply(message);

            // Check if user wants to exit
            if (reply == "exit")
            {
                Engine.TaskMgr.LogAction("User Logout", "User '" + Engine.UserName + "' logged out");
                AddMessageInOneBox("Bot: Goodbye, " + Engine.UserName + "! Stay safe online.");

                // Close the app after 2 seconds so user can see goodbye message
                var timer = new System.Windows.Threading.DispatcherTimer();
                timer.Interval = System.TimeSpan.FromSeconds(2);
                timer.Tick += (s, args) => { timer.Stop(); Application.Current.Shutdown(); };
                timer.Start();
                return;
            }

            // Show bot's reply with typewriter effect
            TypeWrite("Bot: " + reply);

            // Re-enable input and focus back on it
            SendButton.IsEnabled = true;
            MessageInput.IsEnabled = true;
            MessageInput.Focus();
        }

        // Add a complete message in ONE bordered box
        public void AddMessageInOneBox(string text)
        {
            TextBox tb = MakeResponseBox(text);
            Border box = WrapInBorder(tb);
            MessagePanel.Children.Add(box);
            ChatScroll.ScrollToBottom();
        }

        // Typewriter effect - displays text one character at a time
        public void TypeWrite(string text)
        {
            TextBox tb = null;

            // Create empty box on UI thread
            Dispatcher.Invoke(() =>
            {
                tb = MakeResponseBox("");
                Border box = WrapInBorder(tb);
                MessagePanel.Children.Add(box);
                ChatScroll.ScrollToBottom();
            });

            // Add characters one by one on background thread
            new Thread(() =>
            {
                foreach (char c in text)
                {
                    Dispatcher.Invoke(() =>
                    {
                        tb.Text += c;
                        tb.ScrollToEnd();
                        ChatScroll.ScrollToBottom();
                    });
                    Thread.Sleep(18);
                }
            })
            { IsBackground = true }.Start();
        }

        // Create a read-only text box that wraps text
        public TextBox MakeResponseBox(string text)
        {
            return new TextBox
            {
                Text = text,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                IsReadOnly = true,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                Foreground = Brushes.Black,
                Padding = new Thickness(4)
            };
        }

        // Wrap a text box in a visible border
        public Border WrapInBorder(TextBox tb)
        {
            return new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Margin = new Thickness(0, 4, 0, 4),
                Padding = new Thickness(6),
                Child = tb
            };
        }
    }
}