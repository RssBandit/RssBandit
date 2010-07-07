using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Threading;

namespace RssBandit.WinGui.Forms
{
    public partial class MainWindow
    {
        private class UITaskTimer : DispatcherTimer
        {
            private readonly object SynRoot = new object();
            private DelayedTasks tasks;
            private readonly Dictionary<DelayedTasks, object> taskData = new Dictionary<DelayedTasks, object>(7);
        

            public bool this[DelayedTasks task]
            {
                get
                {
                    lock (SynRoot)
                    {
                        if ((tasks & task) == task)
                            return true;
                        return false;
                    }
                }
                set
                {
                    lock (SynRoot)
                    {
                        if (value)
                            tasks |= task;
                        else
                            tasks ^= task;
                    }
                }
            }

            public void StartTask(DelayedTasks task)
            {
                lock (SynRoot)
                {
                    this[task] = true;
                    //if (!base.Enabled)
                    Stop();
                    Start();
                }
            }

            public void StopTask(DelayedTasks task)
            {
                lock (SynRoot)
                {
                    this[task] = false;
                    if (AllTaskDone)
                        Stop();
                }
            }

            public bool AllTaskDone
            {
                get
                {
                    lock (SynRoot)
                    {
                        return (tasks == DelayedTasks.None);
                    }
                }
            }


            public object GetData(DelayedTasks task, bool clear)
            {
                object data = null;
                lock (SynRoot)
                {
                    if (taskData.ContainsKey(task))
                    {
                        data = taskData[task];
                        if (clear)
                            taskData.Remove(task);
                    }
                }
                return data;
            }

            public void SetData(DelayedTasks task, object data)
            {
                lock (SynRoot)
                {
                    if (taskData.ContainsKey(task))
                        taskData.Remove(task);
                    taskData.Add(task, data);
                }
            }
        }
    }
}