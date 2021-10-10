using Microsoft.Win32.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;

namespace AutorunWatcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<Microsoft.Win32.TaskScheduler.Task> Tasks;
        private int CounterRows { get; set; }
        private int CountTasks;
        private WindowState prevState;

        public MainWindow()
        {
            InitializeComponent();
            prevState = Window_.WindowState;
            GetTasksToTable();
            CheckTS();
        }

        private async void CheckTS()
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                CountTasks = GetTasks().Count;
                while (true)
                {
                    Thread.Sleep(1);//300000);
                    int c = GetTasks().Count;
                    if (c != CountTasks)
                    {
                        Dispatcher.Invoke(() => { GetTasksToTable(); });
                        CountTasks = c;
                        MessageBox.Show("Количество задач обновилось!");
                    }
                }
            });
        }

        private List<Microsoft.Win32.TaskScheduler.Task> GetTasks()
        {
            using (TaskService ts = new TaskService())
            {
                return new List<Microsoft.Win32.TaskScheduler.Task>(ts.AllTasks);
            }
        }

        [STAThread]
        public async void GetTasksToTable()
        {
            TasksGrid.ItemsSource = null;

            await System.Threading.Tasks.Task.Run(() =>
            {
                Tasks = new ObservableCollection<Microsoft.Win32.TaskScheduler.Task>(GetTasks());
            });

            TasksGrid.ItemsSource = Tasks.OrderByDescending(i => i.State);
            CounterRows = Tasks.Count;
            StatusCounterRows.Text = $"Количество строк равно {CounterRows}";
        }

        private void Button_Click_Refresh(object sender, RoutedEventArgs e)
        {
            GetTasks();
        }

        private void Button_Click_Stop(object sender, RoutedEventArgs e)
        {
            var selected_item = TasksGrid.SelectedItem as Microsoft.Win32.TaskScheduler.Task;
            if (selected_item != null)
            {
                selected_item.Stop();
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                Hide();
            else
                prevState = WindowState;
        }

        private void TaskIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            Show();
            Window_.WindowState = prevState;
        }
    }
}