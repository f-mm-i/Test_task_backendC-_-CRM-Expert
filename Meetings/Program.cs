using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Timers;


namespace Meetings
{

    class Meeting
    {
        public string Title { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Reminder { get; set; }
        public bool IsReminderSet { get; set; } = false;

        public Meeting(string title, DateTime startTime, DateTime endTime, TimeSpan reminder)
        {
            Title = title;
            StartTime = startTime;
            EndTime = endTime;
            Reminder = reminder;
        }

        public override string ToString()
        {
            return $"{Title}: {StartTime} - {EndTime}, Reminder: {Reminder.TotalMinutes} mins before";
        }
    }

    class MeetingManager
    {
        private List<Meeting> meetings = new List<Meeting>();
        private Timer reminderTimer = new Timer(60000); // Таймер проверяет каждую минуту

        public MeetingManager()
        {
            reminderTimer.Elapsed += CheckReminders;
            reminderTimer.Start();
        }

        public void AddMeeting(string title, DateTime startTime, DateTime endTime, TimeSpan reminder)
        {
            if (startTime < DateTime.Now)
            {
                Console.WriteLine("Ошибка: Нельзя планировать встречи на прошедшее время.");
                return;
            }

            if (IsOverlapping(startTime, endTime))
            {
                Console.WriteLine("Ошибка: Время пересекается с другой встречей.");
                return;
            }

            var meeting = new Meeting(title, startTime, endTime, reminder);
            meetings.Add(meeting);
            Console.WriteLine("Встреча успешно добавлена!");
        }

        public void EditMeeting(int index, string title, DateTime startTime, DateTime endTime, TimeSpan reminder)
        {
            if (index < 0 || index >= meetings.Count)
            {
                Console.WriteLine("Ошибка: Неверный индекс.");
                return;
            }

            if (startTime < DateTime.Now)
            {
                Console.WriteLine("Ошибка: Нельзя планировать встречи на прошедшее время.");
                return;
            }

            meetings[index].Title = title;
            meetings[index].StartTime = startTime;
            meetings[index].EndTime = endTime;
            meetings[index].Reminder = reminder;
            Console.WriteLine("Встреча успешно обновлена!");
        }

        public void DeleteMeeting(int index)
        {
            if (index < 0 || index >= meetings.Count)
            {
                Console.WriteLine("Ошибка: Неверный индекс.");
                return;
            }

            meetings.RemoveAt(index);
            Console.WriteLine("Встреча удалена.");
        }

        public void ViewMeetings(DateTime date)
        {
            var dailyMeetings = meetings.Where(m => m.StartTime.Date == date.Date).ToList();

            if (dailyMeetings.Count == 0)
            {
                Console.WriteLine("На выбранный день нет запланированных встреч.");
            }
            else
            {
                Console.WriteLine($"Встречи на {date.ToShortDateString()}:");
                foreach (var meeting in dailyMeetings)
                {
                    Console.WriteLine(meeting);
                }
            }
        }

        public void ExportMeetings(DateTime date)
        {
            var dailyMeetings = meetings.Where(m => m.StartTime.Date == date.Date).ToList();
            string fileName = $"Meetings_{date:yyyyMMdd}.txt";

            using (StreamWriter writer = new StreamWriter(fileName))
            {
                writer.WriteLine($"Встречи на {date.ToShortDateString()}:");
                foreach (var meeting in dailyMeetings)
                {
                    writer.WriteLine(meeting);
                }
            }

            Console.WriteLine($"Расписание встреч экспортировано в файл {fileName}");
        }

        private bool IsOverlapping(DateTime startTime, DateTime endTime)
        {
            return meetings.Any(m => startTime < m.EndTime && endTime > m.StartTime);
        }

        private void CheckReminders(object sender, ElapsedEventArgs e)
        {
            foreach (var meeting in meetings)
            {
                if (!meeting.IsReminderSet && DateTime.Now >= meeting.StartTime - meeting.Reminder)
                {
                    Console.WriteLine($"Напоминание: Встреча '{meeting.Title}' начнется в {meeting.StartTime}");
                    meeting.IsReminderSet = true;
                }
            }
        }
    }
    public class Program
    {
        static void Main(string[] args)
        {
            MeetingManager manager = new MeetingManager();
            while (true)
            {
                Console.WriteLine("\nМеню:");
                Console.WriteLine("1. Добавить встречу");
                Console.WriteLine("2. Редактировать встречу");
                Console.WriteLine("3. Удалить встречу");
                Console.WriteLine("4. Просмотреть встречи на день");
                Console.WriteLine("5. Экспортировать встречи на день");
                Console.WriteLine("0. Выйти");

                if (!int.TryParse(Console.ReadLine(), out int choice))
                {
                    Console.WriteLine("Ошибка: введите число.");
                    continue;
                }

                switch (choice)
                {
                    case 1:
                        Console.Write("Название: ");
                        string title = Console.ReadLine();
                        DateTime start = PromptDateTime("Дата и время начала (гггг-ММ-дд чч:мм): ");
                        DateTime end = PromptDateTime("Дата и время окончания (гггг-ММ-дд чч:мм): ");
                        int reminderMinutes = PromptInt("Напоминание (мин): ");

                        manager.AddMeeting(title, start, end, TimeSpan.FromMinutes(reminderMinutes));
                        break;
                    case 2:
                        Console.Write("Введите индекс встречи: ");
                        int editIndex = PromptInt("Индекс встречи: ");
                        Console.Write("Новое название: ");
                        title = Console.ReadLine();
                        start = PromptDateTime("Новая дата и время начала (гггг-ММ-дд чч:мм): ");
                        end = PromptDateTime("Новая дата и время окончания (гггг-ММ-дд чч:мм): ");
                        reminderMinutes = PromptInt("Новое напоминание (мин): ");

                        manager.EditMeeting(editIndex, title, start, end, TimeSpan.FromMinutes(reminderMinutes));
                        break;
                    case 3:
                        int deleteIndex = PromptInt("Введите индекс встречи для удаления: ");
                        manager.DeleteMeeting(deleteIndex);
                        break;
                    case 4:
                        DateTime viewDate = PromptDate("Введите дату (гггг-ММ-дд): ");
                        manager.ViewMeetings(viewDate);
                        break;
                    case 5:
                        DateTime exportDate = PromptDate("Введите дату (гггг-ММ-дд): ");
                        manager.ExportMeetings(exportDate);
                        break;
                    case 0:
                        return;
                    default:
                        Console.WriteLine("Некорректный выбор. Попробуйте снова.");
                        break;
                }
            }
        }

        private static DateTime PromptDateTime(string prompt)
        {
            DateTime dateTime;
            while (true)
            {
                Console.Write(prompt);
                if (DateTime.TryParse(Console.ReadLine(), out dateTime))
                {
                    return dateTime;
                }
                Console.WriteLine("Ошибка: неверный формат даты и времени.");
            }
        }

        private static DateTime PromptDate(string prompt)
        {
            DateTime date;
            while (true)
            {
                Console.Write(prompt);
                if (DateTime.TryParse(Console.ReadLine(), out date))
                {
                    return date;
                }
                Console.WriteLine("Ошибка: неверный формат даты.");
            }
        }

        private static int PromptInt(string prompt)
        {
            int result;
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out result))
                {
                    return result;
                }
                Console.WriteLine("Ошибка: введите корректное число.");
            }
        }
    }
}
