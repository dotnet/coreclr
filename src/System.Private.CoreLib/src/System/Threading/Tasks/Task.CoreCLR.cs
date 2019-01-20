// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.Tracing;

namespace System.Threading.Tasks
{
    public partial class Task : IAsyncResult, IDisposable
    {
        static partial void EtwNewID(int newId)
        {
            TplEtwProvider.Log.NewID(newId);
        }

        partial void EtwTaskStarted(Task previousTask, ref Guid savedActivityID)
        {
            TplEtwProvider etwLog = TplEtwProvider.Log;
            if (etwLog.IsEnabled())
            {
                if (etwLog.TasksSetActivityIds)
                    EventSource.SetCurrentThreadActivityId(TplEtwProvider.CreateGuidForTaskID(this.Id), out savedActivityID);
                // previousTask holds the actual "current task" we want to report in the event
                if (previousTask != null)
                    etwLog.TaskStarted(previousTask.m_taskScheduler.Id, previousTask.Id, this.Id);
                else
                    etwLog.TaskStarted(TaskScheduler.Current.Id, 0, this.Id);
            }
        }

        partial void EtwTaskCompleted(Task previousTask, ref Guid savedActivityID)
        {
            TplEtwProvider etwLog = TplEtwProvider.Log;
            if (etwLog.IsEnabled())
            {
                // previousTask holds the actual "current task" we want to report in the event
                if (previousTask != null)
                    etwLog.TaskCompleted(previousTask.m_taskScheduler.Id, previousTask.Id, this.Id, IsFaulted);
                else
                    etwLog.TaskCompleted(TaskScheduler.Current.Id, 0, this.Id, IsFaulted);

                if (etwLog.TasksSetActivityIds)
                    EventSource.SetCurrentThreadActivityId(savedActivityID);
            }
        }

        partial void EtwTaskWaitBegin()
        {
            TplEtwProvider etwLog = TplEtwProvider.Log;
            if (etwLog.IsEnabled())
            {
                Task currentTask = Task.InternalCurrent;
                etwLog.TaskWaitBegin(
                    (currentTask != null ? currentTask.m_taskScheduler.Id : TaskScheduler.Default.Id), (currentTask != null ? currentTask.Id : 0),
                    this.Id, TplEtwProvider.TaskWaitBehavior.Synchronous, 0);
            }
        }

        partial void EtwTaskWaitEnd()
        {
            TplEtwProvider etwLog = TplEtwProvider.Log;
            if (etwLog.IsEnabled())
            {
                Task currentTask = Task.InternalCurrent;
                if (currentTask != null)
                {
                    etwLog.TaskWaitEnd(currentTask.m_taskScheduler.Id, currentTask.Id, this.Id);
                }
                else
                {
                    etwLog.TaskWaitEnd(TaskScheduler.Default.Id, 0, this.Id);
                }
                // logically the continuation is empty so we immediately fire
                etwLog.TaskWaitContinuationComplete(this.Id);
            }
        }

        partial void EtwTaskScheduled(TaskScheduler ts)
        {
            TplEtwProvider etwLog = TplEtwProvider.Log;
            if (etwLog.IsEnabled() && (m_stateFlags & Task.TASK_STATE_TASKSCHEDULED_WAS_FIRED) == 0)
            {
                m_stateFlags |= Task.TASK_STATE_TASKSCHEDULED_WAS_FIRED;

                Task currentTask = Task.InternalCurrent;
                Task parentTask = m_contingentProperties?.m_parent;
                etwLog.TaskScheduled(ts.Id, currentTask == null ? 0 : currentTask.Id,
                                     this.Id, parentTask == null ? 0 : parentTask.Id, (int)this.Options);
            }
        }

        partial void EtwRunningContinuation(object continuationObject)
        {
            TplEtwProvider etw = TplEtwProvider.Log;
            if (etw.IsEnabled())
            {
                etw.RunningContinuation(Id, continuationObject);
            }
        }

        partial void EtwAwaitTaskContinuationScheduled(Task continuationTask)
        {
            if ((this.Options & (TaskCreationOptions)InternalTaskOptions.PromiseTask) != 0 && !(this is ITaskCompletionAction))
            {
                TplEtwProvider etwLog = TplEtwProvider.Log;
                if (etwLog.IsEnabled())
                {
                    etwLog.AwaitTaskContinuationScheduled(TaskScheduler.Current.Id, Task.CurrentId ?? 0, continuationTask.Id);
                }
            }
        }

        partial void EtwRunningContinuationList(int index, object obj)
        {
            TplEtwProvider etw = TplEtwProvider.Log;
            if (etw.IsEnabled())
            {
                etw.RunningContinuationList(Id, index, obj);
            }
        }
    }
}