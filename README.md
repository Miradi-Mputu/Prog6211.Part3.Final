# Prog6211.Part3.Final
# Cyber Bot - Cybersecurity Awareness Chatbot
## Description
Cyber Bot is a Windows desktop chatbot that educates users about cybersecurity threats and best practices. Built with WPF and C#, it provides an interactive way to learn about online safety through conversation.
## YouTube Link 

## Features

- Audio greeting that plays when the application starts
- Visual logo displayed on the launch screen
- Personalised experience that asks for and remembers the user's name
- Typewriter effect that makes bot responses appear one character at a time
- Topic menu that lists all available cybersecurity topics
- Random responses that give different answers for the same topic
- Sentiment detection that responds empathetically when users express worry or confusion
- Follow-up questions that support "tell me more" and "another tip" commands

## Cybersecurity Topics Covered
- Passwords and password safety
- Phishing scams and how to spot them
- Safe browsing habits
- Two Factor Authentication (2FA)
- Malware and its different types
- Ransomware attacks
- Firewall protection
- VPN and private browsing
- Social engineering tactics
- Data breaches
- Encryption and data protection

Bot: Welcome, John! I am Cyber Bot, your personal online safety guide.

Bot: === CYBER BOT MENU ===

LEARN ABOUT:
1. passwords
2. phishing
3. safe browsing
4. two factor (2FA)
5. malware
6. ransomware
7. firewall
8. vpn
9. social engineering
10. data breach
11. encryption

TASK MANAGEMENT:
add task [title] - Add a new task
view tasks - See all your tasks
complete task [number] - Mark task as done
delete task [number] - Remove a task
remind me to [task] - Set a reminder

QUIZ:
start quiz - Test your cybersecurity knowledge

ACTIVITY:
activity log - See recent actions

OTHER:
tell me more - Get extra tips
another tip - Get another tip
my name is [name] - Set your name
exit - Close the application

You: add task Update my passwords

Bot: Task added: 'Update my passwords'. Would you like to set a reminder? (say 'remind me to [task] in X days')

You: view tasks

Bot: Your tasks:
1. Update my passwords [PENDING]
Type 'complete task [number]' or 'delete task [number]' to manage.

You: start quiz

Bot: Let's test your cybersecurity knowledge!

Question 1 of 12: What does a firewall do?
A) Installs viruses
B) Monitors and blocks suspicious traffic
C) Creates passwords
D) Deletes files
Type A, B, C, or D to answer.

You: B

Bot: Correct! A firewall acts like a security guard for your network.

Question 2 of 12: Using the same password for multiple accounts is safe.
A) True
B) False
Type A, B, C, or D to answer.

You: B

Bot: Correct! Never reuse passwords. One breach compromises all accounts.

... (quiz continues)

=== QUIZ COMPLETE ===
Score: 10 out of 12 (83%)
Great job! You're a cybersecurity pro!
Type 'menu' to return.

You: activity log

Bot: Recent actions:
1. User Login: User 'John' logged in
2. Task Added: Task: 'Update my passwords'
3. Viewed Tasks: Total: 1 tasks
4. Quiz Started: User began the cybersecurity quiz
5. Quiz Completed: Score: 10/12 (83%)
6. User Asked About Topic: firewall

You: exit

Bot: Goodbye, John! Stay safe online.

## YouTube 
Click here to watch the demo video: https://youtu.be/spc2_NHJuKQ

## Commit History
Initial commit: Set up WPF project structure and files.
Added AudioPlayer class with greeting sound functionality.
Added ChatEngine with response dictionary and topic detection.
Added MainWindow UI with name entry and chat screens.
Added typewriter effect for bot responses.
Added sentiment detection and empathetic responses.
Added follow-up commands for tell me more and another tip.
Added logo image and finalised UI styling.

CyberBot/
├── AudioPlayer/
│   └── greeting.wav          # Welcome sound file
├── Image/
│   └── logo.jpg              # Application logo
├── App.xaml                  # Application definition
├── App.xaml.cs               # Application code-behind
├── AssemblyInfo.cs           # Assembly metadata
├── AudioPlayer.cs            # Handles audio playback
├── ChatEngine.cs             # Bot logic, responses, NLP, and topic detection
├── MainWindow.xaml           # User interface layout
├── MainWindow.xaml.cs        # UI event handlers and display logic
├── TaskManager.cs            # Database operations for tasks and logging
├── QuizManager.cs            # Quiz functionality with 12+ questions
├── Prog.Part3.csproj         # Project file with dependencies
└── SQLQuery3.sql             # Database creation script
